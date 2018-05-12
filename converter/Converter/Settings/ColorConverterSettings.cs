using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class ColorConverterSettings : BaseConverterSettings
    {
        public ColorConverterSettings(IConfiguration config, string sectionName) : base(config, sectionName)
        {
            _fcolours = LazyStringArray("FColours");
        }

        public bool DeleteAllFColours
        {
            get
            {
                return bool.Parse(Config["DeleteAllFColours"]);
            }
        }

        public IDictionary<string, List<string>> ColorMapping
        {
            get
            {
                var section = Config.GetSection("ColorMapping");
                return section
                    .AsEnumerable(true)
                    .ToDictionary(
                        kvp => kvp.Key.ToLower(),
                        kvp => kvp
                            .Value
                            .Split(",")
                            .Select(x => x.TrimStart().TrimEnd().ToLower())
                            .ToList());
            }
        }

        Lazy<string[]> _fcolours;

        public string[] FColours => _fcolours.Value;
    }
}
