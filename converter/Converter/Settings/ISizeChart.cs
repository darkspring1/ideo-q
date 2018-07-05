using System.Collections.Generic;

namespace Converter.Settings
{
    public interface ISizeChart : IList<IDictionary<string, string>>
    {
        string Name { get; }

        bool ContainsSize(string size, out IDictionary<string, string> szDictionary);

        bool ContainsSize(string size);
    }
}
