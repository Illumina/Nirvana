using Genome;
using Moq;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class SmallVariantCreatorTests
    {
        [Fact]
        public void Small_variant_created_correctly()
        {
            var chrom = new Mock<IChromosome>();
            chrom.Setup(x => x.EnsemblName).Returns("1");
	        var variant = SmallVariantCreator.Create(chrom.Object, 100, "A", "ACG", false, false, null);
			Assert.False(variant.IsRefMinor);
            Assert.True(variant.Behavior.NeedFlankingTranscript);
            Assert.True(variant.Behavior.NeedSaPosition);
            Assert.False(variant.Behavior.NeedSaInterval);
            Assert.False(variant.Behavior.ReducedTranscriptAnnotation);
            Assert.False(variant.Behavior.StructuralVariantConsequence);
            Assert.Equal("1",variant.Chromosome.EnsemblName);
            Assert.Equal(101,variant.Start);
            Assert.Equal(100,variant.End);
            Assert.Equal("",variant.RefAllele);
            Assert.Equal("CG",variant.AltAllele);
            Assert.Equal(VariantType.insertion,variant.Type);
            Assert.False(variant.IsDecomposed);
            Assert.False(variant.IsRecomposed);
            Assert.Null(variant.LinkedVids);
        }



        [Theory]
        [InlineData("A", "G", "1:100:G")]
        [InlineData("ATC", "A", "1:101:102")]
        [InlineData("ATC", "TGC", "1:100:101:TG")]
        [InlineData("AT","GCTC","1:100:101:GCTC")]
        [InlineData("A","ATCTTCGTATGCCGTGTACTGAAATGCATCGCTGTACGTCACTGCGTGATGCTGAT", "1:101:100:f5cfa699545be68b2bb9ef7a288804ed")]
        public void vid_compute_correctly(string refAllele,string altAllele,string expId)
        {
            var chrom = new Mock<IChromosome>();
            chrom.Setup(x => x.EnsemblName).Returns("1");
	        var variant = SmallVariantCreator.Create(chrom.Object, 100, refAllele, altAllele, false, false, null);
			Assert.Equal(expId, variant.VariantId);
        }
    }
}