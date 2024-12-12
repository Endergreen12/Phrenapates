using Plana.MX.GameLogic.DBModel;
using Plana.FlatData;
using Plana.Database;
using Phrenapates.Utils;

namespace Phrenapates.Managers
{
    public class EliminateRaidManager : Singleton<EliminateRaidManager>
    {
        public EliminateRaidLobbyInfoDB EliminateRaidLobbyInfoDB { get; private set; }

        public RaidDB RaidDB { get; private set; }

        public RaidBattleDB RaidBattleDB { get; private set; }

        public EliminateRaidLobbyInfoDB GetLobby(ContentInfo raidInfo, EliminateRaidSeasonManageExcelT targetSeasonData)
        {
            if (EliminateRaidLobbyInfoDB == null || EliminateRaidLobbyInfoDB.SeasonId != raidInfo.EliminateRaidDataInfo.SeasonId)
            {
                EliminateRaidLobbyInfoDB = new EliminateRaidLobbyInfoDB()
                {
                    Tier = 4,
                    Ranking = 1,
                    SeasonId = raidInfo.EliminateRaidDataInfo.SeasonId,
                    BestRankingPoint = 0,
                    TotalRankingPoint = 0,
                    BestRankingPointPerBossGroup = [],
                    ReceiveRewardIds = targetSeasonData.SeasonRewardId,
                    OpenedBossGroups = [],
                    PlayableHighestDifficulty = new()
                    {
                        { targetSeasonData.OpenRaidBossGroup01, Difficulty.Torment },
                        { targetSeasonData.OpenRaidBossGroup02, Difficulty.Torment },
                        { targetSeasonData.OpenRaidBossGroup03, Difficulty.Torment }
                    },
                    SweepPointByRaidUniqueId = [],
                    SeasonStartDate = DateTime.Now.AddHours(-1),
                    SeasonEndDate = DateTime.Now.AddDays(7),
                    SettlementEndDate = DateTime.Now.AddDays(8),
                    NextSeasonId = 300,
                    NextSeasonStartDate = DateTime.Now.AddMonths(1),
                    NextSeasonEndDate = DateTime.Now.AddMonths(1).AddDays(7),
                    NextSettlementEndDate = DateTime.Now.AddMonths(1).AddDays(8),
                    RemainFailCompensation = new() { 
                        { 0, true },
                        { 1, true },
                        { 2, true },
                    }
                };
            }
            
            else
            {
                EliminateRaidLobbyInfoDB.BestRankingPoint = raidInfo.EliminateRaidDataInfo.BestRankingPoint;
                EliminateRaidLobbyInfoDB.TotalRankingPoint = raidInfo.EliminateRaidDataInfo.TotalRankingPoint;
            }

            return EliminateRaidLobbyInfoDB;
        }

        public RaidDB CreateRaid(
            ContentInfo raidInfo,
            long ownerId, string ownerNickname, int ownerLevel, long characterId,
            bool isPractice, long raidId, long currentHp)
        {
            if (RaidDB == null)
            {
                RaidDB = new()
                {
                    Owner = new()
                    {
                        AccountId = ownerId,
                        AccountName = ownerNickname,
                        CharacterId = characterId
                    },
                    ContentType = ContentType.EliminateRaid,
                    RaidState = RaidStatus.Playing,
                    SeasonId = EliminateRaidLobbyInfoDB.SeasonId,
                    UniqueId = raidId,
                    ServerId = 1,
                    SecretCode = "0",
                    Begin = DateTime.Now,
                    End = DateTime.Now.AddHours(1),
                    PlayerCount = 1,
                    IsPractice = isPractice,
                    AccountLevelWhenCreateDB = ownerLevel,
                    RaidBossDBs = [
                        new RaidBossDB()
                        {
                            ContentType = ContentType.EliminateRaid,
                            BossCurrentHP = currentHp,
                        }
                    ],
                };
            }

            else
            {
                RaidDB.BossDifficulty = raidInfo.RaidDataInfo.CurrentDifficulty;
                RaidDB.UniqueId = raidId;
                RaidDB.IsPractice = isPractice;
            }

            EliminateRaidLobbyInfoDB.PlayingRaidDB = RaidDB;

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
                    ContentType = ContentType.EliminateRaid,
                    RaidUniqueId = raidId,
                    CurrentBossHP = bossHp,
                    RaidMembers = [
                        new() {
                            AccountId = ownerId,
                            AccountName = ownerNickname,
                            CharacterId = characterId
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

        public EliminateRaidLobbyInfoDB SaveBattle(long keyId, List<long> characterId, long currentBossHP, long groggyPoint, int bossIndex)
        {
            var RaidBossDB = new RaidBossDB()
            {
                ContentType = ContentType.EliminateRaid,
                BossCurrentHP = currentBossHP,
                BossGroggyPoint = groggyPoint,
                BossIndex = bossIndex
            };
            RaidDB.RaidBossDBs.Clear();
            RaidDB.RaidBossDBs.Add(RaidBossDB);
            EliminateRaidLobbyInfoDB.PlayingRaidDB.RaidBossDBs = RaidDB.RaidBossDBs;
            if (EliminateRaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds == null) EliminateRaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds = new();
            
            if (EliminateRaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds.ContainsKey(keyId))
            {
                EliminateRaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds[keyId].AddRange(characterId);
                EliminateRaidLobbyInfoDB.ParticipateCharacterServerIds.AddRange(characterId);
            }
            else
            {
                EliminateRaidLobbyInfoDB.PlayingRaidDB.ParticipateCharacterServerIds[keyId] = characterId;
                EliminateRaidLobbyInfoDB.ParticipateCharacterServerIds = characterId;
            }
            return EliminateRaidLobbyInfoDB;
        }

        public void ClearPlayingBossDB()
        {
            RaidDB = null;
            EliminateRaidLobbyInfoDB = null;
            RaidBattleDB = null;
        }
    }
}
