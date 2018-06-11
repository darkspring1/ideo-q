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
            var originalSizeFormated = originalSize.Replace(" ", "");
            var key = $"{originalSizeFormated}_{sizeChart.Name}";
            var result = _cache.GetOrCreate(key, cacheEntry =>
            {
                foreach (var szDictionary in sizeChart)
                {
                    if (szDictionary.ContainsKey(originalSizeFormated))
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

        public ConvertResult Convert(string[] originalSizes)
        {
            HashSet<string> hs = new HashSet<string>();
            foreach (var originalSize in originalSizes)
            {
                var result = GetFSizes(_sizeChart, originalSize);
                if (result != null)
                {
                    foreach (var kvp in result)
                    {
                        hs.Add(kvp.Value);
                    }
                }
            }

            var convertedSizes = hs.ToArray();
            //если сконвертили, то прерываем дальнейшую конвертацию
            return new ConvertResult(convertedSizes, convertedSizes.Any());
        }
    }
}
