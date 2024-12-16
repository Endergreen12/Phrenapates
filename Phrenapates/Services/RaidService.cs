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
        public static List<long> CharacterParticipation(GroupSummary Group01Summary)
        {
            return Group01Summary.Heroes.Select(x => x.ServerId).ToList()
            .Concat(Group01Summary.Supporters.Select(x => x.ServerId)).ToList();
        }

        public static RaidDamage CreateRaidDamage(RaidDamage raidDmg)
        {
            return new RaidDamage()
            {
                Index = raidDmg.Index,
                GivenDamage = raidDmg.GivenDamage,
                GivenGroggyPoint = raidDmg.GivenGroggyPoint
            };
        }

        public static long CalculateGroggyAccumulation(long groggyPointTotal, CharacterStatExcelT characterStat)
        {
            long groggyPoint = groggyPointTotal;

            if (groggyPointTotal >= characterStat.GroggyGauge) groggyPoint = groggyPointTotal - characterStat.GroggyGauge;

            return groggyPoint;
        }
    }

}
