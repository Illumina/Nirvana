using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
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
            Assert.Contains($"Cache version: {CacheConstants.DataVersion}", version);
            Assert.Contains($"Supplementary annotation version: {SupplementaryAnnotationCommon.DataVersion}", version);
            Assert.Contains($"Reference version: {CompressedSequenceCommon.HeaderVersion}", version);
        }
    }
}
