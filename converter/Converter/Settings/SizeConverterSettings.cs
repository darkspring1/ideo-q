using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Converter.Settings
{

    public class SizeConverterSettings : SettingsBase
    {
        Lazy<string[]> _shoesCategories;
        
        private SizeChart[] CreateSizeCharts()
        {
            var section = Config.GetSection("SizeCharts");
            
            return section
                .GetChildren()
                .Select(x => new SizeChart(x))
                .ToArray();
        }

        private SizeChartBinding[] CreateSizeChartBindings()
        {
            var section = Config.GetSection("SizeChartBindings");
            
            return section
                .GetChildren()
                .Select(x => new SizeChartBinding(x))
                .ToArray();
            
        }

        public SizeConverterSettings(IConfiguration config, string sectionName) : base(config, sectionName)
        {
            _shoesCategories = LazyStringArray("ShoesCategories");
            SizeCharts = CreateSizeCharts();
            SizeChartBindings = CreateSizeChartBindings();
        }

        public string[] ShoesCategories => _shoesCategories.Value;

        public SizeChart[] SizeCharts { get; }

        public SizeChartBinding[] SizeChartBindings { get; }
    }
}
