using System.Collections.Generic;
using CacheUtils.Genes.Combiners;
using CacheUtils.Genes.DataStructures;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.CacheUtils.Genes.Combiners
{
    public sealed class HgncIdCombinerTests
    {
        private readonly IChromosome _chr1        = new Chromosome("chr1", "1", 0);
        private readonly HgncIdCombiner _combiner = new HgncIdCombiner();

        [Fact]
        public void Combine_CombineWhenAllIdsMatch()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new HashSet<UgaGene> { new UgaGene(_chr1, interval, null, true, "102466751", "ENSG00000278267", "MIR6859-1", 50039) };
            var uga38 = new HashSet<UgaGene> { new UgaGene(_chr1, null, interval, true, "102466751", "ENSG00000278267", "MIR6859-1", 50039) };

            var observedResults = new List<UgaGene>();
            _combiner.Combine(observedResults, uga37, uga38);

            Assert.Single(observedResults);

            var observedGene = observedResults[0];
            Assert.Equal("102466751", observedGene.EntrezGeneId);
            Assert.Equal("ENSG00000278267", observedGene.EnsemblId);
            Assert.Equal(interval, observedGene.GRCh37);
            Assert.Equal(interval, observedGene.GRCh38);
        }

        [Fact]
        public void Combine_DoNotCombine_MixedStrands()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new HashSet<UgaGene> { new UgaGene(_chr1, interval, null, true, "102466751", "ENSG00000278267", "MIR6859-1", 50039) };
            var uga38 = new HashSet<UgaGene> { new UgaGene(_chr1, null, interval, false, "102466751", "ENSG00000278267", "MIR6859-1", 50039) };

            var observedResults = new List<UgaGene>();
            _combiner.Combine(observedResults, uga37, uga38);

            Assert.Equal(2, observedResults.Count);
            Assert.True(observedResults[0].OnReverseStrand);
            Assert.False(observedResults[1].OnReverseStrand);
        }

        [Fact]
        public void Combine_MIR6859_CombineWhenMissingGeneId()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new HashSet<UgaGene> { new UgaGene(_chr1, interval, null, true, "102466751", null, "MIR6859-1", 50039) };
            var uga38 = new HashSet<UgaGene> { new UgaGene(_chr1, null, interval, true, "102466751", "ENSG00000278267", "MIR6859-1", 50039) };

            var observedResults = new List<UgaGene>();
            _combiner.Combine(observedResults, uga37, uga38);

            Assert.Single(observedResults);

            var observedGene = observedResults[0];
            Assert.Equal("102466751", observedGene.EntrezGeneId);
            Assert.Equal("ENSG00000278267", observedGene.EnsemblId);
            Assert.Equal(interval, observedGene.GRCh37);
            Assert.Equal(interval, observedGene.GRCh38);
        }
    }
}
