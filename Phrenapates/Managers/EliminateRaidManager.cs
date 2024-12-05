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

        public EliminateRaidLobbyInfoDB GetLobby(RaidInfo raidInfo, EliminateRaidSeasonManageExcelT targetSeasonData)
        {
            if (EliminateRaidLobbyInfoDB == null || EliminateRaidLobbyInfoDB.SeasonId != raidInfo.EliminateRaidDataInfo.SeasonId)
            {
                EliminateRaidLobbyInfoDB = new EliminateRaidLobbyInfoDB()
                {
                    Tier = 0,
                    Ranking = 1,
                    SeasonId = raidInfo.EliminateRaidDataInfo.SeasonId,
                    BestRankingPoint = 0,
                    TotalRankingPoint = 0,
                    BestRankingPointPerBossGroup = [],
                    ReceiveRewardIds = targetSeasonData.SeasonRewardId,
                    OpenedBossGroups = [
                        targetSeasonData.OpenRaidBossGroup01.FirstOrDefault().ToString(),
                        targetSeasonData.OpenRaidBossGroup02.FirstOrDefault().ToString(),
                        targetSeasonData.OpenRaidBossGroup03.FirstOrDefault().ToString()
                    ],
                    PlayableHighestDifficulty = new()
                    {
                        { targetSeasonData.OpenRaidBossGroup01, Difficulty.Torment },
                        { targetSeasonData.OpenRaidBossGroup02, Difficulty.Torment },
                        { targetSeasonData.OpenRaidBossGroup03, Difficulty.Torment }
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

        public RaidDB CreateRaid(RaidInfo raidInfo, long ownerId, string ownerNickname, bool isPractice, long raidId)
        {
            if (RaidDB == null)
            {
                RaidDB = new()
                {
                    Owner = new()
                    {
                        AccountId = ownerId,
                        AccountName = ownerNickname,
                    },

                    ContentType = ContentType.Raid,
                    UniqueId = raidId,
                    SeasonId = raidInfo.EliminateRaidDataInfo.SeasonId,
                    RaidState = RaidStatus.Playing,
                    IsPractice = isPractice,
                    BossDifficulty = raidInfo.EliminateRaidDataInfo.CurrentDifficulty,
                    RaidBossDBs = [
                        new() {
                            ContentType = ContentType.Raid,
                            BossCurrentHP = long.MaxValue
                        }
                    ],
                };
            }

            else
            {
                RaidDB.BossDifficulty = raidInfo.EliminateRaidDataInfo.CurrentDifficulty;
                RaidDB.UniqueId = raidId;
                RaidDB.IsPractice = isPractice;
            }

            return RaidDB;
        }

        public RaidBattleDB CreateBattle(long ownerId, string ownerNickname, long raidId)
        {
            if (RaidBattleDB == null)
            {
                RaidBattleDB = new()
                {
                    ContentType = ContentType.Raid,
                    RaidUniqueId = raidId,
                    CurrentBossHP = long.MaxValue,
                    RaidMembers = [
                        new() {
                            AccountId = ownerId,
                            AccountName = ownerNickname,
                        }
                    ]
                };
            }

            else
            {
                RaidBattleDB.RaidUniqueId = raidId;
            }

            return RaidBattleDB;
        }
    }
}
