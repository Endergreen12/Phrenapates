using Plana.Database;
using Plana.FlatData;
using Plana.Utils;
using Phrenapates.Services;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("unlockall", "Command to unlock all of its contents (campaign, weekdungeon, schooldungeon)", "/unlockall [target content]")]
    internal class UnlockAllCommand : Command
    {
        public UnlockAllCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"", "Target content name (campaign, weekdungeon, schooldungeon)", ArgumentFlags.IgnoreCase)]
        public string target { get; set; } = string.Empty;

        public override void Execute()
        {
            var account = connection.Account;

            switch (target)
            {
                case "campaign":
                    var campaignChapterExcel = connection.ExcelTableService.GetTable<CampaignChapterExcelTable>().UnPack().DataList;

                    foreach (var excel in campaignChapterExcel)
                    {
                        foreach (var stageId in excel.NormalCampaignStageId.Concat(excel.HardCampaignStageId).Concat(excel.NormalExtraStageId).Concat(excel.VeryHardCampaignStageId))
                        {
                            account.CampaignStageHistories.Add(new()
                            {
                                AccountServerId = account.ServerId,
                                StageUniqueId = stageId,
                                ChapterUniqueId = excel.Id,
                                ClearTurnRecord = 1,
                                Star1Flag = true,
                                Star2Flag = true,
                                Star3Flag = true,
                                LastPlay = DateTime.Now,
                                TodayPlayCount = 1,
                                FirstClearRewardReceive = DateTime.Now,
                                StarRewardReceive = DateTime.Now,
                            });
                        }
                    }

                    connection.Context.SaveChanges();
                    connection.SendChatMessage("Unlocked all of stages of campaign!");
                    break;

                case "weekdungeon":
                    var weekdungeonExcel = connection.ExcelTableService.GetTable<WeekDungeonExcelTable>().UnPack().DataList;

                    foreach (var excel in weekdungeonExcel)
                    {
                        var starGoalRecord = new Dictionary<StarGoalType, long>();
                        
                        if(excel.StarGoal[0] == StarGoalType.GetBoxes)
                        {
                            starGoalRecord.Add(StarGoalType.GetBoxes, excel.StarGoalAmount.Last());
                        } else {
                            foreach(var goalType in excel.StarGoal)
                            {
                                starGoalRecord.Add(goalType, 1);
                            }
                        }

                        account.WeekDungeonStageHistories.Add(new() {
                            AccountServerId = account.ServerId,
                            StageUniqueId = excel.StageId,
                            StarGoalRecord = starGoalRecord
                        });
                    }

                    connection.Context.SaveChanges();
                    connection.SendChatMessage("Unlocked all of stages of week dungeon!");
                    break;

                case "schooldungeon":
                    var schooldungeonExcel = connection.ExcelTableService.GetTable<SchoolDungeonStageExcelTable>().UnPack().DataList;

                    foreach (var excel in schooldungeonExcel)
                    {
                        var starFlags = new bool[excel.StarGoal.Count];
                        for(int i = 0; i < excel.StarGoal.Count; i++)
                        {
                            starFlags[i] = true;
                        }

                        account.SchoolDungeonStageHistories.Add(new() {
                            AccountServerId = account.ServerId,
                            StageUniqueId = excel.StageId,
                            StarFlags = starFlags
                        });
                    }

                    connection.Context.SaveChanges();
                    connection.SendChatMessage("Unlocked all of stages of school dungeon!");
                    break;

                default:
                    throw new ArgumentException("Invalid target!");
            }
        }
    }
}
