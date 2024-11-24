namespace Phrenapates.Utils
{
    public static class RandomUtils
    {
        public static List<T> GetRandomList<T>(List<T> list, int limit)
        {
            Random rng = new Random();
            int n = list.Count;
            
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }

            return list.GetRange(0, Math.Min(limit, list.Count));
        }
    }
}
