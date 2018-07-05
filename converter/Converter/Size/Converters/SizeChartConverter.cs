using Converter.Settings;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Size
{
    /// <summary>
    /// Добавляет префикс к размеру и пытается его найти в таблице
    /// </summary>
    internal class SizeChartConverter2 : SizeChartConverter
    {
        public SizeChartConverter2(ISizeChart sizeChart, IMemoryCache cache) : base(sizeChart, cache)
        {
        }

        protected override IDictionary<string, string> GetFSizes(ISizeChart sizeChart, string originalSize)
        {
            var originalSizeFormated = originalSize.Replace(" ", "");

            if (originalSizeFormated.HasPrefix())
            {
                return null;
            }

            var originalSizeFormatedUs = originalSizeFormated.AddUsPrefix();
            var originalSizeFormatedUk = originalSizeFormated.AddUkPrefix();
            var originalSizeFormatedEur = originalSizeFormated.AddEurPrefix();

            var cacheKey = $"{nameof(SizeChartConverter2)}_{originalSizeFormated}_{sizeChart.Name}";

            var formatedSizes = new List<string>();

            var result = Cache.GetOrCreate(cacheKey, cacheEntry =>
            {
                int i = 0;
                IDictionary<string, string> dict = null;
                if (sizeChart.ContainsSize(originalSizeFormatedEur, ref dict))
                {
                    i++;
                }
                if (sizeChart.ContainsSize(originalSizeFormatedUk, ref dict))
                {
                    i++;
                }
                if (i == 2)
                {
                    //нет смысла дальше искать в таблице есть размер, как с префиксом eur так и с uk, непонятно какой брать
                    return null;
                }

                if (sizeChart.ContainsSize(originalSizeFormatedUs, ref dict))
                {
                    i++;
                }

                if (i == 2)
                {
                    //в таблице есть размер, c разными префиксами , непонятно какой брать
                    return null;
                }

                return dict;
            });

            return result;
        }

    }

    /// <summary>
    /// Конвертит по таблице
    /// </summary>
    class SizeChartConverter : ISizeConverter
    {
        private readonly ISizeChart _sizeChart;
        protected readonly IMemoryCache Cache;

        public SizeChartConverter(ISizeChart sizeChart, IMemoryCache cache)
        {
            _sizeChart = sizeChart;
            Cache = cache;
        }

        protected virtual IDictionary<string, string> GetFSizes(ISizeChart sizeChart, string originalSize)
        {
            var originalSizeFormated = originalSize.Replace(" ", "");
            var cacheKey = $"{nameof(SizeChartConverter)}_{originalSizeFormated}_{sizeChart.Name}";
            var result = Cache.GetOrCreate(cacheKey, cacheEntry =>
            {
                IDictionary<string, string> szDictionary = null;
                if (sizeChart.ContainsSize(originalSizeFormated, ref szDictionary))
                {
                    return szDictionary;
                }
                return null;
            });

            return result;
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
