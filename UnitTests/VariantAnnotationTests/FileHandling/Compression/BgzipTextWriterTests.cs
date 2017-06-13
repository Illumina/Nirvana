using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using VariantAnnotation.FileHandling.Compression;
using Xunit;

namespace UnitTests.VariantAnnotationTests.FileHandling.Compression
{
    public class BgzipTextWriterTests
    {
        private static string RandomLine(Random random, StringBuilder sb, int lineLen)
        {
            const string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            sb.Clear();

            for (var i = 0; i < lineLen; i++)
            {
                var ch = input[random.Next(0, input.Length)];
                sb.Append(ch);
            }

            sb.Append('\n');
            return sb.ToString();
        }

        [Theory]
        [InlineData(30000, 1, 1000)]
        [InlineData(100, 32000, 100000)]
        public void WriteAndReadBack(int expectedLineCount, int minLineLen, int maxLineLen)
        {
            var observedLineCount = 0;
            var random = new Random(0);
            var sb = new StringBuilder();

            using (var ms = new MemoryStream())
            {
                using (var writer = new BgzipTextWriter(new BlockGZipStream(ms, CompressionMode.Compress, true)))
                {
                    for (int i = 0; i < expectedLineCount; i++) writer.Write(RandomLine(random, sb, random.Next(minLineLen, maxLineLen)));
                }
                
                ms.Position = 0;

                using (var reader = new StreamReader(new BlockGZipStream(ms, CompressionMode.Decompress)))
                {
                    while (reader.ReadLine() != null) observedLineCount++;
                }
            }

            Assert.Equal(expectedLineCount, observedLineCount);
        }
    }
}