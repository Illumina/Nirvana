using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class SiftPolyPhenAnnotationTests
    {
        [Fact]
        public void PolyPhenBenign()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000487053_chr1_Ensembl84"),
                "chr1\t1558792\t.\tT\tC\t1060.00\tPASS\t.", "ENST00000487053", "C");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"polyPhenScore\":0.04,\"polyPhenPrediction\":\"benign\"", transcriptAllele.ToString());
        }

        [Fact]
        public void SiftDeleterious()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("chr1_115256529_G_TAA_RefSeq84_pos"),
                "chr1\t115256529\t.\tT\tA\t.\tPASS\t.\tGT:GQX:DP:DPF\t0/0:99:34:2", "NM_002524", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"siftScore\":0,\"siftPrediction\":\"deleterious\"", transcriptAllele.ToString());
        }

        [Fact(Skip="Need to update in order to truly test this issue")]
        public void SiftShouldBeSilent()
        {
            // TODO: Add more variants and transcripts to help pollute the SiftPrediction entry
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000328191_chr1_Ensembl84"),
                "chr1\t6635231\t.\tA\tG\t2965\tPASS\t.", "ENST00000328191");
            Assert.NotNull(transcriptAllele);

            var observedJson = transcriptAllele.ToString();
            Assert.DoesNotContain("\"siftScore\"", observedJson);
            Assert.DoesNotContain("\"siftPrediction\"", observedJson);
        }
    }
}