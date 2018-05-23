using System.Collections.Generic;
using System.Linq;

namespace Converter.Size
{
    /// <summary>
    /// для размеров вида uk 36j / us 36m
    /// </summary>
    class SeparateBySlashConverter : ISizeConverter
    {
        public ConvertResult Convert(string[] originalSizes)
        {
            var result = new List<string>();
            foreach (var originalSize in originalSizes)
            {
                var szArray = originalSize.Split("/");
                if (szArray.Length > 1)
                {
                    result.AddRange(szArray.Select(x => x.Replace(" ", "").ToLower()));
                }
            }

            return new ConvertResult(result.ToArray(), result.Any());
        }
    }
}
