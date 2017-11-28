using System.IO;
using CacheUtils.Genes.Combiners;
using CacheUtils.Genes.DataStructures;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.CacheUtils.Genes.Combiners
{
    public sealed class CombinerUtilsTests
    {
        private readonly IChromosome _chr1 = new Chromosome("chr1", "1", 0);

        [Fact]
        public void Merge_DifferentCombinations()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new UgaGene(_chr1, interval, null, true, "102466751", null, "MIR6859-1", 50039);
            var uga38 = new UgaGene(_chr1, null, interval, true, null, "ENSG00000278267", "MIR6859-1", 50039);

            var observedResult = CombinerUtils.Merge(uga37, uga38);
            Assert.Equal("102466751", observedResult.EntrezGeneId);
            Assert.Equal("ENSG00000278267", observedResult.EnsemblId);
        }

        [Fact]
        public void Merge_ThrowException_IfValuesDifferent()
        {
            var interval = new Interval(17369, 17436);
            var uga37 = new UgaGene(_chr1, interval, null, true, "102466751", "ENSG00000278267", "MIR6859-1", 50039);
            var uga38 = new UgaGene(_chr1, null, interval, true, "000000000", "ENSG00000278267", "MIR6859-1", 50039);

            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var observedResult = CombinerUtils.Merge(uga37, uga38);
            });
        }
    }
}
