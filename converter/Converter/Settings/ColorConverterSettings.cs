using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class ColorConverterSettings
    {
        private readonly IConfigurationSection _config;

        public ColorConverterSettings(IConfiguration config, string sectionName)
        {
            _config = config.GetSection(sectionName);

            _fcolours = new Lazy<string[]>(() => {
                var section = _config.GetSection("FColours");
                return section
                .AsEnumerable(true)
                .Select(x => x.Value.TrimStart().TrimEnd().ToLower())
                .ToArray();
            });
        }
        public string UnknownColoursFile
        {
            get
            {
                return _config["UnknownColoursFile"];
            }
        }

        public bool SaveResult
        {
            get
            {
                return bool.Parse(_config["SaveResult"]);
            }
        }

        public bool DeleteAllFColours
        {
            get
            {
                return bool.Parse(_config["DeleteAllFColours"]);
            }
        }


        public string ResultFile
        {
            get
            {
                return _config["ResultFile"];
            }
        }

        public IDictionary<string, List<string>> ColorMapping
        {
            get
            {
                var section = _config.GetSection("ColorMapping");
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
