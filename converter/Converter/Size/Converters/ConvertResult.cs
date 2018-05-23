using System.Linq;

namespace Converter.Size
{
    class ConvertResult
    {
        public ConvertResult(string[] convertedSizes, bool stopConvertation)
        {
            ConvertedSizes = convertedSizes;
            StopConvertation = stopConvertation;
        }

        public string[] ConvertedSizes { get; }

        public bool WasConverted => ConvertedSizes != null && ConvertedSizes.Any();
        public bool StopConvertation { get; }
    }
}
