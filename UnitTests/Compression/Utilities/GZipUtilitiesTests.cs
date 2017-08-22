using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.FileHandling;
using Compression.Utilities;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Compression.Utilities
{
    public sealed class GZipUtilitiesTests : RandomFileBase
    {
        private const string ExpectedString = "charlie";

        [Fact]
        public void GetAppropriateStream_PeekStream_WithBlockGZip()
        {
            string observedString;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(new BlockGZipStream(ms, CompressionMode.Compress, true)))
                {
                    writer.Write(ExpectedString);
                }

                ms.Position = 0;

                using (var peekStream = new PeekStream(ms))
                using (var reader = new BinaryReader(GZipUtilities.GetAppropriateStream(peekStream)))
                {
                    observedString = reader.ReadString();
                }
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateStream_PeekStream_WithGZip()
        {
            string observedString;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(new GZipStream(ms, CompressionMode.Compress, true)))
                {
                    writer.Write(ExpectedString);
                }

                ms.Position = 0;

                using (var peekStream = new PeekStream(ms))
                using (var reader = new BinaryReader(GZipUtilities.GetAppropriateStream(peekStream)))
                {
                    observedString = reader.ReadString();
                }
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateStream_PeekStream_WithTextFile()
        {
            string observedString;

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    writer.WriteLine(ExpectedString);
                }

                ms.Position = 0;

                using (var peekStream = new PeekStream(ms))
                using (var reader = new StreamReader(GZipUtilities.GetAppropriateStream(peekStream)))
                {
                    observedString = reader.ReadLine();
                }
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateStream_Handle_PeekStream()
        {
            string observedString;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(new BlockGZipStream(ms, CompressionMode.Compress, true)))
                {
                    writer.Write(ExpectedString);
                }

                ms.Position = 0;

                using (var peekStream = new PeekStream(ms))
                using (var reader = new BinaryReader(GZipUtilities.GetAppropriateStream(peekStream)))
                {
                    observedString = reader.ReadString();
                }
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateBinaryReader_Handle_BlockGZipFile()
        {
            var randomPath = GetRandomPath();

            using (var writer = GZipUtilities.GetBinaryWriter(randomPath))
            {
                writer.Write(ExpectedString);
            }

            string observedString;
            using (var reader = GZipUtilities.GetAppropriateBinaryReader(randomPath))
            {
                observedString = reader.ReadString();
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateReadStream_Handle_TextFile()
        {
            var randomPath = GetRandomPath();

            using (var writer = new StreamWriter(FileUtilities.GetCreateStream(randomPath)))
            {
                writer.WriteLine(ExpectedString);
            }

            string observedString;
            using (var reader = GZipUtilities.GetAppropriateStreamReader(randomPath))
            {
                observedString = reader.ReadLine();
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateReadStream_Handle_GZipFile()
        {
            var randomPath = GetRandomPath();            

            using (var writer = new StreamWriter(new GZipStream(FileUtilities.GetCreateStream(randomPath), CompressionMode.Compress)))
            {
                writer.WriteLine(ExpectedString);
            }

            string observedString;
            using (var reader = GZipUtilities.GetAppropriateStreamReader(randomPath))
            {
                observedString = reader.ReadLine();
            }

            Assert.Equal(ExpectedString, observedString);
        }

        [Fact]
        public void GetAppropriateReadStream_Handle_BlockGZipFile()
        {
            var randomPath = GetRandomPath();

            using (var writer = GZipUtilities.GetStreamWriter(randomPath))
            {
                writer.WriteLine(ExpectedString);
            }

            string observedString;
            using (var reader = GZipUtilities.GetAppropriateStreamReader(randomPath))
            {
                observedString = reader.ReadLine();
            }

            Assert.Equal(ExpectedString, observedString);
        }
    }
}
