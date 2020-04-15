using System.IO;
using System.IO.Compression;
using Compression.FileHandling;

namespace Jist
{
    public static class JistUtilities
    {
        public static byte[] GetCompressedBlock(string s, int compressionLevel=1)
        {
            using (var stream = new MemoryStream())
            {
                using(var memStream = new BlockGZipStream(stream, CompressionMode.Compress, true))
                using (var writer = new StreamWriter(memStream))
                {
                    writer.Write(s);
                }

                return stream.ToArray();
            }

        }

    }
}