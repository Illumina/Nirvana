using System.Collections.Generic;
using System.Linq;
using Phantom.DataStructures;
using Xunit;

namespace UnitTests.Phantom.DataStructures
{
    public sealed class AlleleIndexBlockTests
    {

        [Fact]
        public void GetPloidyFromGenotypes_DotIsIgnored()
        {
            string genotypes = ".;1|2;0/2";
            var ploidy = AlleleIndexBlock.GetMaxPloidy(genotypes);
            Assert.Equal(2, ploidy);
        }

        [Fact]
        public void TrimTaillingRefAlleles_RefIsTrimmed()
        {
            var blocks = new[]
            {
                new List<int> {1, 2, 1, 0, 1, 0, 0},
                new List<int> {0, 2, 1, 0, 1, 1, 0}
            };

            var expected = new[]
            {
                new List<int> {1, 2, 1, 0, 1, 0},
                new List<int> {0, 2, 1, 0, 1, 1}
            };

            AlleleIndexBlock.TrimTrailingRefAlleles(blocks);
            Assert.Equal(expected, blocks);
        }


        [Fact]
        public void GetAlleleIndexBlockToSampleIndex_AsExpected()
        {
            var genotypes1 = new[] {"1|2", "1/1", "0|1", "./1"};
            var genotypes2 = new[] {"0/1", "0|0", "1|1", "1|1"};
            var genotypes3 = new[] {"0|1", "1|1", "0/0"};
            var genotypeToSample =
                new Dictionary<(string, int), List<int>>
                {
                    {(string.Join(";", genotypes1), 0), new List<int> {0, 1}},
                    {(string.Join(";", genotypes2), 0), new List<int> {2}},
                    {(string.Join(";", genotypes3), 1), new List<int> {3}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<string>(), genotypes1.Length).ToArray();
            var starts = Enumerable.Range(100, 4).ToArray();
            var functionBlockRanges = starts.Select(x => x + 2).ToList();


            var alleleIndexBlocksToSample = AlleleIndexBlock.GetAlleleIndexBlockToSampleIndex(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges);
            var expectedBlock1 = new AlleleIndexBlock(0, new List<int> {1, 1, 0});
            var expectedBlock2 = new AlleleIndexBlock(0, new List<int> {2, 1, 1});
            var expectedBlock3 = new AlleleIndexBlock(2, new List<int> {1, 1});
            var expectedBlock4 = new AlleleIndexBlock(1, new List<int> {1, 1});
            var expectedBlock5 = new AlleleIndexBlock(1, new List<int> {0, 1});

            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock1));
            Assert.True(alleleIndexBlocksToSample[expectedBlock1]
                .SequenceEqual(new[] {new SampleAllele(0, 0), new SampleAllele(1, 0)}));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock2));
            Assert.True(alleleIndexBlocksToSample[expectedBlock2]
                .SequenceEqual(new[] {new SampleAllele(0, 1), new SampleAllele(1, 1)}));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock3));
            Assert.True(alleleIndexBlocksToSample[expectedBlock3]
                .SequenceEqual(new[] {new SampleAllele(2, 0), new SampleAllele(2, 1)}));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock4));
            Assert.True(alleleIndexBlocksToSample[expectedBlock4].SequenceEqual(new[] {new SampleAllele(3, 1)}));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock5));
            Assert.True(alleleIndexBlocksToSample[expectedBlock5].SequenceEqual(new[] {new SampleAllele(3, 0)}));
        }


        [Fact]
        public void GetAlleleIndexBlockToSampleIndex_AlleleBlock_WithInternalRefPositions_SplitIfOutOfRange()
        {
            var genotypes1 = new[] {"1|2", "0/0", "0|0", "1/1"};
            var genotypes2 = new[] {"1/1", "0|0", "1|1"};
            var genotypes3 = new[] {"1|2", "0|0", "1|1"};
            var starts = new[] {100, 102, 103, 104};
            var functionBlockRanges = starts.Select(x => x + 2).ToList();

            var genotypeToSample =
                new Dictionary<(string, int), List<int>>
                {
                    {(string.Join(";", genotypes1), 0), new List<int> {0}},
                    {(string.Join(";", genotypes2), 0), new List<int> {1}},
                    {(string.Join(";", genotypes3), 1), new List<int> {2}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<string>(), genotypes1.Length).ToArray();

            var alleleIndexBlocksToSample = AlleleIndexBlock.GetAlleleIndexBlockToSampleIndex(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges);
            var expectedBlock1 = new AlleleIndexBlock(1, new List<int> {1, 0, 1});
            var expectedBlock2 = new AlleleIndexBlock(1, new List<int> {2, 0, 1});

            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock1));
            Assert.True(alleleIndexBlocksToSample[expectedBlock1].SequenceEqual(new[] { new SampleAllele(2, 0) }));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock2));
            Assert.True(alleleIndexBlocksToSample[expectedBlock2].SequenceEqual(new[] { new SampleAllele(2, 1) }));
        }

        [Fact]
        public void GetAlleleIndexBlockToSampleIndex_AlleleBlock_OneAlleleIsRef_EachTime()
        {
            var genotypes1 = new[] { "1|0", "0|1", "1|0", "0|1" };
            var genotypes2 = new[] { "1/1", "0|1", "1|0" };
            var genotypes3 = new[] { "0|0", "1|0", "0|1", "0|0" };
            var genotypes4 = new[] { "0|1", "1|0", "1|0"};
            var starts = new[] { 100, 101, 102, 104 };
            var functionBlockRanges = starts.Select(x => x + 2).ToList();

            var genotypeToSample =
                new Dictionary<(string, int), List<int>>
                {
                    {(string.Join(";", genotypes1), 0), new List<int> {0}},
                    {(string.Join(";", genotypes2), 1), new List<int> {1}},
                    {(string.Join(";", genotypes3), 0), new List<int> {2}},
                    {(string.Join(";", genotypes4), 0), new List<int> {3}}
                };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<string>(), genotypes1.Length).ToArray();

            var alleleIndexBlocksToSample = AlleleIndexBlock.GetAlleleIndexBlockToSampleIndex(genotypeToSample,
                indexOfUnsupportedVars, starts, functionBlockRanges);
            var expectedBlock1 = new AlleleIndexBlock(0, new List<int> { 1, 0, 1 });
            var expectedBlock2 = new AlleleIndexBlock(0, new List<int> { 0, 1, 0 });
            var expectedBlock3 = new AlleleIndexBlock(1, new List<int> { 1, 0 });
            var expectedBlock4 = new AlleleIndexBlock(1, new List<int> { 1, 1 });
            var expectedBlock5 = new AlleleIndexBlock(1, new List<int> { 0, 0 });

            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock1));
            Assert.True(alleleIndexBlocksToSample[expectedBlock1].SequenceEqual(new[] { new SampleAllele(0, 0) }));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock2));
            Assert.True(alleleIndexBlocksToSample[expectedBlock2].SequenceEqual(new[] { new SampleAllele(0, 1) }));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock3));
            Assert.True(alleleIndexBlocksToSample[expectedBlock3].SequenceEqual(new[] { new SampleAllele(1, 0) }));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock4));
            Assert.True(alleleIndexBlocksToSample[expectedBlock4].SequenceEqual(new[] { new SampleAllele(1, 1) , new SampleAllele(3, 0) }));
            Assert.True(alleleIndexBlocksToSample.ContainsKey(expectedBlock5));
            Assert.True(alleleIndexBlocksToSample[expectedBlock5].SequenceEqual(new[] { new SampleAllele(3, 1) }));
        }
    }
}