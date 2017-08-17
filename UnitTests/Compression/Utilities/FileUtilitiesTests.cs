using System.IO;
using Compression.Utilities;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Compression.Utilities
{
    public sealed class FileUtilitiesTests : RandomFileBase
    {
        [Fact]
        public void GetReadStream_GetCreateStream_Loopback()
        {
            var random = GetRandomPath();
            const string expectedString = "charlie";

            using (var writer = new StreamWriter(FileUtilities.GetCreateStream(random)))
            {
                writer.WriteLine(expectedString);
            }

            string observedString;
            using (var reader = new StreamReader(FileUtilities.GetReadStream(random)))
            {
                observedString = reader.ReadLine();
            }

            Assert.Equal(expectedString, observedString);
        }
    }
}
