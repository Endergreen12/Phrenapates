using Plana.FlatData;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("setseason", "Set season of content (raid, timeattackdungeon, eliminateraid, arena)", "/setseason <raid|timeattackdungeon|eliminateraid|arena> <seasonid>")]
    internal class SetSeason : Command
    {
        public SetSeason(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"", "Target content name (raid, timeattackdungeon, eliminateraid, arena)", ArgumentFlags.IgnoreCase)]
        public string target { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]", "Target season id", ArgumentFlags.IgnoreCase)]
        public string value { get; set; } = string.Empty;

        public override void Execute()
        {
            var account = connection.Account;

            long seasonId = 0;
            switch (target)
            {
                case "raid":
                    if (long.TryParse(value, out seasonId))
                    {
                        var raidSeason = connection.ExcelTableService.GetTable<RaidSeasonManageExcelTable>().UnPack().DataList.FirstOrDefault(x => x.SeasonId == seasonId);
                        connection.Account.ContentInfo.RaidDataInfo.SeasonId = seasonId;
                        connection.Account.ContentInfo.RaidDataInfo.BestRankingPoint = 0;
                        connection.Account.ContentInfo.RaidDataInfo.TotalRankingPoint = 0;

                        connection.SendChatMessage($"Total Assault Boss: {string.Join(", ", raidSeason.OpenRaidBossGroup)}");
                        connection.SendChatMessage($"Total Assault ID: {raidSeason.SeasonId}");
                        connection.SendChatMessage($"Total Assault StartTime: {raidSeason.SeasonStartData}");
                        connection.SendChatMessage($"Total Assault Raid is set to {seasonId}");
                        connection.Context.SaveChanges();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Value");
                    }
                    break;
                case "timeattackdungeon":
                    if (long.TryParse(value, out seasonId))
                    {
                        var TADSeasonData = connection.ExcelTableService.GetTable<TimeAttackDungeonSeasonManageExcelTable>().UnPack().DataList.FirstOrDefault(x => x.Id == seasonId);
                        var TADExcel = connection.ExcelTableService.GetTable<TimeAttackDungeonExcelTable>().UnPack().DataList.FirstOrDefault(x => x.Id == TADSeasonData.DungeonId);
                        connection.Account.ContentInfo.TimeAttackDungeonDataInfo.SeasonId = seasonId;
                        connection.Account.ContentInfo.TimeAttackDungeonDataInfo.SeasonBestRecord = 0;

                        connection.SendChatMessage($"Joint Firing Drill Type: {string.Join(", ", TADExcel.TimeAttackDungeonType)}");
                        connection.SendChatMessage($"Joint Firing Drill ID: {TADSeasonData.Id}");
                        connection.SendChatMessage($"Joint Firing Drill StartTime: {TADSeasonData.StartDate}");
                        connection.SendChatMessage($"Joint Firing Drill is set to {seasonId}");
                        connection.Context.SaveChanges();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Value");
                    }
                    break;
                case "eliminateraid":
                    if (long.TryParse(value, out seasonId))
                    {
                        var eliminateRaidSeason = connection.ExcelTableService.GetTable<EliminateRaidSeasonManageExcelTable>().UnPack().DataList.FirstOrDefault(x => x.SeasonId == seasonId);
                        connection.Account.ContentInfo.EliminateRaidDataInfo.SeasonId = seasonId;
                        connection.Account.ContentInfo.EliminateRaidDataInfo.BestRankingPoint = 0;
                        connection.Account.ContentInfo.EliminateRaidDataInfo.TotalRankingPoint = 0;
                        
                        List<string> raidBoss = [
                            eliminateRaidSeason.OpenRaidBossGroup01,
                            eliminateRaidSeason.OpenRaidBossGroup02,
                            eliminateRaidSeason.OpenRaidBossGroup03
                        ];
                        connection.SendChatMessage($"All Grand Assault Boss: {string.Join(", ", raidBoss)}");
                        connection.SendChatMessage($"Grand Assault ID: {eliminateRaidSeason.SeasonId}");
                        connection.SendChatMessage($"Grand Assault StartTime: {eliminateRaidSeason.SeasonStartData}");
                        connection.SendChatMessage($"Grand Assault Raid is set to {seasonId}");
                        connection.Context.SaveChanges();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Value");
                    }
                    break;
                case "arena":
                    connection.SendChatMessage($"PVP command isn't implemented yet!");
                    connection.Context.SaveChanges();
                    break;
                case "multifloorraid":
                    connection.SendChatMessage($"Final Restriction command isn't implemented yet!");
                    connection.Context.SaveChanges();
                    break;
                default:
                    throw new ArgumentException("Invalid target!");
            }
        }
    }
}
