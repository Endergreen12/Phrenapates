using Plana.FlatData;

namespace Phrenapates.Services
{
    public static class RaidUtils
    {
        public static long CalculateTimeScore(float duration, Difficulty difficulty)
        {
            int[] multipliers = [120, 240, 480, 960, 1440, 1920, 2400];

            return (long)((3600f - duration) * multipliers[(int)difficulty]);
        }
    }
}
