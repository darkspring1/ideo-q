using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class SizeChart : List<Dictionary<string, string>>
    {
        public SizeChart(IConfigurationSection config)
        {
            Name = config.Key.ToLower();
            var childSections = config.GetChildren();
            AddRange(
                childSections
                    .Select(x => x
                                .AsEnumerable(true)
                                .ToDictionary(sz => sz.Key, sz => sz.Value))
                );
        }

        public string Name { get; }
    }
}
