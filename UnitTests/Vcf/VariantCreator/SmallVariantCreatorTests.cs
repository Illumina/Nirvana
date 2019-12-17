using Genome;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class SmallVariantCreatorTests
    {
        private static readonly IChromosome Chr1 = new Chromosome("chr1", "1", 0);

        [Fact]
        public void Create_Insertion_ReturnVariant()
        {
            var variant = SmallVariantCreator.Create(Chr1, 101, 100, "", "CG", false, false, null, null, false);
            Assert.False(variant.IsRefMinor);
            Assert.Equal(AnnotationBehavior.SmallVariants, variant.Behavior);
            Assert.Equal("1", variant.Chromosome.EnsemblName);
            Assert.Equal(101, variant.Start);
            Assert.Equal(100, variant.End);
            Assert.Equal("", variant.RefAllele);
            Assert.Equal("CG", variant.AltAllele);
            Assert.Equal(VariantType.insertion, variant.Type);
            Assert.False(variant.IsDecomposed);
            Assert.False(variant.IsRecomposed);
            Assert.Null(variant.LinkedVids);
        }
    }
}