using CacheUtils.TranscriptCache;
using Genome;
using UnitTests.TestUtilities;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class ReferenceVariantCreatorTests
    {
        private static readonly ISequence Sequence = new NSequence();
        private readonly VariantId _vidCreator = new VariantId();

        [Fact]
        public void Create_SinglePosition_NoGlobalMajorAllele_ReturnNull()
        {
            IVariant[] variants = ReferenceVariantCreator.Create(_vidCreator, Sequence, ChromosomeUtilities.Chr1, 100, 100, "A", ".", null);
            Assert.Null(variants);
        }

        [Fact]
        public void Create_SinglePosition_HasGlobalMajorAllele_ReturnVariant()
        {
            var variant = GetVariant(100, 100, "A", ".", "T");
            Assert.True(variant.IsRefMinor);
        }

        [Fact]
        public void Create_MultiplePositions_NoGlobalMajorAllele_ReturnNull()
        {
            IVariant[] variants = ReferenceVariantCreator.Create(_vidCreator, Sequence, ChromosomeUtilities.Chr1, 100, 101, "A", ".", null);
            Assert.Null(variants);
        }

        [Fact]
        public void Create_MultiplePositions_HasGlobalMajorAllele_ReturnNull()
        {
            IVariant[] variants = ReferenceVariantCreator.Create(_vidCreator, Sequence, ChromosomeUtilities.Chr1, 100, 101, "A", ".", "T");
            Assert.Null(variants);
        }

        private IVariant GetVariant(int start, int end, string refAllele, string altAllele, string globalMajorAllele)
        {
            IVariant[] variants = ReferenceVariantCreator.Create(_vidCreator, Sequence, ChromosomeUtilities.Chr1, start, end, refAllele, altAllele, globalMajorAllele);
            Assert.Single(variants);
            return variants[0];
        }
    }
}