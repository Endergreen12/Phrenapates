using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.Logic.Battles.Summary;

public class WeekDungeonService
{
    public static WeekDungeonStageHistoryDB CreateWeekDungeonStageHistoryDB(long accountServerId, WeekDungeonExcelT excel) {
        return new WeekDungeonStageHistoryDB() { AccountServerId = accountServerId, StageUniqueId = excel.StageId, StarGoalRecord = new() };
    }

    public static void CalcStarGoals(WeekDungeonExcelT excel, WeekDungeonStageHistoryDB stageHistory, BattleSummary battleSummary, bool clearStage)
    {
        for (int i = 0; i < excel.StarGoal.Count; i++)
        {
            stageHistory.StarGoalRecord.Add(excel.StarGoal[i], WeekDungeonService.CalcStarGoal(excel.WeekDungeonType,
                excel.StarGoal[i], excel.StarGoalAmount[i], battleSummary, clearStage));
        }
    }

    public static long CalcStarGoal(WeekDungeonType dungeonType, StarGoalType goalType, long goalSeconds, BattleSummary battleSummary, bool clearStage) {
        long result = 0;

        switch (goalType)
        {
            case StarGoalType.Clear:
                if (clearStage)
                {
                    result = 1;
                }
                break;

            case StarGoalType.AllAlive:
                result = CalcAllAlive(battleSummary);
                break;

            case StarGoalType.GetBoxes:
                result = CalcGetBoxes(dungeonType, battleSummary);
                break;

            case StarGoalType.ClearTimeInSec:
                if (battleSummary.EndFrame <= goalSeconds * 30)
                {
                    result = 1;
                }
                break;
        }

        return result;
    }

    public static long CalcAllAlive(BattleSummary battleSummary) {
        var allAlive = 1;
        foreach(var hero in battleSummary.Group01Summary.Heroes)
        {
            if(hero.DeadFrame != -1)
            {
                allAlive = 0;
                break;
            }
        }

        return allAlive;
    }

    public static long CalcGetBoxes(WeekDungeonType dungeonType, BattleSummary battleSummary) {
        return battleSummary.WeekDungeonSummary.FindGifts.First().ClearCount;
    }
}