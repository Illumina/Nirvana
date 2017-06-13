using System;
using UnitTests.Mocks;
using VariantAnnotation.Algorithms;
using Xunit;

namespace UnitTests.VariantAnnotationTests.Algorithms
{
    public class VariantAlignerTests
    {
        [Theory]
        //insertion tests
        [InlineData("chr1", 107024, "A", "ACA", 107021, "", "CA")]
        [InlineData("chr1", 107018, "C", "CG", 107019, "", "G")]
        [InlineData("chr1", 107019, "C", "CC", 107018, "", "C")]
        //deletion tests
        [InlineData("chr1", 107022, "ACA", "A", 107021, "CA", "")]
        public void LeftAlignmentTest(string chromosome, int refPos, string refAllele, string altAllele, int alignedPos,
            string alignedRefAllele, string alignedAltAllele)
        {
            var sequence = new MockReferenceSequence("TGGGGTGAGAATCATTGACATAATTGTAACAGGATAATATTCAGGAAATATGGAGATAAATAATTTTCTTCTCGACATTAAAAAAATCTAATAAAAAGTTTTATGTTTTCCCCTAACTCAGGGTCATCAGCCTTCAAGCTTCAGTCTCTGTGTGTTCACAGGTGCTGTAAACACACGCATCACTACTAATATCCCACTTCAGTGCTATTGCTGCTCCCAAAACTCCAGGTATTTTTAACCTTATAAACCTCCAGAATAATGAGACCACTGGGTTCAGTAAATTGCTTTGTTTTGAAGCACTATTAGACAAAGTGGGAGACTAGAAGATAAATCTGTCAATGACATGTCCTTTAAGACTACTTAGATTTTGTTGAATTTGTGGATCATTCCTTACTTGAGCAAATGGTAAATTAACTCTCTCTTTTCTCTCTCTCTCTAGCTGGCACACTTTTTCCAGTAGCCATTCTACTTGGTATGCTTACTTATCAGCTGTCCTCCAGGGGCCTCACATTAGATGTTTCTCTGA", 106514);

            var aligner = new VariantAligner(sequence);
            var leftAlignedVariant = aligner.LeftAlign(refPos, refAllele, altAllele);

            Assert.Equal(Tuple.Create(alignedPos, alignedRefAllele, alignedAltAllele), leftAlignedVariant);
        }

        [Theory]
        [InlineData("chr1", 7, "CTAGC", "C", 2, "GCTA", "")]
        [InlineData("chr1", 20, "TAT", "T", 16, "TA", "")]
        public void LeftAlignmentAlleleChange(string chromosome, int refPos, string refAllele, string altAllele,
            int alignedPos, string alignedRefAllele, string alignedAltAllele)
        {
            var sequence = new MockReferenceSequence("GGCTAGCTAGCTTATTATATAT");
            var aligner = new VariantAligner(sequence);
            var leftAlignedVariant = aligner.LeftAlign(refPos, refAllele, altAllele);
            Assert.Equal(Tuple.Create(alignedPos, alignedRefAllele, alignedAltAllele), leftAlignedVariant);
        }

        [Fact]
        public void AlleleChangeDeletion()
        {
            var sequence = new MockReferenceSequence("AAAAAAAAAATTTTTTTTTTGGGGGGCTATTAACCCAAAAAAAAAATTTTTTTTTTGGGGGG");
            var aligner = new VariantAligner(sequence);

            var leftAlignedVariant = aligner.LeftAlign(29, "ATTA", "A");
            Assert.Equal(Tuple.Create(28, "TAT", ""), leftAlignedVariant);
        }
    }
}
