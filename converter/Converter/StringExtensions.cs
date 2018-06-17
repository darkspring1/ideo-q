using Converter.DAL.Constants;
using System;

namespace Converter
{
    public static class StringExtensions
    {
        /// <summary>
        /// Определяет, есть ли префикс US в размере
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool IsUs(this string src)
        {
            try
            {
                return src.Length >= 2 && src[0] == 'u' && src[1] == 's';
            }
            catch (Exception e)
            {

            }
            return false;
        }

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
        /// Выбрасывает исключение, если передать значение без префикса
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
        public static string TryWithoutPrefix(this string key)
        {
            var str = "";
            return TryWithoutPrefix(key, out str);
        }

        /// <summary>
        /// Только знаение размера, без префикса us, uk, eur
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string TryWithoutPrefix(this string key, out string prefix)
        {
            if (key.IndexOf(SizePrefix.UK) == 0)
            {
                prefix = SizePrefix.UK;
            }
            else if (key.IndexOf(SizePrefix.US) == 0)
            {
                prefix = SizePrefix.US;
            }
            else if (key.IndexOf(SizePrefix.EUR) == 0)
            {
                prefix = SizePrefix.EUR;
            }
            else
            {
                prefix = null;
                return key;
            }

            return key.Substring(prefix.Length);
        }


        /// <summary>
        /// Только знаение размера, без префикса us, uk, eur
        /// Выбрасывает исключение, если передать значение без префикса
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string WithoutPrefix(this string key, out string prefix)
        {
            if (key.IndexOf(SizePrefix.UK) == 0)
            {
                prefix = SizePrefix.UK;
            }
            else if (key.IndexOf(SizePrefix.US) == 0)
            {
                prefix = SizePrefix.US;
            }
            else if (key.IndexOf(SizePrefix.EUR) == 0)
            {
                prefix = SizePrefix.EUR;
            }
            else
            {
                throw new System.ArgumentException($"Wrong size: {key}", nameof(key));
            }

            return key.Substring(prefix.Length);
        }
    }
}
