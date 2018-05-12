﻿using Microsoft.Extensions.Configuration;

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
    }
}
