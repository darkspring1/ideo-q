using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Converter.Settings
{

    public class SizeConverterSettings : BaseConverterSettings
    {
        Lazy<string[]> _shoesCategories;
        
        private ISizeChart[] CreateSizeCharts()
        {
            const string bra_sizes = "bra_sizes";
            const string cup_sizes = "cup_sizes";

            var section = Config.GetSection("SizeCharts");

            var braSizesSection = section.GetSection(bra_sizes);
            var cupSizesSection = section.GetSection(cup_sizes);

            var result = section
                .GetChildren()
                .Where(x => x.Key != bra_sizes || x.Key != cup_sizes)
                .Select(x => new SizeChart(x))
                .ToList<ISizeChart>();

            result.Add(new BraSizeChart(braSizesSection, cupSizesSection));
            return result.ToArray();

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

        public ISizeChart[] SizeCharts { get; }

        public SizeChartBinding[] SizeChartBindings { get; }
    }
}
