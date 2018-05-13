using System.Linq;

namespace Converter.Size
{
    /// <summary>
    /// для размеров вида uk 36j / us 36m
    /// </summary>
    class SeparateBySlashConverter : ISizeConverter
    {
        public string[] Convert(string originalSize, out bool wasConverted)
        {
            var szArray = originalSize.Split("/");
            if (!szArray.Any())
            {
                wasConverted = false;
                return null;
            }
            wasConverted = true;
            return szArray.Select(x => x.Replace(" ", "").ToLower()).ToArray();
        }
    }
}
