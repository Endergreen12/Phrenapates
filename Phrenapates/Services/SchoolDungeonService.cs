using Plana.MX.GameLogic.DBModel;
using Plana.FlatData;
using Plana.MX.Logic.Battles.Summary;

namespace Phrenapates.Services
{
    public class SchoolDungeonService
    {
        public static SchoolDungeonStageHistoryDB CreateSchoolDungeonStageHistoryDB(long accountId, SchoolDungeonStageExcelT excel)
        {
            return new SchoolDungeonStageHistoryDB() { AccountServerId = accountId, StageUniqueId = excel.StageId };
        }

        public static void CalcStarGoals(SchoolDungeonStageExcelT excel, SchoolDungeonStageHistoryDB historyDB, BattleSummary battleSummary)
        {
            historyDB.StarFlags = new bool[excel.StarGoal.Count];

            var starGoalTypes = excel.StarGoal;
            var starGoalAmounts = excel.StarGoalAmount;

            for (int i = 0; i < starGoalTypes.Count; i++)
            {
                var targetGoalType = starGoalTypes[i];
                var targetGoalAmount = starGoalAmounts[i];

                historyDB.StarFlags[i] = IsStarGoalCleared(targetGoalType, targetGoalAmount, battleSummary);
            }
        }

        private static bool IsStarGoalCleared(StarGoalType goalType, int goalAmount, BattleSummary battleSummary)
        {
            var result = false;

            switch (goalType)
            {
                case StarGoalType.Clear:
                    result = battleSummary.EndType == Plana.MX.Logic.Battles.BattleEndType.Clear;
                    break;

                case StarGoalType.AllAlive:
                    foreach (var hero in battleSummary.Group01Summary.Heroes)
                    {
                        if (hero.DeadFrame != -1)
                        {
                            return false;
                        }
                    }

                    result = true;
                    break;

                case StarGoalType.ClearTimeInSec:
                    result = battleSummary.EndFrame <= goalAmount * 30;
                    break;
            }

            return result;
        }
    }
}