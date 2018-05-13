using System.Collections.Generic;

namespace Converter.Settings
{
    public interface ISizeChart : IList<IDictionary<string, string>>
    {
        string Name { get; }
    }
}
