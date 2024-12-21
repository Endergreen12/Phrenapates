using Plana.FlatData;
using Plana.MX.Logic.Battles.Summary;
using Plana.MX.Logic.Data;

namespace Phrenapates.Services
{
    public class RaidService
    {
        public static long CalculateTimeScore(float duration, Difficulty difficulty)
        {
            int[] multipliers = [120, 240, 480, 960, 1440, 1920, 2400];
            
            return (long)((3600f - duration) * multipliers[(int)difficulty]);
        }
        
        public static long CalculateGroggyAccumulation(long groggyPointTotal, CharacterStatExcelT characterStat)
        {
            long groggyPoint = groggyPointTotal;
            if(characterStat.GroggyGauge != 0) groggyPoint = groggyPointTotal % characterStat.GroggyGauge;

            return groggyPoint;
        }

        public static List<long> CharacterParticipation(GroupSummary Group01Summary)
        {
            return Group01Summary.Heroes.Select(x => x.ServerId).ToList()
            .Concat(Group01Summary.Supporters.Select(x => x.ServerId)).ToList();
        }

        public static RaidDamage CreateRaidCollection(RaidDamage raidDmg)
        {
            return new RaidDamage()
            {
                Index = raidDmg.Index,
                GivenDamage = raidDmg.GivenDamage,
                GivenGroggyPoint = raidDmg.GivenGroggyPoint
            };
        }

        public static long AIPhaseChecks(
            int bossIndex, long bossHp, long previousPhase,
            EliminateRaidStageExcelT raidStageExcel, List<CharacterStatExcelT> characterStatExcels    
        )
        {
            var characterStat = characterStatExcels.FirstOrDefault(x => x.CharacterId == raidStageExcel.BossCharacterId[bossIndex]);
            var bossName = raidStageExcel.GroundDevName;
            var maxHp = characterStat?.MaxHP100 ?? 0;

            // Special case for HOD which continues from previous phase
            if (bossName == "HOD") return previousPhase;

            // Handle bosses that change phase based on bossIndex
            var indexBasedPhases = new Dictionary<string, (int targetIndex, int phaseValue)>
            {
                { "ShiroKuro", (1, 7) },
                { "Kaitenger", (1, 1) },
                { "HoverCraft", (1, 4) }
            };

            if (indexBasedPhases.TryGetValue(bossName, out var indexPhase) && bossIndex == indexPhase.targetIndex)
            {
                return indexPhase.phaseValue;
            }

            // Handle bosses with HP threshold based phases
            var hpThresholds = new Dictionary<string, (float threshold, int phase)[]>
            {
                { "Chesed", new[] { (1f, 1) } },
                { "Hieronymus", new[] { (0.5f, 1) } },
                { "Goz", new[] { (0.6f, 1) } },
                { "EN0006", new[] { (0.75f, 1), (0.1f, 2) } }
            };

            if (hpThresholds.TryGetValue(bossName, out var thresholds))
            {
                foreach (var (threshold, phase) in thresholds.OrderBy(t => t.threshold))
                {
                    if (bossHp <= (maxHp * threshold)) return phase;
                }
            }

            // Special case for Binah with difficulty-based thresholds
            if (bossName == "Binah")
            {
                var difficultyIndex = (int)raidStageExcel.Difficulty;
                var secondPhase = new[] { 2f/3, 5f/8, 7f/11, 12f/20, 3.5f/6, 4f/7, 13.2f/23 };
                var thirdPhase = new[] { 1f/3, 2f/8, 3f/11, 4f/20, 1f/6, 1.5f/7, 4.9f/23 };

                if (bossHp <= (maxHp * thirdPhase[difficultyIndex])) return 2;
                else if (bossHp <= (maxHp * secondPhase[difficultyIndex])) return 1;
            }

            // Perorozilla and Gregorius use default phase

            return 0; // Default phase
        }
    }

}
