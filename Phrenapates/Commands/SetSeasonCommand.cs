using Plana.Database;
using Plana.FlatData;
using Plana.Utils;
using Phrenapates.Services;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("setseason", "Set season of content (raid, arena)", "/setseason <raid|arena> <seasonid>")]
    internal class SetSeason : Command
    {
        public SetSeason(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"", "Target content name (raid, arena)", ArgumentFlags.IgnoreCase)]
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
                        var raidSeasonName = connection.ExcelTableService.GetTable<RaidSeasonManageExcelTable>().UnPack().DataList.FirstOrDefault(x => x.SeasonId == seasonId);
                        connection.Account.RaidInfo = new RaidInfo()
                        {
                            SeasonId = seasonId,
                            BestRankingPoint = 0,
                            TotalRankingPoint = 0,
                        };

                        connection.SendChatMessage($"Raid Name: {string.Join(", ", raidSeasonName.OpenRaidBossGroup)}");
                        connection.SendChatMessage($"Raid ID: {raidSeasonName.SeasonId}");
                        connection.SendChatMessage($"Raid StartTime: {raidSeasonName.SeasonStartData}");
                        connection.SendChatMessage($"Raid EndTime: {raidSeasonName.SeasonEndData}");
                        connection.SendChatMessage($"Total Assault Raid is set to {seasonId}");
                        connection.Context.SaveChanges();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Value");
                    }
                    break;
                case "arena":
                    connection.SendChatMessage($"Arena command isn't implemented yet!");
                    connection.Context.SaveChanges();
                    break;
                default:
                    throw new ArgumentException("Invalid target!");
            }
        }
    }
}
