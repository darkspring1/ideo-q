namespace Converter.Size
{
    interface ISizeConverter
    {
        string[] Convert(string originalSize, out bool wasConverted);
    }
}
