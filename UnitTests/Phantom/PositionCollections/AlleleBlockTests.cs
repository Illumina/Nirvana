using System.Collections.Generic;
using System.Linq;
using Phantom.PositionCollections;
using Xunit;

namespace UnitTests.Phantom.PositionCollections
{
    public sealed class AlleleBlockTests
    {

        [Fact]
        public void GetPloidyFromGenotypes_DotIsIgnored()
        {
            var genotypes = new[] { Genotype.GetGenotype("."), Genotype.GetGenotype("1|2"), Genotype.GetGenotype("0/2") };
            var ploidy = AlleleBlock.GetMaxPloidy(genotypes);
            Assert.Equal(2, ploidy);
        }

        [Fact]
        public void GetAlleleBlockToSampleHaplotype_AsExpected()
        {
            var genotypeBlock1 = new GenotypeBlock(new[] { "1|2", "1/1", "0|1", "./1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock2 = new GenotypeBlock(new[] { "0/1", "0|0", "1|1", "1|1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock3 = new GenotypeBlock(new[] { "0|1", "1|1", "0/0" }.Select(Genotype.GetGenotype).ToArray(), 1);
            var genotypeToSample =
                new Dictionary<GenotypeBlock, List<int>>
                {
                    {genotypeBlock1, new List<int> {0, 1}},
                    {genotypeBlock2, new List<int> {2}},
                    {genotypeBlock3, new List<int> {3}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<int>(), 4).ToArray();
            var starts = Enumerable.Range(100, 4).ToArray();
            var functionBlockRanges = starts.Select(x => x + 2).ToList();


            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges, out _);
            var expectedBlock1 = new AlleleBlock(0, new[] { 1, 1, 0 }, 0, 0);
            var expectedBlock2 = new AlleleBlock(0, new[] { 2, 1, 1 }, 0, 0);
            var expectedBlock3 = new AlleleBlock(2, new[] { 1, 1 }, 1, 0);
            var expectedBlock4 = new AlleleBlock(1, new[] { 1, 1 }, 0, 1);
            var expectedBlock5 = new AlleleBlock(1, new[] { 0, 1 }, 0, 1);

            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock1));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock1]
                .SequenceEqual(new[] { new SampleHaplotype(0, 0), new SampleHaplotype(1, 0) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock2));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock2]
                .SequenceEqual(new[] { new SampleHaplotype(0, 1), new SampleHaplotype(1, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock3));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock3]
                .SequenceEqual(new[] { new SampleHaplotype(2, 0), new SampleHaplotype(2, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock4));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock4].SequenceEqual(new[] { new SampleHaplotype(3, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock5));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock5].SequenceEqual(new[] { new SampleHaplotype(3, 0) }));
        }


        [Fact]
        public void GetAlleleBlockToSampleHaplotype_AlleleBlock_WithInternalRefPositions_SplitIfOutOfRange()
        {
            var genotypeBlock1 = new GenotypeBlock(new[] { "1|2", "0/0", "0|0", "1/1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock2 = new GenotypeBlock(new[] { "1/1", "0|0", "1|1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock3 = new GenotypeBlock(new[] { "1|2", "0|0", "1|1" }.Select(Genotype.GetGenotype).ToArray(), 1);
            var genotypeToSample =
                new Dictionary<GenotypeBlock, List<int>>
                {
                    {genotypeBlock1, new List<int> {0}},
                    {genotypeBlock2, new List<int> {1}},
                    {genotypeBlock3, new List<int> {2}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<int>(), genotypeBlock1.Genotypes.Length).ToArray();

            var starts = new[] { 100, 102, 103, 104 };
            var functionBlockRanges = starts.Select(x => x + 2).ToList();

            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges, out _);
            var expectedBlock1 = new AlleleBlock(1, new[] { 1, 0, 1 }, 0, 0);
            var expectedBlock2 = new AlleleBlock(1, new[] { 2, 0, 1 }, 0, 0);

            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock1));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock1].SequenceEqual(new[] { new SampleHaplotype(2, 0) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock2));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock2].SequenceEqual(new[] { new SampleHaplotype(2, 1) }));
        }

        [Fact]
        public void GetAlleleBlockToSampleHaplotype_AlleleBlock_OneAlleleIsRef_EachTime()
        {
            var genotypeBlock1 = new GenotypeBlock(new[] { "1|0", "0|1", "1|0", "0|1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock2 = new GenotypeBlock(new[] { "1/1", "0|1", "1|0" }.Select(Genotype.GetGenotype).ToArray(), 1);
            var genotypeBlock3 = new GenotypeBlock(new[] { "0|0", "1|0", "0|1", "0|0" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock4 = new GenotypeBlock(new[] { "0|1", "1|0", "1|0" }.Select(Genotype.GetGenotype).ToArray());

            var genotypeToSample =
                new Dictionary<GenotypeBlock, List<int>>
                {
                    {genotypeBlock1, new List<int> {0}},
                    {genotypeBlock2, new List<int> {1}},
                    {genotypeBlock3, new List<int> {2}},
                    {genotypeBlock4, new List<int> {3}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<int>(), genotypeBlock1.Genotypes.Length).ToArray();

            var starts = new[] { 100, 101, 102, 104 };
            var functionBlockRanges = starts.Select(x => x + 2).ToList();

            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges, out _);
            var expectedBlock1 = new AlleleBlock(0, new[] { 1, 0, 1 }, 0, 0);
            var expectedBlock2 = new AlleleBlock(0, new[] { 0, 1, 0 }, 0, 0);
            var expectedBlock3 = new AlleleBlock(1, new[] { 1, 0 }, 0, 0);
            var expectedBlock4 = new AlleleBlock(1, new[] { 1, 1 }, 0, 0);
            var expectedBlock5 = new AlleleBlock(1, new[] { 0, 0 }, 0, 0);

            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock1));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock1].SequenceEqual(new[] { new SampleHaplotype(0, 0) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock2));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock2].SequenceEqual(new[] { new SampleHaplotype(0, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock3));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock3].SequenceEqual(new[] { new SampleHaplotype(1, 0) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock4));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock4].SequenceEqual(new[] { new SampleHaplotype(1, 1), new SampleHaplotype(3, 0) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock5));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock5].SequenceEqual(new[] { new SampleHaplotype(3, 1) }));
        }

        [Fact]
        public void GetAlleleBlockToSampleHaplotype_AwareOfTrimmedRefPositions()
        {
            var genotypeBlock1 = new GenotypeBlock(new[] { "0|0", "1|1", "1|1", "0|0" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock2 = new GenotypeBlock(new[] { "0|0", "1|1", "1|1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeBlock3 = new GenotypeBlock(new[] { "1|1", "1|1", "0|0" }.Select(Genotype.GetGenotype).ToArray(), 1);
            var genotypeBlock4 = new GenotypeBlock(new[] { "1|1", "1|1" }.Select(Genotype.GetGenotype).ToArray(), 1);
            var genotypeToSample =
                new Dictionary<GenotypeBlock, List<int>>
                {
                    {genotypeBlock1, new List<int> {0}},
                    {genotypeBlock2, new List<int> {1}},
                    {genotypeBlock3, new List<int> {2}},
                    {genotypeBlock4, new List<int> {3}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<int>(), 4).ToArray();
            var starts = Enumerable.Range(100, 4).ToArray();
            var functionBlockRanges = starts.Select(x => x + 2).ToList();


            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges, out _);
            var expectedBlock1 = new AlleleBlock(1, new[] { 1, 1 }, 1, 1);
            var expectedBlock2 = new AlleleBlock(1, new[] { 1, 1 }, 1, 0);
            var expectedBlock3 = new AlleleBlock(1, new[] { 1, 1 }, 0, 1);
            var expectedBlock4 = new AlleleBlock(1, new[] { 1, 1 }, 0, 0);

            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock1));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock1]
                .SequenceEqual(new[] { new SampleHaplotype(0, 0), new SampleHaplotype(0, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock2));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock2]
                .SequenceEqual(new[] { new SampleHaplotype(1, 0), new SampleHaplotype(1, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock3));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock3]
                .SequenceEqual(new[] { new SampleHaplotype(2, 0), new SampleHaplotype(2, 1) }));
            Assert.True(alleleBlockToSampleHaplotype.ContainsKey(expectedBlock4));
            Assert.True(alleleBlockToSampleHaplotype[expectedBlock4]
                .SequenceEqual(new[] { new SampleHaplotype(3, 0), new SampleHaplotype(3, 1) }));
        }
    }
}