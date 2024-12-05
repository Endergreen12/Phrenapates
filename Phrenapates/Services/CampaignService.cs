using Plana.MX.GameLogic.DBModel;
using Plana.MX.Logic.Battles.Summary;
using Plana.MX.Data;

public class CampaignService
{
    public static CampaignStageHistoryDB CreateStageHistoryDB(long accountServerId, CampaignStageInfo stageInfo)
    {
        return new CampaignStageHistoryDB() {
            AccountServerId = accountServerId,
            StageUniqueId = stageInfo.UniqueId,
            ChapterUniqueId = stageInfo.ChapterUniqueId,
            TacticClearCountWithRankSRecord = 0,
            ClearTurnRecord = 0,
            Star1Flag = false,
            Star2Flag = false,
            Star3Flag = false,
            LastPlay = DateTime.Now,
            TodayPlayCount = 1,
            FirstClearRewardReceive = DateTime.Now,
            StarRewardReceive = DateTime.Now,
        };
    }

    // Original Implementation
    public static void CalcStrategySkipStarGoals(CampaignStageHistoryDB historyDB, BattleSummary summary)
    {
        // All enemies are defeated
        var alivedEnemy = 0;
        foreach (var enemy in summary.Group02Summary.Heroes)
        {
            if (enemy.DeadFrame == -1)
            {
                alivedEnemy++;
            }
        }

        historyDB.Star1Flag = alivedEnemy == 0;

        // All enemies are defeated in 120 seconds
        historyDB.Star2Flag = summary.Group02Summary.Heroes.Last().DeadFrame <= 120 * 30;

        // No one is defeated
        var deadHero = 0;
        foreach (var hero in summary.Group01Summary.Heroes)
        {
            if (hero.DeadFrame != -1)
            {
                deadHero++;
            }
        }

        historyDB.Star3Flag = deadHero == 0;

        historyDB.ClearTurnRecord = 1; // The game uses this value to determine if the stage has been cleared
    }
}