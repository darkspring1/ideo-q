using Microsoft.Extensions.Configuration;
using System;

namespace Converter.Settings
{
    public class SizeConverterSettings : SettingsBase
    {
        Lazy<string[]> _shoesCategories;

        public SizeConverterSettings(IConfiguration config, string sectionName) : base(config, sectionName)
        {
            _shoesCategories = LazyArray("ShoesCategories");
        }


        public string[] ShoesCategories => _shoesCategories.Value;
    }
}
