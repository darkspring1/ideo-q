using System.Collections.Generic;

namespace Converter.Size.Converters
{
    class ConvertersChain : List<ISizeConverter>
    {
        public string[] Convert(string originalSize)
        {
            string[] convertedSizes = new string[0];
            var inputSizes = new[] { originalSize };
            foreach (var converter in this)
            {
                var curResult = converter.Convert(inputSizes);
                if (curResult.WasConverted)
                {
                    convertedSizes = curResult.ConvertedSizes;
                    inputSizes = curResult.ConvertedSizes;
                }
                if (curResult.StopConvertation)
                {
                    break;
                }
            }
            return convertedSizes;
        }
    }
}
