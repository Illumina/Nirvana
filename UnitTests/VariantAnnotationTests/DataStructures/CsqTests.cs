using System.Collections.Generic;
using System.Linq;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures
{
    public sealed class CsqTests
    {
        [Fact]
        public void UnwantedTranscriptInsertion()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("chr2_216305895_T_TAA_Ensembl84_pos"), null as List<string>,
                "chr2\t216305895\t.\tT\tTAA\t130.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);
        }

        [Fact]
        public void TranscriptInsertion5000()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("chr1_51801236_A_AA_Ensembl84_pos"), null as List<string>,
                "chr1\t51801235\t.\tCA\tCAA,C\t134.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(0);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(7, altAllele);

            altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(8, altAllele);
        }
    }
}