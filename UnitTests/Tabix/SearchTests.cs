using IO;
using Tabix;
using Xunit;
using UnitTests.TestUtilities;

namespace UnitTests.Tabix
{
    public sealed class SearchTests
    {
        private readonly Search _search;
        private const string ChromosomeName = "chr15";

        public SearchTests()
        {
            Index index;
            using (var stream = FileUtilities.GetReadStream(Resources.TopPath("miniHEXA_minimal.vcf.gz.tbi")))
            {
                index = Reader.GetTabixIndex(stream, ChromosomeUtilities.RefNameToChromosome);
            }

            var vcfStream = FileUtilities.GetReadStream(Resources.TopPath("miniHEXA_minimal.vcf.gz"));
            _search       = new Search(index, vcfStream);
        }

        [Fact]
        public void HasVariants_IntervalBeforeReads_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 1, 71589359);
            Assert.False(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalOverlapsReads_HasVcfPositionsOnIntervalTrue_ReturnsTrue()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 1, 71589360);
            Assert.True(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalOverlapsReads_ReturnsTrue()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 71589360, 76592131);
            Assert.True(observedResult);
        }

        [Fact]
        public void HasVariants_NoOverlap_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 76591006, 76592130);
            Assert.False(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalAfterReads_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 76592132, 101991189);
            Assert.False(observedResult);
        }

        [Fact]
        public void HasVariants_NullRefSeq_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants("chr18", 71589360, 76592131);
            Assert.False(observedResult);
        }
    }
}
