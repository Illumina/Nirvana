using System.Collections.Generic;
using System.Linq;
using Phantom.PositionCollections;
using Xunit;

namespace UnitTests.Phantom.PositionCollections
{
    public sealed class AlleleBlockMergerTests
    {
        [Fact]
        public void ExtendAlleleBlock_AsExpected()
        {
            var alleleBlock1 = new AlleleBlock(2, new [] {1, 1}, 2, 2);
            var extendedBlock1 = AlleleBlockMerger.ExtendAlleleBlock(alleleBlock1, 2, 2);

            var expectedBlock1 = new AlleleBlock(0, new []{0, 0, 1, 1, 0, 0}, -1, -1);
            Assert.Equal(extendedBlock1, expectedBlock1);
        }

        [Fact]
        public void Merge_AsExpected()
        {
            var genotypeBlock1 = new GenotypeBlock(new[] { "1|0", "1|1", "1|1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock2 = new GenotypeBlock(new[] { "0|0", "1|0", "1|1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeToSample =
                new Dictionary<GenotypeBlock, List<int>>
                {
                    {genotypeBlock1, new List<int> {0}},
                    {genotypeBlock2, new List<int> {1}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<int>(), 3).ToArray();
            var starts = Enumerable.Range(100, 3).ToArray();
            var functionBlockRanges = starts.Select(x => x + 2).ToList();
            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges, out var alleleBlockGraph);
            var mergedAlleleBlockToSampleHaplotype =
                AlleleBlockMerger.Merge(alleleBlockToSampleHaplotype, alleleBlockGraph);

            var expectedBlock1 = new AlleleBlock(0, new[] { 1, 1, 1 }, -1, -1);
            var expectedBlock2 = new AlleleBlock(0, new[] { 0, 1, 1 }, -1, -1);
            var expectedBlock3 = new AlleleBlock(0, new[] { 0, 0, 1 }, -1, -1);

            Assert.True(mergedAlleleBlockToSampleHaplotype.ContainsKey(expectedBlock1));
            Assert.True(mergedAlleleBlockToSampleHaplotype[expectedBlock1]
                .SequenceEqual(new[] { new SampleHaplotype(0, 0) }));
            Assert.True(mergedAlleleBlockToSampleHaplotype.ContainsKey(expectedBlock2));
            Assert.True(mergedAlleleBlockToSampleHaplotype[expectedBlock2]
                .SequenceEqual(new[] { new SampleHaplotype(0, 1), new SampleHaplotype(1, 0) }));
            Assert.True(mergedAlleleBlockToSampleHaplotype.ContainsKey(expectedBlock3));
            Assert.True(mergedAlleleBlockToSampleHaplotype[expectedBlock3]
                .SequenceEqual(new[] { new SampleHaplotype(1, 1) }));
        }
    }
}