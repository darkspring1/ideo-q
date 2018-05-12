using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Converter.Settings
{
    public class SettingsBase
    {
        protected IConfigurationSection Config { get; }
        public SettingsBase(IConfiguration config, string sectionName)
        {
            Config = config.GetSection(sectionName);
        }

        protected Lazy<string[]> LazyStringArray(string sectionName)
        {
            return new Lazy<string[]>(() => {
                var section = Config.GetSection(sectionName);
                return section
                .AsEnumerable(true)
                .Select(x => x.Value.TrimStart().TrimEnd().ToLower())
                .ToArray();
            });
        }
    }
}
