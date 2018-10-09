using System.IO;

namespace IO.StreamSource
{
    public interface IStreamSource
    {
        Stream GetStream(long start = 0);

        IStreamSource GetAssociatedStreamSource(string extraExtension);
        long GetLength();
        Stream GetRawStream(long start, long end = long.MaxValue);
    }
}