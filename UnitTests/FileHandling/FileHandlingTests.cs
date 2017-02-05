using System.IO;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class FileHandlingTests : RandomFileBase
    {
        [Fact]
        public void GZipReadAndWrite()
        {
            const string expectedLine1 =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            const string expectedLine2 =
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

            var randomPath = GetRandomPath();

            using (var writer = GZipUtilities.GetStreamWriter(randomPath))
            {
                writer.WriteLine(expectedLine1);
                writer.WriteLine(expectedLine2);
            }

            string observedLine1;
            string observedLine2;
            string observedLine3;

            using (var reader = GZipUtilities.GetAppropriateStreamReader(randomPath))
            {
                observedLine1 = reader.ReadLine();
                observedLine2 = reader.ReadLine();
                observedLine3 = reader.ReadLine();
            }

            Assert.Equal(expectedLine1, observedLine1);
            Assert.Equal(expectedLine2, observedLine2);
            Assert.Null(observedLine3);
        }

        [Fact]
        public void HandleEmptyFile()
        {
            var randomPath = GetRandomPath();
            string observedLine;

            using (File.Create(randomPath))
            {
            }

            // NOTE: if an exception is thrown here, the unit test will fail
            using (var reader = GZipUtilities.GetAppropriateStreamReader(randomPath))
            {
                observedLine = reader.ReadLine();
            }

            Assert.Null(observedLine);
        }
    }
}