using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Converter.Settings
{
    public class SettingsBase
    {
        protected IConfigurationSection Config { get; }
        public SettingsBase(IConfiguration parent, string sectionName)
        {
            Config = parent.GetSection(sectionName);
        }

        public SettingsBase(IConfigurationSection config)
        {
            Config = config;
        }

        protected Lazy<string[]> LazyStringArray(string sectionName)
        {
            return new Lazy<string[]>(() => {
                var section = Config.GetSection(sectionName);
                return StringArray(section);
            });
        }

        protected string[] StringArray(IConfigurationSection section)
        {
            return section
                .AsEnumerable(true)
                .Select(x => x.Value.TrimStart().TrimEnd().ToLower())
                .ToArray();
        }
    }
}
