using System.Collections.Generic;
using System.IO;
using System.Linq;
using IO.StreamSource;

namespace IO.StreamSourceCollection
{
    public class FileDirectory : IStreamSourceCollection
    {
        private readonly string _directory;

        public FileDirectory(string directory)
        {
            _directory = directory;
        }

        public IEnumerable<IStreamSource> GetStreamSources() => Directory.GetFiles(_directory).Select(x => new FileStreamSource(x));
        public IEnumerable<IStreamSource> GetStreamSources(string suffix) => Directory.GetFiles(_directory, "*" + suffix).Select(x => new FileStreamSource(x));
    }
}