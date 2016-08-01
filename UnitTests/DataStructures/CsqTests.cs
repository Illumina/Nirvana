using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class CsqTests
    {
        /// <summary>
        /// Once upon a time we had non-unique transcripts in the Nirvana database.
        /// This unit test checks one of the problematic transcripts for an unusual amount
        /// of transcripts.
        /// </summary>
        [Fact]
        public void CorrectTranscriptCount()
        {
            var annotatedVariant = DataUtilities.GetVariant("chr1_59758869_T_G_UF_RefSeq84_pos.ndb",
                "chr1\t59758869\t.\tT\tG\t130.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckRefSeqTranscriptCount(7, altAllele);
        }

        [Fact]
        public void UnwantedTranscriptInsertion()
        {
            var annotatedVariant = DataUtilities.GetVariant("chr2_216305895_T_TAA_Ensembl84_pos.ndb",
                "chr2\t216305895\t.\tT\tTAA\t130.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);
        }

        [Fact]
        public void TranscriptInsertion5000()
        {
            var annotatedVariant = DataUtilities.GetVariant("chr1_51801236_A_AA_Ensembl84_pos.ndb",
                "chr1\t51801235\t.\tCA\tCAA,C\t134.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(0);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(7, altAllele);

            altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(8, altAllele);
        }

        [Fact]
        public void HashCode()
        {
            var csqEntry = new CsqEntry { Allele = "AC" };
            const int expectedHashCode = 768942877;
            Assert.Equal(expectedHashCode, csqEntry.GetHashCode());

            const int expectedHashCode2 = 358252778;
            csqEntry.Feature = "ENST00000288135";
            Assert.Equal(expectedHashCode2, csqEntry.GetHashCode());
        }

        [Fact]
        public void StringRepresentation()
        {
            var csqEntry = new CsqEntry { Allele = "AC", Feature = "ENST00000288135", Symbol = "NRAS" };

            const string expectedResult =
                "Allele:                     AC\nFeature:                    ENST00000288135\nSymbol:                     NRAS\n";
            var observedResult = csqEntry.ToString();

            Assert.Equal(expectedResult, observedResult);
        }
    }
}