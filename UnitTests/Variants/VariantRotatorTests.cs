using Genome;
using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class VariantRotatorTests
    {
        private static readonly IChromosome Chromosome = new Chromosome("chrBob", "bob", 2);

        private readonly ISequence _refSequence =
            new SimpleSequence(
                new string('A', VariantRotator.MaxDownstreamLength) + "ATGTGTGTGTGCAGT" +
                new string('A', VariantRotator.MaxDownstreamLength), 965891);

        [Fact]
        public void Right_Deletion_ForwardStrand()
        {
            // chr1	966391	.	ATG	A	2694.00	PASS	.
            var variant = GetDeletion();

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(966300);
            transcript.SetupGet(x => x.End).Returns(966405);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);

            var rotatedVariant = VariantRotator.Right(variant, transcript.Object, _refSequence, transcript.Object.Gene.OnReverseStrand);

            Assert.False(ReferenceEquals(variant, rotatedVariant));
            Assert.Equal(966400, rotatedVariant.Start);
            Assert.Equal("TG", rotatedVariant.RefAllele);
        }

        [Fact]
        public void Right_Deletion_ReverseStrand()
        {
            var variant = new SimpleVariant(Chromosome, 966399, 966401, "TG", "", VariantType.deletion);

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(966300);
            transcript.SetupGet(x => x.End).Returns(966405);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);

            var rotatedVariant = VariantRotator.Right(variant, transcript.Object, _refSequence, transcript.Object.Gene.OnReverseStrand);

            Assert.False(ReferenceEquals(variant, rotatedVariant));
            Assert.Equal(966393, rotatedVariant.Start);
            Assert.Equal("TG", rotatedVariant.RefAllele);
        }

        [Fact]
        public void Right_Insertion()
        {
            var variant = GetInsertion();

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(966300);
            transcript.SetupGet(x => x.End).Returns(966405);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);

            var rotated = VariantRotator.Right(variant, transcript.Object, _refSequence, transcript.Object.Gene.OnReverseStrand);

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
            var originalVariant = new SimpleVariant(Chromosome, 966392, 966392, "T", "A", VariantType.SNV);
            var rotated = VariantRotator.Right(originalVariant, null, _refSequence, false);
            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_VariantBeforeTranscript_ForwardStrand()
        {
            var originalVariant = GetDeletion();

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(966397);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);

            var rotated = VariantRotator.Right(originalVariant, transcript.Object, _refSequence, transcript.Object.Gene.OnReverseStrand);
            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_VariantBeforeTranscript_ReverseStrand()
        {
            var originalVariant = GetDeletion();

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.End).Returns(966390);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);

            var rotated = VariantRotator.Right(originalVariant, transcript.Object, _refSequence, transcript.Object.Gene.OnReverseStrand);
            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_InsertionVariantBeforeTranscript_ForwardStrand()
        {
            var originalVariant = GetInsertion();

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.End).Returns(966392);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);

            var rotated = VariantRotator.Right(originalVariant, transcript.Object, _refSequence, transcript.Object.Gene.OnReverseStrand);
            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        [Fact]
        public void Right_Identity_WithNoRotation()
        {
            var originalVariant = GetDeletion();

            ISequence refSequence = new SimpleSequence(
                new string('A', VariantRotator.MaxDownstreamLength) + "GAGAGTTAGGTA" +
                new string('A', VariantRotator.MaxDownstreamLength), 965891);

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(966300);
            transcript.SetupGet(x => x.End).Returns(966405);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);

            var rotated = VariantRotator.Right(originalVariant, transcript.Object, refSequence, transcript.Object.Gene.OnReverseStrand);
            Assert.True(ReferenceEquals(originalVariant, rotated));
        }

        private static ISimpleVariant GetDeletion() =>
            new SimpleVariant(Chromosome, 966392, 966394, "TG", "", VariantType.deletion);

        private static ISimpleVariant GetInsertion() =>
            new SimpleVariant(Chromosome, 966397, 966396, "", "TG", VariantType.insertion);

        [Theory]
        [InlineData(519, "TG", 515, "TG")]
        [InlineData(511, "ATT", 509, "TTA")]
        [InlineData(508, "GTT", 504, "TGT")]
        public void Left_align_deletions(int position, string refAllele, int rotatedPos, string rotatedRef)
        {
            var reference = new SimpleSequence(new string('A', VariantUtils.MaxUpstreamLength) + "ATGTGTTGTTATTCTGTGTGCAT");

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
            var reference = new SimpleSequence(new string('A', VariantUtils.MaxUpstreamLength) + "ATGTGTTGTTATTCTGTGTGCAT");

            var rotatedVariant = VariantUtils.TrimAndLeftAlign(position, "", altAllele, reference);

            Assert.Equal(rotatedPos, rotatedVariant.start);
            Assert.Equal(rotatedAlt, rotatedVariant.altAllele);
        }

        [Fact]
        public void Left_align_multiple_padding_bases()
        {
            var reference = new SimpleSequence(new string('A', VariantUtils.MaxUpstreamLength) + "ATGTGTTGTTATTCTGTGTGCAT");

            var rotatedVariant = VariantUtils.TrimAndLeftAlign(501, "AT", "ATT", reference);

            Assert.Equal(502, rotatedVariant.start);
            Assert.Equal("T", rotatedVariant.altAllele);
        }
        [Theory]
        [InlineData("TC", "T", false)]
        [InlineData("T", "TC", false)]
        [InlineData("T", "TCT", true)]
        [InlineData("TCT", "T", true)]
        [InlineData("TCT", "TA", true)] // no conclusion for indels
        [InlineData("TC", "AT", true)]//no conclusion for mnvs
        [InlineData("T", "A", false)]
        [InlineData("T", "T", false)]
        public void CanNotLeftRotate(string refAllele, string altAllele, bool result)
        {
            Assert.Equal(result, VariantUtils.IsLeftShiftPossible(refAllele, altAllele));
        }

    }


}