namespace Converter.Size
{
    /// <summary>
    /// конвертит как есть, только удаляет пробелы, и переводит в нижний регистр
    /// </summary>
    class AsIsConverter : ISizeConverter
    {
        public string[] Convert(string originalSize, out bool wasConverted)
        {
            wasConverted = false;
            return new[] { originalSize.Replace(" ", "").ToLower() };
        }
    }
}
