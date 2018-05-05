using Microsoft.Extensions.Configuration;

namespace Converter.Settings
{
    class MainSettings
    {
        private readonly IConfiguration _config;

        public MainSettings(IConfiguration config)
        {
            _config = config;
            ColorConverterSettings = new ColorConverterSettings(config, "ColorConverter");
            SizeConverterSettings = new SizeConverterSettings(config, "SizeConverter");
        }

        public ColorConverterSettings ColorConverterSettings { get; }

        public SizeConverterSettings SizeConverterSettings { get; }

        public string ConnectionString
        {
            get
            {
                return _config["ConnectionString"];
            }
        }
        
    }
}
