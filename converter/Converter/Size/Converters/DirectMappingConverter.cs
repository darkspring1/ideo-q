using System.Collections.Generic;
using System.Linq;

namespace Converter.Size
{
    class DirectMappingConverter : ISizeConverter
    {
        private readonly IDictionary<string, List<string>> _directMapping;

        public DirectMappingConverter(IDictionary<string, List<string>> directMapping)
        {
            _directMapping = directMapping;
        }
        public string[] Convert(string originalSize, out bool wasConverted)
        {
            if (_directMapping.ContainsKey(originalSize))
            {
                wasConverted = true;
                return _directMapping[originalSize].ToArray();
            }
            wasConverted = false;
            return null;
        }
    }
}
