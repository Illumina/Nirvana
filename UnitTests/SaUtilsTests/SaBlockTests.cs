using Xunit;

namespace UnitTests.SaUtilsTests
{
    public class SaBlockTests
    {
        [Fact]
        public void Loopback()
        {
            //const string testString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
            //var expectedBytes = Encoding.ASCII.GetBytes(testString);

            //byte[] compressedBytes;
            //var outputBlock = new SaBlock(new Zstandard());
            //var blockOffsets = new List<ISaIndexOffset<int>>();

            //using (var outputStream = new MemoryStream())
            //{
            //    outputBlock.Add(expectedBytes);
            //    outputBlock.Write(outputStream, blockOffsets);
            //    compressedBytes = outputStream.ToArray();
            //}

            //const int expectedCompressedByteLength = 314;
            //Assert.Equal(expectedCompressedByteLength, compressedBytes.Length);

            //byte[] uncompressedBytes;
            //var inputBlock = new SaBlock(new Zstandard());

            //using (var inputStream = new MemoryStream())
            //using (var outputStream = new MemoryStream())
            //{
            //    inputStream.Write(compressedBytes, 0, compressedBytes.Length);
            //    inputStream.Position = 0;

            //    inputBlock.Read(inputStream, outputStream);
            //    uncompressedBytes = outputStream.ToArray();
            //}

            //Assert.Equal(expectedBytes, uncompressedBytes);
        }
    }
}
