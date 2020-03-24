using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Compression.FileHandling;
using Xunit;

namespace UnitTests.Compression.FileHandling
{
    public sealed class BgzipTextWriterTests
    {
        [Fact]
        public void BgzipTextWriter_EndToEnd()
        {
            var asterisks         = new string('*', BlockGZipStream.BlockGZipFormatCommon.BlockSize);
            var observedLines     = new List<string>();
            var observedPositions = new List<long>();

            using (var ms = new MemoryStream())
            {
                using (var stream = new BlockGZipStream(ms, CompressionMode.Compress, true))
                using (var writer = new BgzipTextWriter(stream))
                {
                    writer.Flush();
                    writer.WriteLine("BOB");
                    writer.WriteLine();
                    writer.Flush();
                    writer.Write("AB");
                    writer.Write("");
                    writer.Write("C");
                    writer.Write(" ");
                    writer.WriteLine("123");
                    writer.WriteLine(asterisks);
                    writer.WriteLine(asterisks);
                    writer.WriteLine(asterisks);
                }

                ms.Position = 0;

                using (var reader = new BgzipTextReader(new BlockGZipStream(ms, CompressionMode.Decompress)))
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        observedPositions.Add(reader.Position);
                        if (line == null) break;
                        observedLines.Add(line);
                    }
                }
            }

            Assert.Equal(6,         observedLines.Count);
            Assert.Equal("BOB",     observedLines[0]);
            Assert.Equal(0,         observedLines[1].Length);
            Assert.Equal("ABC 123", observedLines[2]);
            Assert.Equal(asterisks, observedLines[3]);
            Assert.Equal(2162687,   observedPositions[0]);
            Assert.Equal(2162688,   observedPositions[1]);
            Assert.Equal(2162696,   observedPositions[2]);
            Assert.Equal(45678601,  observedPositions[3]);
            Assert.Equal(88932362,  observedPositions[4]);
        }
    }
}