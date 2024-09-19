using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Controllers.Api.ProtocolHandlers;
using Phrenapates.Utils;

namespace Phrenapates.Managers
{
    public class RaidManager : Singleton<RaidManager>
    {
        public SingleRaidLobbyInfoDB RaidLobbyInfoDB { get; private set; }

        public RaidDB RaidDB {  get; private set; }

        public RaidBattleDB RaidBattleDB { get; private set; }

        public SingleRaidLobbyInfoDB GetLobby(RaidInfo raidInfo, RaidSeasonManageExcelT targetSeasonData)
        {
            if (RaidLobbyInfoDB == null || RaidLobbyInfoDB.SeasonId != raidInfo.SeasonId)
            {
                RaidLobbyInfoDB = new SingleRaidLobbyInfoDB()
                {
                    Tier = 0,
                    Ranking = 1,
                    SeasonId = raidInfo.SeasonId,
                    BestRankingPoint = 0,
                    TotalRankingPoint = 0,
                    ReceiveRewardIds = targetSeasonData.SeasonRewardId,
                    PlayableHighestDifficulty = new()
                    {
                        { targetSeasonData.OpenRaidBossGroup.FirstOrDefault(), Difficulty.Torment }
                    }
                };
            } 
            
            else
            {
                RaidLobbyInfoDB.BestRankingPoint = raidInfo.BestRankingPoint;
                RaidLobbyInfoDB.TotalRankingPoint = raidInfo.TotalRankingPoint;
            }

            return RaidLobbyInfoDB;
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
                    SeasonId = raidInfo.SeasonId,
                    RaidState = RaidStatus.Playing,
                    IsPractice = isPractice,
                    BossDifficulty = raidInfo.CurrentDifficulty,
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
                RaidDB.BossDifficulty = raidInfo.CurrentDifficulty;
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

        public static long CalculateTimeScore(float duration, Difficulty difficulty)
        {
            int[] multipliers = [120, 240, 480, 960, 1440, 1920, 2400]; // from wiki

            return (long)((3600f - duration) * multipliers[(int)difficulty]);
        }
    }
}
