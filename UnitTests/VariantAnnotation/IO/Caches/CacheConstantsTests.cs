using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.VariantAnnotation.IO.Caches
{
    public sealed class CacheConstantsTests
    {
        [Fact]
        public void TranscriptPath_Null_WithNullPrefix()
        {
            var observedResult = CacheConstants.TranscriptPath(null);
            Assert.Null(observedResult);
        }

        [Fact]
        public void TranscriptPath_NominalCase()
        {
            const string expectedResult = "bob.transcripts.ndb";
            var observedResult = CacheConstants.TranscriptPath("bob");
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void SiftPath_NominalCase()
        {
            const string expectedResult = "bob.sift.ndb";
            var observedResult = CacheConstants.SiftPath("bob");
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void PolyPhenPath_NominalCase()
        {
            const string expectedResult = "bob.polyphen.ndb";
            var observedResult = CacheConstants.PolyPhenPath("bob");
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
