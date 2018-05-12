using Microsoft.Extensions.Configuration;

namespace Converter.Settings
{
    public class SizeChartBinding : SettingsBase
    {
        public SizeChartBinding(IConfigurationSection config) : base(config)
        {
            Categories = StringArray(config.GetSection("Categories"));
            SizeChartName = config["SizeChart"].ToLower();
        }

        public string[] Categories { get; }
        public string SizeChartName { get; }
    }
}
