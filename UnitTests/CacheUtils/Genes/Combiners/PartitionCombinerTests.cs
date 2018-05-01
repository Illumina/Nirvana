using System.Collections.Generic;
using CacheUtils.Genes.Combiners;
using CacheUtils.Genes.DataStructures;
using Genome;
using Intervals;
using Xunit;

namespace UnitTests.CacheUtils.Genes.Combiners
{
    public sealed class PartitionCombinerTests
    {
        private readonly IChromosome _chr1           = new Chromosome("chr1", "1", 0);
        private readonly PartitionCombiner _combiner = new PartitionCombiner();

        [Fact]
        public void Combine_MergeIfSameIds_EntrezGeneOnly()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new HashSet<UgaGene> { new UgaGene(_chr1, interval, null, true, "102466751", null, "MIR6859-1", 50039) };
            var uga38 = new HashSet<UgaGene> { new UgaGene(_chr1, null, interval, true, "102466751", null, "MIR6859-1", 50039) };

            var observedResults = new List<UgaGene>();
            _combiner.Combine(observedResults, uga37, uga38);

            Assert.Single(observedResults);

            var observedGene = observedResults[0];
            Assert.Equal("102466751", observedGene.EntrezGeneId);
            Assert.Null(observedGene.EnsemblId);
            Assert.Equal(interval, observedGene.GRCh37);
            Assert.Equal(interval, observedGene.GRCh38);
        }

        [Fact]
        public void Combine_MergeIfSameIds_EnsemblOnly()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new HashSet<UgaGene> { new UgaGene(_chr1, interval, null, true, null, "ENSG00000278267", "MIR6859-1", 50039) };
            var uga38 = new HashSet<UgaGene> { new UgaGene(_chr1, null, interval, true, null, "ENSG00000278267", "MIR6859-1", 50039) };

            var observedResults = new List<UgaGene>();
            _combiner.Combine(observedResults, uga37, uga38);

            Assert.Single(observedResults);

            var observedGene = observedResults[0];
            Assert.Equal("ENSG00000278267", observedGene.EnsemblId);
            Assert.Null(observedGene.EntrezGeneId);
            Assert.Equal(interval, observedGene.GRCh37);
            Assert.Equal(interval, observedGene.GRCh38);
        }

        [Fact]
        public void Combine_DoNotCombine_MixedIds()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new HashSet<UgaGene> { new UgaGene(_chr1, interval, null, true, "102466751", null, "MIR6859-1", 50039) };
            var uga38 = new HashSet<UgaGene> { new UgaGene(_chr1, null, interval, true, "102466751", "ENSG00000278267", "MIR6859-1", 50039) };

            var observedResults = new List<UgaGene>();
            _combiner.Combine(observedResults, uga37, uga38);

            Assert.Equal(2, observedResults.Count);
            Assert.Equal("ENSG00000278267", observedResults[0].EnsemblId);
            Assert.Null(observedResults[1].EnsemblId);            
        }
    }
}
