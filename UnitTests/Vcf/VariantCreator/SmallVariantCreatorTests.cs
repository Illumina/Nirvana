using UnitTests.TestUtilities;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class SmallVariantCreatorTests
    {
        [Fact]
        public void Create_Insertion_ReturnVariant()
        {
            var variant = SmallVariantCreator.Create(ChromosomeUtilities.Chr1, 101, 100, "", "CG", false, false, null, null, false);
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