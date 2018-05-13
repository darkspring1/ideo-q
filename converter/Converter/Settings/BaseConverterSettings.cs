using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{
    public class BaseConverterSettings : SettingsBase
    {
        public BaseConverterSettings(IConfiguration config, string sectionName) : base(config, sectionName)
        {
            
        }

        public bool SaveResult
        {
            get
            {
                return bool.Parse(Config["SaveResult"]);
            }
        }

        public string UnknownFile
        {
            get
            {
                return Config["UnknownFile"];
            }
        }

        public string ResultFile
        {
            get
            {
                return Config["ResultFile"];
            }
        }

        public IDictionary<string, List<string>> DirectMapping
        {
            get
            {
                var section = Config.GetSection("DirectMapping");
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
    }
}
