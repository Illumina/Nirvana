using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.Utilities
{
    public sealed class TranscriptUtilitiesTests
    {
        private readonly ICdnaCoordinateMap[] _cdnaMaps;

        public TranscriptUtilitiesTests()
        {
            _cdnaMaps = GetCdnaCoordinateMaps();
        }

        [Fact]
        public void GetTotalExonLength_MultipleExons()
        {
            const int expectedLength = 300;
            int observedLength = ExonUtilities.GetTotalExonLength(_cdnaMaps);
            Assert.Equal(expectedLength, observedLength);
        }

        private static ICdnaCoordinateMap[] GetCdnaCoordinateMaps()
        {
            var cdnaMaps = new ICdnaCoordinateMap[3];
            cdnaMaps[0] = new CdnaCoordinateMap(100, 199, 0, 99);
            cdnaMaps[1] = new CdnaCoordinateMap(300, 399, 100, 199);
            cdnaMaps[2] = new CdnaCoordinateMap(500, 599, 200, 299);
            return cdnaMaps;
        }
    }
}
