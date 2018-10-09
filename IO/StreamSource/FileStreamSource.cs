using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IO.StreamSource
{
    public sealed class FileStreamSource : IStreamSource
    {
        private readonly string _filePath;

        public FileStreamSource(string filePath)
        {
            _filePath = filePath;
        }

        private Stream GetStream() => FileUtilities.GetReadStream(_filePath);

        public Stream GetStream(long start)
        {
            var stream = GetStream();
            stream.Position = start;
            return stream;
        }

        public IStreamSource GetAssociatedStreamSource(string extraExtension) => new FileStreamSource(_filePath + extraExtension);

        public long GetLength() => new FileInfo( _filePath).Length;

        public Stream GetRawStream(long start, long end) => GetStream(start);
    }
}
