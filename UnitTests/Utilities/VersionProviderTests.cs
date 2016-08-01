using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    [Collection("Entry Assembly")]
    public class VersionProviderTests
    {
        [Fact]
        public void GetProgramVersion()
        {
            var versionProvider = new NirvanaVersionProvider();
            var version = versionProvider.GetProgramVersion();
            Assert.StartsWith("Nirvana", version);
        }

        [Fact]
        public void GetDataVersion()
        {
            var versionProvider = new NirvanaVersionProvider();
            var version = versionProvider.GetDataVersion();
            Assert.Contains($"Cache version: {NirvanaDatabaseCommon.DataVersion}", version);
            Assert.Contains($"Supplementary annotation version: {SupplementaryAnnotationCommon.DataVersion}", version);
            Assert.Contains($"Reference version: {CompressedSequenceCommon.HeaderVersion}", version);
        }
    }
}
