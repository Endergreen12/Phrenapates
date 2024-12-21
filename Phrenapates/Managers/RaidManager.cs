using Phrenapates.Services;
using Phrenapates.Utils;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.Logic.Battles.Summary;

namespace Phrenapates.Managers
{
    public class RaidManager : Singleton<RaidManager>
    {
        public SingleRaidLobbyInfoDB RaidLobbyInfoDB { get; private set; }
        public RaidDB RaidDB { get; private set; }
        public RaidBattleDB RaidBattleDB { get; private set; }

        // Track boss data and time.
        public long SeasonId { get; private set; }
        public DateTime OverrideServerTimeTicks { get; private set; }
        public List<long> BossCharacterIds { get; private set; }

        public DateTime CreateServerTime(RaidSeasonManageExcelT targetSeason, ContentInfo contentInfo)
        {
            if (OverrideServerTimeTicks == null || SeasonId != contentInfo.RaidDataInfo.SeasonId)
            {
                OverrideServerTimeTicks = DateTime.Parse(targetSeason.SeasonStartData);
                SeasonId = contentInfo.RaidDataInfo.SeasonId;
            }
            return OverrideServerTimeTicks;
        }

        public DateTime GetServerTime() => OverrideServerTimeTicks;
        public SingleRaidLobbyInfoDB GetLobby(ContentInfo raidInfo, RaidSeasonManageExcelT targetSeasonData)
        {
            if (RaidLobbyInfoDB == null || RaidLobbyInfoDB.SeasonId != raidInfo.RaidDataInfo.SeasonId)
            {
                ClearBossData();
                RaidLobbyInfoDB = new SingleRaidLobbyInfoDB()
                {
                    Tier = 4,
                    Ranking = 1,
                    SeasonId = raidInfo.RaidDataInfo.SeasonId,
                    BestRankingPoint = 0,
                    TotalRankingPoint = 0,
                    ReceiveRewardIds = targetSeasonData.SeasonRewardId,
                    PlayableHighestDifficulty = new()
                    {
                        { targetSeasonData.OpenRaidBossGroup.FirstOrDefault(), Difficulty.Torment }
                    },
                    SweepPointByRaidUniqueId = [],
                    SeasonStartDate = OverrideServerTimeTicks.AddHours(-1),
                    SeasonEndDate = OverrideServerTimeTicks.AddDays(7),
                    SettlementEndDate = OverrideServerTimeTicks.AddDays(8),
                    NextSeasonId = 999,
                    NextSeasonStartDate = OverrideServerTimeTicks.AddMonths(1),
                    NextSeasonEndDate = OverrideServerTimeTicks.AddMonths(1).AddDays(7),
                    NextSettlementEndDate = OverrideServerTimeTicks.AddMonths(1).AddDays(8),
                    RemainFailCompensation = new() { { 0, true } },
                    ReceivedRankingRewardId = new(),
                    ReceiveLimitedRewardIds = new(),
                    CanReceiveRankingReward = new(),
                };
            } 
            
            else
            {
                RaidLobbyInfoDB.BestRankingPoint = raidInfo.RaidDataInfo.BestRankingPoint;
                RaidLobbyInfoDB.TotalRankingPoint = raidInfo.RaidDataInfo.TotalRankingPoint;
            }

            return RaidLobbyInfoDB;
        }

        public RaidDB CreateRaid(
            ContentInfo raidInfo,
            long ownerId, string ownerNickname, int ownerLevel, long characterId,
            bool isPractice, long raidId,
            RaidStageExcelT currentRaidData, List<CharacterStatExcelT> characterStatExcel
        )
        {
            if (RaidDB == null)
            {
                List<RaidBossDB> raidBossDBs = currentRaidData.BossCharacterId.Select((x, index) => {
                    return new RaidBossDB()
                    {
                        ContentType = ContentType.Raid,
                        BossCurrentHP = characterStatExcel.FirstOrDefault(y => y.CharacterId == x).MaxHP100,
                        BossGroggyPoint = 0,
                        BossIndex = index
                    };
                }).ToList();

                BossCharacterIds ??= currentRaidData.BossCharacterId;

                RaidDB = new()
                {
                    Owner = new()
                    {
                        AccountId = ownerId,
                        AccountName = ownerNickname,
                        CharacterId = characterId
                    },
                    ContentType = ContentType.Raid,
                    RaidState = RaidStatus.Playing,
                    SeasonId = RaidLobbyInfoDB.SeasonId,
                    UniqueId = raidId,
                    ServerId = 1,
                    SecretCode = "0",
                    Begin = OverrideServerTimeTicks,
                    End = OverrideServerTimeTicks.AddHours(1),
                    PlayerCount = 1,
                    IsPractice = isPractice,
                    AccountLevelWhenCreateDB = ownerLevel,
                    RaidBossDBs = raidBossDBs,
                };
            }

            else
            {
                RaidDB.BossDifficulty = raidInfo.RaidDataInfo.CurrentDifficulty;
                RaidDB.UniqueId = raidId;
                RaidDB.IsPractice = isPractice;
            }

            RaidLobbyInfoDB.PlayingRaidDB = RaidDB;

            return RaidDB;
        }

        public RaidBattleDB CreateBattle(
            long ownerId, string ownerNickname, long characterId,
            long raidId, long bossHp
        )
        {
            if (RaidBattleDB == null)
            {
                RaidBattleDB = new()
                {
                    ContentType = ContentType.Raid,
                    RaidUniqueId = raidId,
                    CurrentBossHP = bossHp,
                    RaidMembers = [
                        new() {
                            AccountId = ownerId,
                            AccountName = ownerNickname,
                            CharacterId = characterId,
                        }
                    ],
                };
            }

            else
            {
                RaidBattleDB.RaidUniqueId = raidId;
            }

            return RaidBattleDB;
        }

        public bool SaveBattle(
            long keyId, BattleSummary summary,
            RaidStageExcelT raidStageExcel, List<CharacterStatExcelT> characterStatExcels
        )
        {
            RaidBattleDB.RaidMembers.FirstOrDefault().DamageCollection = new();
            foreach(var raidDamage in summary.RaidSummary.RaidBossResults)
            {
                RaidBattleDB.RaidMembers.FirstOrDefault().DamageCollection.Add(
                    RaidService.CreateRaidCollection(raidDamage.RaidDamage)
                );
            }

            foreach (var bossResult in summary.RaidSummary.RaidBossResults)
            {
                var characterStat = characterStatExcels.FirstOrDefault(x => x.CharacterId == BossCharacterIds[bossResult.RaidDamage.Index]);

                // Calculate updated HP and Groggy points
                long hpLeft = RaidDB.RaidBossDBs[bossResult.RaidDamage.Index].BossCurrentHP - bossResult.RaidDamage.GivenDamage;
                long groggyPoint = RaidService.CalculateGroggyAccumulation(
                    RaidDB.RaidBossDBs[bossResult.RaidDamage.Index].BossGroggyPoint + bossResult.RaidDamage.GivenGroggyPoint, 
                    characterStat
                );

                if (hpLeft <= 0)
                {
                    // Boss defeated
                    Console.WriteLine("Boss defeated");
                    RaidDB.RaidBossDBs[bossResult.RaidDamage.Index].BossCurrentHP = default;
                    RaidDB.RaidBossDBs[bossResult.RaidDamage.Index].BossGroggyPoint = groggyPoint;

                    int nextBossIndex = bossResult.RaidDamage.Index + 1;
                    if (nextBossIndex < RaidDB.RaidBossDBs.Count)
                    {
                        // Move to the next boss
                        Console.WriteLine("Move to the next boss");
                        var nextBoss = RaidDB.RaidBossDBs[nextBossIndex];
                        RaidBattleDB.CurrentBossHP = nextBoss.BossCurrentHP;
                        RaidBattleDB.CurrentBossGroggy = 0;
                        RaidBattleDB.CurrentBossAIPhase = 1;
                        RaidBattleDB.SubPartsHPs = bossResult.SubPartsHPs;
                        RaidBattleDB.RaidBossIndex = nextBossIndex;
                    }
                    else
                    {
                        // Raid complete
                        Console.WriteLine("Raid complete");
                        RaidBattleDB.CurrentBossHP = 0;
                        RaidBattleDB.CurrentBossGroggy = groggyPoint;
                        RaidBattleDB.CurrentBossAIPhase = bossResult.AIPhase;
                        RaidBattleDB.SubPartsHPs = bossResult.SubPartsHPs;
                    }
                }
                else
                {
                    // Boss not defeated
                    Console.WriteLine("Boss not defeated");
                    RaidDB.RaidBossDBs[bossResult.RaidDamage.Index].BossCurrentHP = hpLeft;
                    RaidDB.RaidBossDBs[bossResult.RaidDamage.Index].BossGroggyPoint = groggyPoint;

                    RaidBattleDB.CurrentBossHP = hpLeft;
                    RaidBattleDB.CurrentBossGroggy = groggyPoint;
                    RaidBattleDB.CurrentBossAIPhase = 1;
                    RaidBattleDB.SubPartsHPs = bossResult.SubPartsHPs;
                }
            }
            RaidLobbyInfoDB.PlayingRaidDB.RaidBossDBs = RaidDB.RaidBossDBs;

            // Disabled for now until futher update on assistant character
            /*List<long> characterId = RaidService.CharacterParticipation(summary.Group01Summary);
            if (RaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds == null) RaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds = new();
            
            if (RaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds.ContainsKey(keyId))
            {
                RaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds[keyId].AddRange(characterId);
                RaidLobbyInfoDB.ParticipateCharacterServerIds.AddRange(characterId);
            }
            else
            {
                RaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds[keyId] = characterId;
                RaidLobbyInfoDB.ParticipateCharacterServerIds = characterId;
            }*/

            if (RaidDB.RaidBossDBs.All(x => x.BossCurrentHP == 0)) return true;
            else return false;
        }

        public void ClearBossData()
        {
            RaidDB = null;
            RaidLobbyInfoDB = null;
            RaidBattleDB = null;
            BossCharacterIds = null;
        }
    }
}