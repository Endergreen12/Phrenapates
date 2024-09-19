using Plana.Database;
using Plana.FlatData;
using Plana.Utils;
using Phrenapates.Services;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("setseason", "Set season of content (raid, arena)", "/setseason [target content] [target season id]")]
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
                        connection.Account.RaidInfo = new RaidInfo()
                        {
                            SeasonId = seasonId,
                            BestRankingPoint = 0,
                            TotalRankingPoint = 0,
                        };

                        connection.SendChatMessage($"Set Raid SeasonId to: {seasonId}");
                        connection.Context.SaveChanges();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Value");
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid target!");
            }
        }
    }
}
