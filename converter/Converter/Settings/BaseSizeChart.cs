using System.Collections.Generic;

namespace Converter.Settings
{
    public abstract class BaseSizeChart : List<IDictionary<string, string>>, ISizeChart
    {
        public string Name { get; protected set; }

        public bool ContainsSize(string size, out IDictionary<string, string> szDictionary)
        {
            szDictionary = null;
            foreach (var dictionary in this)
            {
                if (dictionary.ContainsKey(size))
                {
                    szDictionary = dictionary;
                    return true;
                }
            }

            return false;
        }

        public bool ContainsSize(string size)
        {
            IDictionary<string, string> szDictionary;

            return ContainsSize(size, out szDictionary);
        }
    }
}
