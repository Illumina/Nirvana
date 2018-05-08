using System.IO;
using IO;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Compression.Utilities
{
    public sealed class FileUtilitiesTests
    {
        [Fact]
        public void GetReadStream_GetCreateStream_Loopback()
        {
            var random = RandomPath.GetRandomPath();
            const string expectedString = "charlie";

            using (var writer = new StreamWriter(FileUtilities.GetCreateStream(random)))
            {
                writer.WriteLine(expectedString);
            }

            string observedString;
            using (var reader = FileUtilities.GetStreamReader(FileUtilities.GetReadStream(random)))
            {
                observedString = reader.ReadLine();
            }

            Assert.Equal(expectedString, observedString);
        }
    }
}
