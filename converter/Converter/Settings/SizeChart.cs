using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class SizeChart : List<IDictionary<string, string>>, ISizeChart
    {
        KeyValuePair<string, string> GetKVP(string configSize)
        {
            var key = configSize.Replace(" ", "").ToLower();
            var value = key;
            /*
            if (key.IndexOf(UK) == 0 || key.IndexOf(US) == 0)
            {
                value = $"{configSize.Substring(0, 2)} {configSize.Substring(2)}";
            }
            else if (key.IndexOf(EUR) == 0)
            {
                value = $"{EUR} {configSize.Substring(3)}";
            }
            */
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

        public string Name { get; }
    }
}
