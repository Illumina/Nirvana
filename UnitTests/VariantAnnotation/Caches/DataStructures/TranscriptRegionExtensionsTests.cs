using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class TranscriptRegionExtensionsTests
    {
        private readonly ITranscriptRegion[] _forwardTranscriptRegions;
        private readonly ITranscriptRegion[] _reverseTranscriptRegions;

        public TranscriptRegionExtensionsTests()
        {
            _forwardTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1,   77997792, 77998025, 1, 234),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 77998026, 78001531, 234, 235),
                new TranscriptRegion(TranscriptRegionType.Exon, 2,   78001532, 78001723, 235, 426),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 78001724, 78024286, 426, 427),
                new TranscriptRegion(TranscriptRegionType.Exon, 3,   78024287, 78024416, 427, 556)
            };

            _reverseTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 3,   312957, 313157, 136, 336),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 313158, 313873, 135, 136),
                new TranscriptRegion(TranscriptRegionType.Exon, 2,   313874, 313892, 117, 135),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 313893, 314242, 116, 117),
                new TranscriptRegion(TranscriptRegionType.Exon, 1,   314243, 314358, 1, 116)
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
