using System.IO;
using System.IO.Compression;
using Compression.Utilities;
using IO;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Compression.Utilities
{
    public sealed class GZipUtilitiesTests
    {
        private const string ExpectedString = "charlie";

        [Fact]
        public void GetAppropriateReadStream_Handle_TextFile()
        {
            string randomPath = RandomPath.GetRandomPath();

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
            string randomPath = RandomPath.GetRandomPath();            

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
            string randomPath = RandomPath.GetRandomPath();

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
