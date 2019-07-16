#if EXPANDED_TESTS

using Genome;
using System.Collections.Generic;
using IO;
using Tabix;
using Xunit;

namespace UnitTests.Tabix
{
    public sealed class SearchTestsLocalMother
    {
        private readonly Search _search;
        private const string ChromosomeName = "chr2";

        public SearchTestsLocalMother()
        {
            var chr2 = new Chromosome("chr2", "2", 1);

            var refNameToChromosome = new Dictionary<string, IChromosome>
            {
                [chr2.EnsemblName] = chr2,
                [chr2.UcscName]    = chr2
            };

            Index index;
            using (var stream = FileUtilities.GetReadStream(@"E:\Data\Nirvana\Data\Mother\Mother.vcf.gz.tbi"))
            {
                index = Reader.GetTabixIndex(stream, refNameToChromosome);
            }

            var vcfStream = FileUtilities.GetReadStream(@"E:\Data\Nirvana\Data\Mother\Mother.vcf.gz");
            _search = new Search(index, vcfStream);
        }

        [Fact]
        public void HasVariants_IntervalBeforeReads_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 1, 11319);
            Assert.False(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalOverlapsReads_HasVcfPositionsOnIntervalTrue_ReturnsTrue()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 1, 11320);
            Assert.True(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalOverlapsReads_ReturnsTrue()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 217826, 435772);
            Assert.True(observedResult);
        }

        [Fact]
        public void HasVariants_NoOverlap_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 431200, 434667);
            Assert.False(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalAfterReads_ReturnsTrue()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 243172390, 243199373);
            Assert.True(observedResult);
        }

        [Fact]
        public void HasVariants_IntervalAfterReads_ReturnsFalse()
        {
            bool observedResult = _search.HasVariants(ChromosomeName, 243172391, 243199373);
            Assert.False(observedResult);
        }
    }
}

#endif
