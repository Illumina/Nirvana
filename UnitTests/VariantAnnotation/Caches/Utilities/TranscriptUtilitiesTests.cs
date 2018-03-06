using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.Utilities
{
    public sealed class TranscriptUtilitiesTests
    {
        private readonly ITranscriptRegion[] _transcriptRegions;

        public TranscriptUtilitiesTests()
        {
            _transcriptRegions = GetTranscriptRegions();
        }

        [Fact]
        public void GetTotalExonLength_MultipleExons()
        {
            const int expectedLength = 300;
            int observedLength = ExonUtilities.GetTotalExonLength(_transcriptRegions);
            Assert.Equal(expectedLength, observedLength);
        }

        private static ITranscriptRegion[] GetTranscriptRegions()
        {
            return new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 100, 199, 0, 99),
                new TranscriptRegion(TranscriptRegionType.Gap, 0, 200, 299, 99, 100),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 300, 399, 100, 199),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 400, 499, 199, 200),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 500, 599, 200, 299)
            };
        }
    }
}
