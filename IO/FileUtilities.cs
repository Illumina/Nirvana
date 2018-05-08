using System.IO;
using System.Text;

namespace IO
{
    public static class FileUtilities
    {
        private const int StreamReaderBufferSize = 10_485_760;

        public static FileStream GetReadStream(string path)   => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        public static FileStream GetCreateStream(string path) => new FileStream(path, FileMode.Create);

        public static StreamReader GetStreamReader(Stream stream, bool leaveOpen = false) =>
            new StreamReader(stream, Encoding.Default, true, StreamReaderBufferSize, leaveOpen);
    }
}
