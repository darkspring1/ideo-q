using Microsoft.Extensions.Configuration;

namespace Converter.Settings
{
    class MainSettings
    {
        private readonly IConfiguration _config;

        public MainSettings(IConfiguration config)
        {
            ColorConverterSettings = new ColorConverterSettings(config, "ColorConverter");

            _config = config;
        }

        public ColorConverterSettings ColorConverterSettings { get; private set; }


        public string ConnectionString
        {
            get
            {
                return _config["ConnectionString"];
            }
        }
        
    }
}
