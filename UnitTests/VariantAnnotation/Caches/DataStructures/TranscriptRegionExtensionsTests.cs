using Cache.Data;
using VariantAnnotation.Caches.DataStructures;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class TranscriptRegionExtensionsTests
    {
        private readonly TranscriptRegion[] _forwardTranscriptRegions;
        private readonly TranscriptRegion[] _reverseTranscriptRegions;

        public TranscriptRegionExtensionsTests()
        {
            _forwardTranscriptRegions = new TranscriptRegion[]
            {
                new(77997792, 77998025, 1, 234, TranscriptRegionType.Exon, 1, null),
                new(77998026, 78001531, 234, 235, TranscriptRegionType.Intron, 1, null),
                new(78001532, 78001723, 235, 426, TranscriptRegionType.Exon, 2, null),
                new(78001724, 78024286, 426, 427, TranscriptRegionType.Intron, 2, null),
                new(78024287, 78024416, 427, 556, TranscriptRegionType.Exon, 3, null)
            };

            _reverseTranscriptRegions = new TranscriptRegion[]
            {
                new(312957, 313157, 136, 336, TranscriptRegionType.Exon, 3, null),
                new(313158, 313873, 135, 136, TranscriptRegionType.Intron, 2, null),
                new(313874, 313892, 117, 135, TranscriptRegionType.Exon, 2, null),
                new(313893, 314242, 116, 117, TranscriptRegionType.Intron, 1, null),
                new(314243, 314358, 1, 116, TranscriptRegionType.Exon, 1, null)
            };
        }

        [Theory]
        [InlineData(77997792, 0)]
        [InlineData(78001723, 2)]
        [InlineData(78024416, 4)]
        [InlineData(78001724, 3)]
        public void BinarySearch_Nominal(int position, int expectedResult)
        {
            var observedResult = _forwardTranscriptRegions.BinarySearch(position);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(77997791, -1)]
        [InlineData(78024417, -6)]
        // the binarysearch method returns the bitwise complement of the next larger element
        public void BinarySearch_ReturnNegative_BeyondExons(int position, int expectedResult)
        {
            var observedResult = _forwardTranscriptRegions.BinarySearch(position);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetExonsAndIntrons_Forward_Internal()
        {
            var observedResults = _forwardTranscriptRegions.GetExonsAndIntrons(1, 3);

            Assert.Equal(2, observedResults.ExonStart);
            Assert.Equal(2, observedResults.ExonEnd);
            Assert.Equal(1, observedResults.IntronStart);
            Assert.Equal(2, observedResults.IntronEnd);
        }

        [Fact]
        public void GetExonsAndIntrons_Reverse_Internal()
        {
            var observedResults = _reverseTranscriptRegions.GetExonsAndIntrons(2, 4);

            Assert.Equal(1, observedResults.ExonStart);
            Assert.Equal(2, observedResults.ExonEnd);
            Assert.Equal(1, observedResults.IntronStart);
            Assert.Equal(1, observedResults.IntronEnd);
        }

        [Fact]
        public void GetExonsAndIntrons_Forward_StartBefore()
        {
            var observedResults = _forwardTranscriptRegions.GetExonsAndIntrons(-1, 3);

            Assert.Equal(1, observedResults.ExonStart);
            Assert.Equal(2, observedResults.ExonEnd);
            Assert.Equal(1, observedResults.IntronStart);
            Assert.Equal(2, observedResults.IntronEnd);
        }

        [Fact]
        public void GetExonsAndIntrons_Forward_EndAfter()
        {
            var observedResults = _forwardTranscriptRegions.GetExonsAndIntrons(2, -6);

            Assert.Equal(2, observedResults.ExonStart);
            Assert.Equal(3, observedResults.ExonEnd);
            Assert.Equal(2, observedResults.IntronStart);
            Assert.Equal(2, observedResults.IntronEnd);
        }

        [Fact]
        public void GetExonsAndIntrons_Reverse_StartBefore_EndAfter()
        {
            var observedResults = _reverseTranscriptRegions.GetExonsAndIntrons(-1, -6);

            Assert.Equal(1, observedResults.ExonStart);
            Assert.Equal(3, observedResults.ExonEnd);
            Assert.Equal(1, observedResults.IntronStart);
            Assert.Equal(2, observedResults.IntronEnd);
        }
    }
}