using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.VariantAnnotationTests.Algorithms
{
    public sealed class ReduceAltAlleleTests
    {        
        [Fact]
        public void SnvTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "A";
            const string altAllele = "C";

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newStart = newAlleles.Item1;
			var newAltAllele = newAlleles.Item3;
			// in this case the alt allele should be the same as new alt allele
			// and the start position should not change;
			Assert.Equal(start, newStart);
            Assert.Equal(altAllele, newAltAllele);
        }

        [Fact]
        public void InsertionTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "A";
            const string altAllele = "AC";

            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newStart = newAlleles.Item1;
			var newAltAllele = newAlleles.Item3;
			// in this case the alt allele should be the same as new alt allele
            // and the start position should not change;
            Assert.Equal(start+1, newStart);
            Assert.Equal("iC", newAltAllele);
        }

        [Fact]
        public void DeletionTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "ATCG";
            const string altAllele = "AT";

            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newStart = newAlleles.Item1;
			var newAltAllele = newAlleles.Item3;
			
			// in this case the alt allele should be the same as new alt allele
            // and the start position should not change;
            Assert.Equal(start + 2, newStart);
            Assert.Equal("2", newAltAllele);
        }

        [Fact]
        public void TrimBothEndsTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "ATCGGA";
            const string altAllele = "ATGA";

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

            var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;

			// in this case the alt allele should be the same as new alt allele
			// and the start position should not change;
			Assert.Equal("CG", newRefAllele);
            Assert.Equal("2", newAltAllele);
        }

        [Fact]
        public void DelInsTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "ATCG";
            const string altAllele = "CGT";

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;

			// in this case the alt allele should be the same as new alt allele
			// and the start position should not change;
			Assert.Equal(referenceAllele , newRefAllele);
            Assert.Equal("4CGT", newAltAllele);
        }

        [Fact]
        public void LongInsertionTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "AT";
            const string altAllele = "ATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA";

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;
			// in this case the alt allele should be the same as new alt allele
			// and the start position should not change;
			Assert.Equal("", newRefAllele);

            var expectedAltAllele = 'i' + altAllele.Substring(2); 
            Assert.Equal(expectedAltAllele, newAltAllele);
        }

        [Fact]
        public void LongDelInsTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "ATGCTTTAACGGGTATTTTTAAAAGGGGG";
            const string altAllele = "ATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA";

            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

	        var newStart = newAlleles.Item1;
			var newAltAllele = newAlleles.Item3;
			// in this case the alt allele should be the same as new alt allele
            // and the start position should not change;
            Assert.Equal(start + 2, newStart);

			const string expectedAltAllele = "27CGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA";
            Assert.Equal(expectedAltAllele, newAltAllele);
        }

		[Fact]
		public void LongDelInsTest1()
		{
			const int start = 1000; // represents the start position of the variant.
			const string referenceAllele = "AATGTGAAAAATATATTTTATATAATTTCAATATTTTTAACA";
			const string altAllele = "ATTGAAAAATATATTTTATATAATTTCAATATTTTTAACAT";

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newAltAllele = newAlleles.Item3;
			// in this case the alt allele should be the same as new alt allele
			// and the start position should not change;

			const string expectedAltAllele = "41TTGAAAAATATATTTTATATAATTTCAATATTTTTAACAT";
			Assert.Equal(expectedAltAllele, newAltAllele);
		}

        [Fact]
        public void LongDeletionTest()
        {
            const int start = 1000; // represents the start position of the variant.
            const string referenceAllele = "ATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA";
            const string altAllele = "AT";

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);

			var newStart = newAlleles.Item1;
			var newAltAllele = newAlleles.Item3;
			// in this case the alt allele should be the same as new alt allele
			// and the start position should not change;
			Assert.Equal(start + 2, newStart);

            Assert.Equal("39", newAltAllele);
        }

        [Fact]
        public void InsertionWithDashRef()
        {
            const int start = 100;
            const string referenceAllele = "-";
            const string altAllele = "AG";

            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);
            Assert.Equal("iAG",newAlleles.Item3);
            Assert.Equal(100,newAlleles.Item1);
        }

        [Fact]
        public void DeletionWithDashAlt()
        {
            const int start = 100;
            const string referenceAllele = "AG";
            const string altAllele = "-";

            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(start, referenceAllele, altAllele);
            Assert.Equal("2", newAlleles.Item3);
            Assert.Equal(100, newAlleles.Item1);
        }

        [Fact]
        public void ReverseReducedAlleleInsertion()
        {
            const string reducedAllele = "iACT";

            var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(reducedAllele);

            Assert.Equal("ACT", altAllele);
        }

        [Fact]
        public void ReverseReducedAlleleDeletion()
        {
            const string reducedAllele = "15";

            var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(reducedAllele);

            Assert.Equal("-", altAllele);
        }

        [Fact]
        public void ReverseReducedAlleleSnv()
        {
            const string reducedAllele = "T";

            var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(reducedAllele);

            Assert.Equal(reducedAllele, altAllele);
        }

        [Fact]
        public void ReverseReducedAlleleMnv()
        {
            const string reducedAllele = "TCGG";

            var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(reducedAllele);

            Assert.Equal(reducedAllele, altAllele);
        }

        [Fact]
        public void ReverseReducedAlleleDelIns()
        {
            const string reducedAllele = "15ATC";

            var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(reducedAllele);

            Assert.Equal("ATC", altAllele);
        }

        [Fact]
        public void ReverseReducedAlleleLongIns()
        {
            const string originalAlt = "iATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA";

            var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(originalAlt);

			Assert.Equal("ATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA", altAllele);
        }

		[Fact]
		public void ReverseReducedLongDelIns()
		{
			const string originalAlt = "47ATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA";

			var altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(originalAlt);

			Assert.Equal("ATCGGGGGAAAAAATTTTTCGCGCGTATATGAGACATTAAA", altAllele);
		}

    }
}
