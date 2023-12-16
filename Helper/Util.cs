namespace Tenor.Helper
{
    public static class Util
    {
        public static string cleanExtraFieldValue(string name)
        {
            string result = string.Empty;

            foreach (char c in name)
                if (c != '[' && c != ']' && c != '\\' && c != '"' && c != ' ')
                    result += c;

            return result;
        }
    }
}