using Microsoft.Extensions.Configuration;
using System;

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

        

        Lazy<string[]> _fcolours;

        public string[] FColours => _fcolours.Value;
    }
}
