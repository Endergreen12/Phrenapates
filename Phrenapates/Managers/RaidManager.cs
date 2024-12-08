using System;
using System.Text.Json;
using Plana.MX.GameLogic.DBModel;
using Plana.FlatData;
using Plana.Database;
using Phrenapates.Utils;

namespace Phrenapates.Managers
{
    public class RaidManager : Singleton<RaidManager>
    {
        public SingleRaidLobbyInfoDB RaidLobbyInfoDB { get; private set; }

        public RaidDB RaidDB { get; private set; }

        public RaidBattleDB RaidBattleDB { get; private set; }

        public SingleRaidLobbyInfoDB GetLobby(ContentInfo raidInfo, RaidSeasonManageExcelT targetSeasonData)
        {
            if (RaidLobbyInfoDB == null || RaidLobbyInfoDB.SeasonId != raidInfo.RaidDataInfo.SeasonId)
            {
                RaidLobbyInfoDB = new SingleRaidLobbyInfoDB()
                {
                    Tier = 0,
                    Ranking = 1,
                    SeasonId = raidInfo.RaidDataInfo.SeasonId,
                    BestRankingPoint = 0,
                    TotalRankingPoint = 0,
                    ReceiveRewardIds = targetSeasonData.SeasonRewardId,
                    PlayableHighestDifficulty = new()
                    {
                        { targetSeasonData.OpenRaidBossGroup.FirstOrDefault(), Difficulty.Torment }
                    },
                    ClanAssistUseInfo = new(),
                    SeasonStartDate = DateTime.Now.AddHours(-1),
                    SeasonEndDate = DateTime.Now.AddDays(7),
                    SettlementEndDate = DateTime.Now.AddDays(8),
                    NextSeasonId = 300,
                    NextSeasonStartDate = DateTime.Now.AddMonths(1),
                    NextSeasonEndDate = DateTime.Now.AddMonths(1).AddDays(7),
                    NextSettlementEndDate = DateTime.Now.AddMonths(1).AddDays(8),
                    RemainFailCompensation = new() { { 0, true } }
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
                    ContentType = ContentType.Raid,
                    RaidState = RaidStatus.Playing,
                    SeasonId = RaidLobbyInfoDB.SeasonId,
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
                            ContentType = ContentType.Raid,
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

        public RaidLobbyInfoDB SaveBattle(long keyId, List<long> characterId, long currentBossHP, long groggyPoint, int bossIndex)
        {
            var RaidBossDB = new RaidBossDB()
            {
                ContentType = ContentType.Raid,
                BossCurrentHP = currentBossHP,
                BossGroggyPoint = groggyPoint,
                BossIndex = bossIndex
            };
            RaidDB.RaidBossDBs.Clear();
            RaidDB.RaidBossDBs.Add(RaidBossDB);
            RaidLobbyInfoDB.PlayingRaidDB.RaidBossDBs = RaidDB.RaidBossDBs;
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
            }
            Console.WriteLine(JsonSerializer.Serialize(RaidLobbyInfoDB));
            return RaidLobbyInfoDB;
        }

        public void ClearPlayingBossDB()
        {
            RaidDB = null;
            RaidLobbyInfoDB.PlayingRaidDB = RaidDB;
        }
    }
}