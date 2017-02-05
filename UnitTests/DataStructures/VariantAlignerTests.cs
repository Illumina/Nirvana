using System;
using VariantAnnotation.Algorithms;
using Xunit;

namespace UnitTests.DataStructures
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
            var sequence = new VariantAligner.ReferenceSequence("TGGGGTGAGAATCATTGACATAATTGTAACAGGATAATATTCAGGAAATATGGAGATAAATAATTTTCTTCTCGACATTAAAAAAATCTAATAAAAAGTTTTATGTTTTCCCCTAACTCAGGGTCATCAGCCTTCAAGCTTCAGTCTCTGTGTGTTCACAGGTGCTGTAAACACACGCATCACTACTAATATCCCACTTCAGTGCTATTGCTGCTCCCAAAACTCCAGGTATTTTTAACCTTATAAACCTCCAGAATAATGAGACCACTGGGTTCAGTAAATTGCTTTGTTTTGAAGCACTATTAGACAAAGTGGGAGACTAGAAGATAAATCTGTCAATGACATGTCCTTTAAGACTACTTAGATTTTGTTGAATTTGTGGATCATTCCTTACTTGAGCAAATGGTAAATTAACTCTCTCTTTTCTCTCTCTCTCTAGCTGGCACACTTTTTCCAGTAGCCATTCTACTTGGTATGCTTACTTATCAGCTGTCCTCCAGGGGCCTCACATTAGATGTTTCTCTGA", 106514);

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
            var sequence = new VariantAligner.ReferenceSequence("GGCTAGCTAGCTTATTATATAT");
            var aligner = new VariantAligner(sequence);
            var leftAlignedVariant = aligner.LeftAlign(refPos, refAllele, altAllele);
            Assert.Equal(Tuple.Create(alignedPos, alignedRefAllele, alignedAltAllele), leftAlignedVariant);
        }

        [Theory]
        //insertion tests
        [InlineData("chr1", 5, "T", "TCA", 10, "", "CA")]
        [InlineData("chr1", 3, "C", "CG", 4, "", "G")]
        [InlineData("chr1", 2, "G", "GC", 5, "", "C")]
        //deletion tests
        [InlineData("chr1", 5, "TCA", "T", 8, "CA", "")]
        [InlineData("chr1", 7, "ACA", "A", 8, "CA", "")]
        public void RightAlignmentTest(string chromosome, int refPos, string refAllele, string altAllele, int alignedPos,
            string alignedRefAllele, string alignedAltAllele)
        {
            var sequence = new VariantAligner.ReferenceSequence("GGCCTCACATTTTAAAAGGGGCCCCCTATTATATAT");
            var aligner = new VariantAligner(sequence);
            var rightAlignedVar = aligner.RightAlign(refPos, refAllele, altAllele);

            Assert.Equal(Tuple.Create(alignedPos, alignedRefAllele, alignedAltAllele), rightAlignedVar);
        }

        [Fact]
        public void AlleleChangeDeletion()
        {
            var sequence = new VariantAligner.ReferenceSequence("AAAAAAAAAATTTTTTTTTTGGGGGGCTATTAACCCAAAAAAAAAATTTTTTTTTTGGGGGG");
            var aligner = new VariantAligner(sequence);

            var leftAlignedVariant = aligner.LeftAlign(29, "ATTA", "A");
            Assert.Equal(Tuple.Create(28, "TAT", ""), leftAlignedVariant);

            var rightAlingedVariant = aligner.RightAlign(27, "CTAT", "C");
            Assert.Equal(Tuple.Create(30, "TTA", ""), rightAlingedVariant);
        }
    }
}
