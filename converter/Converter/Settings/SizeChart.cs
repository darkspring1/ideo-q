using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class SizeChart : BaseSizeChart
    {
        KeyValuePair<string, string> GetKVP(string configSize)
        {
            var key = configSize.Replace(" ", "").ToLower();
            var value = key;
            return new KeyValuePair<string, string>(key, value);
        }

        public SizeChart(IConfigurationSection config)
        {
            Name = config.Key.ToLower();
            var childSections = config.GetChildren();

            foreach (var child in childSections)
            {
                var szDictionary = child.GetChildren().Select(x => GetKVP(x.Value)).ToDictionary(x => x.Key, x => x.Value);
                Add(szDictionary);
            }
        }
    }
}
