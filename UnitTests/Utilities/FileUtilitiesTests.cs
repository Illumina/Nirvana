using System.IO;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class FileUtilitiesTests : RandomFileBase
    {
        [Fact]
        public void GetPath()
        {
            var randomPath = GetRandomPath();

            using (var writer = FileUtilities.GetCreateStream(randomPath))
            {
                Assert.Equal(randomPath, FileUtilities.GetPath(writer));
            }
        }

        [Fact]
        public void GetPathStream()
        {
            const string randomPath = "(stream)";

            using (var ms = new MemoryStream())
            {
                Assert.Equal(randomPath, FileUtilities.GetPath(ms));
            }
        }
    }
}
