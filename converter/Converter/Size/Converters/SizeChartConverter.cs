using Converter.Settings;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Size
{
    /// <summary>
    /// Конвертит по таблице
    /// </summary>
    class SizeChartConverter : ISizeConverter
    {
        private readonly ISizeChart _sizeChart;
        private readonly IDictionary<string, List<string>> _directMapping;
        private readonly IMemoryCache _cache;

        public SizeChartConverter(ISizeChart sizeChart, IMemoryCache cache)
        {
            _sizeChart = sizeChart;
            _cache = cache;
        }

        IDictionary<string, string> GetFSizes(ISizeChart sizeChart, string originalSize)
        {
            var key = $"{originalSize}_{sizeChart.Name}";

            var result = _cache.GetOrCreate(key, cacheEntry =>
            {
                foreach (var szDictionary in sizeChart)
                {
                    if (szDictionary.ContainsKey(originalSize))
                    {
                        return szDictionary;
                    }
                }
                return null;
            });

            return result;
        }

        public string[] Convert(string originalSize, out bool wasConverted)
        {

            var result = GetFSizes(_sizeChart, originalSize);
            if (result != null)
            {
                wasConverted = true;
                return result.Select(x => x.Value).ToArray();
            }

            wasConverted = false;
            return null;
        }
    }
}
