using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class ColorConverterSettings : SettingsBase
    {
        public ColorConverterSettings(IConfiguration config, string sectionName) : base(config, sectionName)
        {
            _fcolours = LazyArray("FColours");
        }

        public string UnknownColoursFile
        {
            get
            {
                return Config["UnknownColoursFile"];
            }
        }

        public bool SaveResult
        {
            get
            {
                return bool.Parse(Config["SaveResult"]);
            }
        }

        public bool DeleteAllFColours
        {
            get
            {
                return bool.Parse(Config["DeleteAllFColours"]);
            }
        }


        public string ResultFile
        {
            get
            {
                return Config["ResultFile"];
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
