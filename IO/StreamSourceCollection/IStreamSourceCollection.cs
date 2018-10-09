using System.Collections.Generic;
using IO.StreamSource;

namespace IO.StreamSourceCollection
{
    public interface IStreamSourceCollection
    {
        IEnumerable<IStreamSource> GetStreamSources();
        IEnumerable<IStreamSource> GetStreamSources(string suffix);
    }
}