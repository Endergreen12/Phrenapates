namespace Phrenapates.Utils
{
    public static class StringExtension
    {
        public static string Capitalize(this string str)
        {
            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }
    }
}
