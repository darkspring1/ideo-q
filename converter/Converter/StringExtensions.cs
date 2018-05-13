namespace Converter
{
    public static class StringExtensions
    {
        const string UK = "uk";
        const string US = "us";
        const string EUR = "eur";


        public static string[] SplitPrefixAndValue(this string key)
        {
            string prefix = null;
            var value = WithoutPrefix(key, out prefix);
            return new[] { prefix, value };
        }

        /// <summary>
        /// только префикс размера us, uk, eur
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string WithoutValue(this string src)
        {
            var prefix = "";
            WithoutPrefix(src, out prefix);
            return prefix;
        }

        /// <summary>
        /// только знаение размера, без префикса us, uk, eur
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string WithoutPrefix(this string key)
        {
            var str = "";
            return WithoutPrefix(key, out str);
        }

        /// <summary>
        /// только знаение размера, без префикса us, uk, eur
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string WithoutPrefix(this string key, out string prefix)
        {
            if (key.IndexOf(UK) == 0)
            {
                prefix = UK;
                return key.Substring(2);
            }
            else if (key.IndexOf(US) == 0)
            {
                prefix = US;
                return key.Substring(2);
            }
            else if (key.IndexOf(EUR) == 0)
            {
                prefix = EUR;
                return key.Substring(3);
            }

            throw new System.ArgumentException($"Wrong size: {key}", nameof(key));
        }
    }
}
