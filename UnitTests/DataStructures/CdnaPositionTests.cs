using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class CdnaPositionTests
    {
        [Fact]
        public void FlankingInsertionBoth()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000486318_chr1_Ensembl84.ndb",
                "chr1\t17278450\t.\tA\tAGGACGT\t217.00\tPASS\t.", "ENST00000486318", "GGACGT");
            Assert.NotNull(transcriptAllele);

            const string expectedComplementaryDnaPosition = null;
            var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
            Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
        }

	    [Theory]		
		[InlineData("chr1	17270619	.	N	NA	.	.	.",null)]
		[InlineData("chr1	17270620	.	N	NA	.	.	.", "1-2")]
		[InlineData("chr1	17270776	.	N	NA	.	.	.", "157-158")]
		[InlineData("chr1	17270777	.	N	NA	.	.	.", "158-159")] //VEP: 158-159
		[InlineData("chr1	17270778	.	N	NA	.	.	.", null)]
		[InlineData("chr1	17271980	.	N	NA	.	.	.","182-183")]
		[InlineData("chr1	17278449	.	N	NA	.	.	.", "723-724")]
		[InlineData("chr1	17278450	.	A	AGGACGT	.	.	.", null)]
		[InlineData("chr1	17271900	.	A	AGGACGT	.	.	.",null)]
		[InlineData("chr1	17271950	.	CCCACAGCCATAGACAACTAGAGCAGCTGGAAGGGAAGCGCTCAGTCCTGGCCAAGGAGCTGGTGGAGGTGAGGGAGGCGCTGAGCCGCGCCACACTGCAACGGGACATGCTGCAGGCCGAGAAGGCCGAGGTGGCCGAGGCGCTGACCAAGGTGGGTCCC	C	.	.	.",null)]
		public void CdnaPositionOnForwardTranscript(string vcfLine, string expectedComplementaryDnaPosition)
	    {
			var transcriptAllele = DataUtilities.GetTranscript("ENST00000486318_chr1_Ensembl84.ndb",vcfLine, "ENST00000486318");
			Assert.NotNull(transcriptAllele);

			var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
			Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
		}


		[Theory]
		[InlineData("chr1	59096779	.	G	GA	.	.	.", "285-286")]
		[InlineData("chr1	59097066	.	N	NA	.	.	.", null)]
		[InlineData("chr1	59097065	.	N	NA	.	.	.", "1-2")]
		[InlineData("chr1	59096781	.	N	NA	.	.	.", "285-286")]
		[InlineData("chr1	59096782	.	N	NA	.	.	.", "284-285")]
		[InlineData("chr1	59096686	.	N	NA	.	.	.", null)]
		[InlineData("chr1	59096687	.	N	NA	.	.	.", "377-378")]
		public void CdnaPositionOnReverseTranscript(string vcfLine, string expectedComplementaryDnaPosition)
		{
			var transcriptAllele = DataUtilities.GetTranscript("ENST00000330659_chr1_Ensembl84.ndb",
				vcfLine, "ENST00000330659");
			Assert.NotNull(transcriptAllele);

			var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
			Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
		}

		[Fact]
        public void NoCdnaPositions()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000470983_chr2_Ensembl84.ndb",
                "2	25469623	COSM3719947	TTCCCCGCGCGGCTGCTGGCCACC	T	.	.	.", "ENST00000470983");
            Assert.NotNull(transcriptAllele);

            const string expectedComplementaryDnaPosition = null;
            var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
            Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
        }

        [Fact]
        public void GappedStartPositionDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000419924_chr1_Ensembl84.ndb",
                "chr1\t3566625\t.\tAGCAGGCTG\tA\t1244.00\tPASS\t.", "ENST00000419924", "");
            Assert.NotNull(transcriptAllele);

            const string expectedComplementaryDnaPosition = "?-2";
            var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
            Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
        }

        [Fact]
        public void GappedEndPositionDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000451250_chr1_Ensembl84.ndb",
                "chr1\t55683603\t.\tCAAGT\tC\t577.00\tPASS\t.", "ENST00000451250", "");
            Assert.NotNull(transcriptAllele);

            const string expectedComplementaryDnaPosition = "71-?";
            var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
            Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
        }


        [Fact]
        public void MissingPositionInsertion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000330659_chr1_Ensembl84.ndb",
                "1\t59096779\t.\tG\tGA\t2220.00\tPASS\t.", "ENST00000330659", "A");
            Assert.NotNull(transcriptAllele);

            const string expectedComplementaryDnaPosition = "285-286";
            var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
            Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
        }

        [Fact]
        public void MissingEndPosition()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000288135_chr4_Ensembl84.ndb",
                "4	55593487	COSM1156	ACAGGTAACCATTTATTTGTTCTCTCTCCAGAGTGCTCTAATGACTGAGACAATAATTATTAAAAGGTGATCTATTTTTCCCTTTCTCCCCACAGAAACCCATGTATGAAG	AA	.	.	.",
                "ENST00000288135", "A");
            Assert.NotNull(transcriptAllele);

            const string expectedComplementaryDnaPosition = "1742-1760";
            var observedComplementaryDnaPosition = transcriptAllele.ComplementaryDnaPosition;
            Assert.Equal(expectedComplementaryDnaPosition, observedComplementaryDnaPosition);
        }
    }
}