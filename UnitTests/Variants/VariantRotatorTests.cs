using Genome;
using Intervals;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class VariantRotatorTests
    {
        private readonly ISequence _refSequence =
            new SimpleSequence(
                new string('A', VariantRotator.MaxDownstreamLength) + "ATGTGTGTGTGCAGT" +
                new string('A', VariantRotator.MaxDownstreamLength), 965891);

        [Fact]
        public void Right_Deletion_ForwardStrand()
        {
            // chr1	966391	.	ATG	A	2694.00	PASS	.
            var       variant            = GetDeletion();
            IInterval transcriptInterval = new Interval(966300, 966405);

            var rotatedVariant = VariantRotator.Right(variant, transcriptInterval, _refSequence, false);

            Assert.False(ReferenceEquals(variant, rotatedVariant));
            Assert.Equal(966400, rotatedVariant.Start);
            Assert.Equal("TG", rotatedVariant.RefAllele);
        }

        [Fact]
        public void Right_Deletion_ReverseStrand()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 966399, 966401, "TG", "", VariantType.deletion);

            IInterval transcriptInterval = new Interval(966300, 966405);
            var       rotatedVariant     = VariantRotator.Right(variant, transcriptInterval, _refSequence, true);

            Assert.False(ReferenceEquals(variant, rotatedVariant));
            Assert.Equal(966393, rotatedVariant.Start);
            Assert.Equal("TG", rotatedVariant.RefAllele);
        }

        [Fact]
        public void Right_Insertion()
        {
            var variant = GetInsertion();

            IInterval transcriptInterval = new Interval(966300, 966405);
            var       rotated            = VariantRotator.Right(variant, transcriptInterval, _refSequence, false);

            Assert.False(ReferenceEquals(variant, rotated));
            Assert.Equal(966403, rotated.Start);
            Assert.Equal("TG", rotated.AltAllele);
        }

        [Fact]
        public void Right_Identity_WhenRefSequenceNull()
        {
            var originalVariant = GetDeletion();
            var rotatedVariant  = VariantRotator.Right(originalVariant, null, null, false);
            Assert.True(ReferenceEquals(originalVariant, rotatedVariant));
        }

        [Fact]
        public void Right_Identity_WhenNotInsertionOrDeletion()
        {
            var originalVariant =
                new SimpleVariant(ChromosomeUtilities.Chr1, 966392, 966392, "T", "A", VariantType.SNV);
            var rotated = VariantRotator.Right(originalVariant, null, _refSequence, false);
            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_VariantBeforeTranscript_ForwardStrand()
        {
            var originalVariant = GetDeletion();

            IInterval transcriptInterval = new Interval(966397, 966405);
            var       rotated = VariantRotator.Right(originalVariant, transcriptInterval, _refSequence, false);

            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_VariantBeforeTranscript_ReverseStrand()
        {
            var originalVariant = GetDeletion();

            IInterval transcriptInterval = new Interval(966380, 966390);
            var       rotated = VariantRotator.Right(originalVariant, transcriptInterval, _refSequence, true);

            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_InsertionVariantBeforeTranscript_ForwardStrand()
        {
            var originalVariant = GetInsertion();

            IInterval transcriptInterval = new Interval(966380, 966392);
            var       rotated = VariantRotator.Right(originalVariant, transcriptInterval, _refSequence, false);

            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_WithNoRotation()
        {
            var originalVariant = GetDeletion();

            ISequence refSequence = new SimpleSequence(
                new string('A', VariantRotator.MaxDownstreamLength) + "GAGAGTTAGGTA" +
                new string('A', VariantRotator.MaxDownstreamLength), 965891);

            IInterval transcriptInterval = new Interval(966300, 966405);
            var       rotated = VariantRotator.Right(originalVariant, transcriptInterval, refSequence, false);

            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        private static ISimpleVariant GetDeletion() =>
            new SimpleVariant(ChromosomeUtilities.Chr1, 966392, 966394, "TG", "", VariantType.deletion);

        private static ISimpleVariant GetInsertion() =>
            new SimpleVariant(ChromosomeUtilities.Chr1, 966397, 966396, "", "TG", VariantType.insertion);

        [Theory]
        [InlineData(519, "TG", 515, "TG")]
        [InlineData(511, "ATT", 509, "TTA")]
        [InlineData(508, "GTT", 504, "TGT")]
        public void Left_align_deletions(int position, string refAllele, int rotatedPos, string rotatedRef)
        {
            var reference =
                new SimpleSequence(new string('A', VariantUtils.MaxUpstreamLength) + "ATGTGTTGTTATTCTGTGTGCAT");

            var rotatedVariant = VariantUtils.TrimAndLeftAlign(position, refAllele, "", reference);

            Assert.Equal(rotatedPos, rotatedVariant.start);
            Assert.Equal(rotatedRef, rotatedVariant.refAllele);
        }

        [Theory]
        [InlineData(519, "TG", 515, "TG")]
        [InlineData(511, "ATT", 509, "TTA")]
        [InlineData(508, "GTT", 504, "TGT")]
        public void Left_align_insertion(int position, string altAllele, int rotatedPos, string rotatedAlt)
        {
            var reference =
                new SimpleSequence(new string('A', VariantUtils.MaxUpstreamLength) + "ATGTGTTGTTATTCTGTGTGCAT");

            var rotatedVariant = VariantUtils.TrimAndLeftAlign(position, "", altAllele, reference);

            Assert.Equal(rotatedPos, rotatedVariant.start);
            Assert.Equal(rotatedAlt, rotatedVariant.altAllele);
        }
    }
}