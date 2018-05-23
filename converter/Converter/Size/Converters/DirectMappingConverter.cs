using System.Collections.Generic;

namespace Converter.Size
{
    class DirectMappingConverter : ISizeConverter
    {
        private readonly IDictionary<string, List<string>> _directMapping;

        public DirectMappingConverter(IDictionary<string, List<string>> directMapping)
        {
            _directMapping = directMapping;
        }

       
        public ConvertResult Convert(string[] originalSizes)
        {
            var result = new List<string>();
            foreach (var originalSize in originalSizes)
            {
                if (_directMapping.ContainsKey(originalSize))
                {
                    result.AddRange(_directMapping[originalSize]);
                }
            }

            return new ConvertResult(result.ToArray(), false);
        }
    }
}
