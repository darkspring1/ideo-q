using Converter.Settings;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Size
{

    class SizeConverter
    {
        private readonly IMemoryCache _cache;

        public SizeConverter(IMemoryCache cache)
        {
            _cache = cache;
        }

        Dictionary<string, string> GetFSizes(SizeChart sizeChart, string originalSize)
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

        public string[] Convert(SizeChart sizeChart, string originalSize, out bool wasConverted)
        {
            if (sizeChart != null)
            {
                var result = GetFSizes(sizeChart, originalSize);
                if (result != null)
                {
                    wasConverted = true;
                    return result.Select(x => x.Value).ToArray();
                }
            }
            wasConverted = false;
            return new[] { originalSize };
        }
    }
}
