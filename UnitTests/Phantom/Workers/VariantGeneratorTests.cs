using System.Collections.Generic;
using System.Linq;
using Genome;
using Moq;
using Phantom.DataStructures;
using Phantom.Utilities;
using Phantom.Workers;
using UnitTests.TestDataStructures;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;
using Xunit;

namespace UnitTests.Phantom.Workers
{
    public sealed class VariantGeneratorTests
    {
        [Fact]
        public void GetPositionsAndRefAltAlleles_AsExpected()
        {
            var genotypeBlock = new GenotypeBlock(new[] { "1|2", "1/1", "0|1", "0/1" }.Select(Genotype.GetGenotype).ToArray());
            var genotypeToSample =
                new Dictionary<GenotypeBlock, List<int>> { { genotypeBlock, new List<int> { 0 } } };
            var indexOfUnsupportedVars = Enumerable.Repeat(new HashSet<int>(), genotypeBlock.Genotypes.Length).ToArray();
            var starts = new[] { 356, 358, 360, 361 };
            var functionBlockRanges = new List<int> { 358, 360, 362, 364 };
            var alleles = new[] { new[] { "G", "C", "T" }, new[] { "A", "T" }, new[] { "C", "G" }, new[] { "G", "T" } };
            const string refSequence = "GAATCG";
            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSample, indexOfUnsupportedVars, starts, functionBlockRanges, out var alleleBlockGraph);
            var mergedAlleleBlockToSampleHaplotype =
                AlleleBlockMerger.Merge(alleleBlockToSampleHaplotype, alleleBlockGraph).ToArray();
            var alleleSet = new AlleleSet(null, starts, alleles);
            var alleleBlocks = mergedAlleleBlockToSampleHaplotype.Select(x => x.Key).ToArray();
            var decomposedPositionIndex = new HashSet<(int, int)>();
            var result1 = VariantGenerator.GetPositionsAndRefAltAlleles(alleleBlocks[0], alleleSet, refSequence, starts[0], decomposedPositionIndex);
            var result2 = VariantGenerator.GetPositionsAndRefAltAlleles(alleleBlocks[1], alleleSet, refSequence, starts[0], decomposedPositionIndex);

            Assert.Equal((356, 360, "GAATC", "CATTC"), result1);
            Assert.Equal((356, 360, "GAATC", "TATTG"), result2);
        }


        [Fact]
        public void VariantGenerator_AsExpected()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T,G	.	PASS	.	GT:PS	0|1:123	2/2:789	0|2:456", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A,G	.	PASS	.	GT:PS	1|1:301	1|2:789	1|2:456", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT:PS	.	1|0:789	0/1:.", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Equal(2, recomposedPositions.Count);
            Assert.Equal("chr1	2	.	AGC	AGA,GGG,TGA	.	PASS	RECOMPOSED	GT:PS	1|3:123	.	1|2:456", string.Join("\t", recomposedPositions[0].VcfFields));
            Assert.Equal("chr1	2	.	AGCTG	GGATC,GGGTG	.	PASS	RECOMPOSED	GT:PS	.	1|2:789	.", string.Join("\t", recomposedPositions[1].VcfFields));
        }

        [Fact]
        public void VariantGenerator_NoMnvAfterTrimming_NotRecompose()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	1	.	C	A	.	PASS	.	GT:PS	1|0:1584593	1|1:.	0|1:.", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	2	.	A	C	.	PASS	.	GT:PS	0|1:1584593	0/0:.	0/0:.", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 3, 4 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2 }, functionBlockRanges).ToList();

            Assert.Empty(recomposedPositions);
        }

        [Fact]
        public void VariantGenerator_OverlappingDeletionInTheMiddle_Ignored()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAATCGCGA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T	.	PASS	.	GT	0|1	0/0", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	CTGAATCGCGA	C	.	PASS	.	GT	0/0	0|1", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	4	.	C	A	.	PASS	.	GT	1|1	0/0", sequenceProvider.RefNameToChromosome);

            var functionBlockRanges = new List<int> { 4, 6, 6 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Single(recomposedPositions);
            Assert.Equal("chr1	2	.	AGC	AGA,TGA	.	PASS	RECOMPOSED	GT	1|2	.", string.Join("\t", recomposedPositions[0].VcfFields));
        }

        [Fact]
        public void VariantGenerator_OverlappingDeletionAtTheEnd_Ignored()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAATCGCGA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T	.	PASS	.	GT	0|1	0/0", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A	.	PASS	.	GT	1|1	0/0", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	4	.	CTGAATCGCGA	C	.	PASS	.	GT	0/0	0|1", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 6 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Single(recomposedPositions);
            Assert.Equal("chr1	2	.	AGC	AGA,TGA	.	PASS	RECOMPOSED	GT	1|2	.", string.Join("\t", recomposedPositions[0].VcfFields));
        }

        [Fact]
        public void VariantGenerator_MinQualUsed_DotIgnored()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T,G	.	PASS	.	GT:PS	0|1:123	2/2:.	0|2:456", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A,G	45	PASS	.	GT:PS	1|1:301	1|2:.	1|2:456", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	30.1	PASS	.	GT	.	1|0	0/1", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Equal(2, recomposedPositions.Count);
            Assert.Equal("chr1	2	.	AGC	AGA,GGG,TGA	45	PASS	RECOMPOSED	GT:PS	1|3:123	.	1|2:456", string.Join("\t", recomposedPositions[0].VcfFields));
            Assert.Equal("chr1	2	.	AGCTG	GGATC,GGGTG	30.1	PASS	RECOMPOSED	GT	.	1|2	.", string.Join("\t", recomposedPositions[1].VcfFields));
        }

        [Fact]
        public void VariantGenerator_FailedFilterTagGivenCorrectly_DotTreatedAsPass()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T,G	.	PASS	.	GT:PS	0|1:123	2/2:.	0|2:456", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A,G	.	.	.	GT:PS	1|1:301	1|2:.	1|2:456", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	FailedForSomeReason	.	GT:PS	.	1|0:.	0/1:456", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Equal(2, recomposedPositions.Count);
            Assert.Equal("chr1	2	.	AGC	AGA,GGG,TGA	.	PASS	RECOMPOSED	GT:PS	1|3:123	.	1|2:456", string.Join("\t", recomposedPositions[0].VcfFields));
            Assert.Equal("chr1	2	.	AGCTG	GGATC,GGGTG	.	FilteredVariantsRecomposed	RECOMPOSED	GT	.	1|2	.", string.Join("\t", recomposedPositions[1].VcfFields));
        }

        [Fact]
        public void VariantGenerator_MinQGUsed_DotAndNullIgnored()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T,G	.	PASS	.	GT:PS:GQ	0|1:123:.	2/2:.:14.2	0|2:456:.", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A,G	.	PASS	.	GT:PS:GQ	1|1:301:.	1|2:.:18	1|2:456:15.6", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT	.	1|0	0/1", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Equal(2, recomposedPositions.Count);
            Assert.Equal("chr1	2	.	AGC	AGA,GGG,TGA	.	PASS	RECOMPOSED	GT:GQ:PS	1|3:.:123	.	1|2:15.6:456", string.Join("\t", recomposedPositions[0].VcfFields));
            Assert.Equal("chr1	2	.	AGCTG	GGATC,GGGTG	.	PASS	RECOMPOSED	GT:GQ	.	1|2:14.2	.", string.Join("\t", recomposedPositions[1].VcfFields));
        }

        [Fact]
        public void VariantGenerator_SampleColumnCorrectlyProcessed_WhenTrailingMissingValuesDroped()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T,G	.	PASS	.	GT:PS:GQ	0|1:123	2/2:.:14.2	./.", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A,G	.	PASS	.	GT:PS:GQ	./.	1|2:.:18	1|2:456:15.6", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT	./.	1|0	./.", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Single(recomposedPositions);
            Assert.Equal("chr1	2	.	AGCTG	GGATC,GGGTG	.	PASS	RECOMPOSED	GT:GQ	.	1|2:14.2	.", string.Join("\t", recomposedPositions[0].VcfFields));
        }

        [Fact]
        public void VariantGenerator_AllTrailingMissingValuesDroped()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T,G	.	PASS	.	GT:GQ:PS	0|1:.:123	2/2	1|1:17:456", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A,G	.	PASS	.	GT:GQ:PS	./.	1|2	1|2:15.6:456", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT:GQ:PS	./.	1|0	1|1:13:456", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3 }, functionBlockRanges).ToList();

            Assert.Single(recomposedPositions);
            Assert.Equal("chr1	2	.	AGCTG	GGATC,GGGTG,TGATC,TGGTC	.	PASS	RECOMPOSED	GT:GQ:PS	.	1|2	3|4:13:456", string.Join("\t", recomposedPositions[0].VcfFields));
        }

        [Fact]
        public void VariantGenerator_RefAllelesAddedToMergeAlleles()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) } });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAACT"));
            var sequenceProvider = mockSequenceProvider.Object;

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T	.	PASS	.	GT	0|1	0|0	0|1	0|0", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	A	.	PASS	.	GT	1|1	1|1	1|1	1|1", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT	1|1	1|1	1|1	1|1", sequenceProvider.RefNameToChromosome);
            var position4 = SimplePosition.GetSimplePosition("chr1	8	.	A	G	.	PASS	.	GT	0|1	0|1	0|0	0|0", sequenceProvider.RefNameToChromosome);
            var functionBlockRanges = new List<int> { 4, 6, 8, 10 };

            var recomposer = new VariantGenerator(sequenceProvider);
            var recomposedPositions = recomposer.Recompose(new List<ISimplePosition> { position1, position2, position3, position4 }, functionBlockRanges).ToList();

            Assert.Single(recomposedPositions);
            Assert.Equal("chr1	2	.	AGCTGAA	AGATCAA,AGATCAG,TGATCAA,TGATCAG	.	PASS	RECOMPOSED	GT	1|4	1|2	1|3	1|1", string.Join("\t", recomposedPositions[0].VcfFields));
        }

    }
}