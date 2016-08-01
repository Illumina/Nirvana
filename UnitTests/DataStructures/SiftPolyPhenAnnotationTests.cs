using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class SiftPolyPhenAnnotationTests
    {
        [Fact]
        public void PolyPhenBenign()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000487053_chr1_Ensembl84.ndb",
                "chr1\t1558792\t.\tT\tC\t1060.00\tPASS\t.", "ENST00000487053", "C");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"polyPhenScore\":0.042,\"polyPhenPrediction\":\"benign\"", transcriptAllele.ToString());
        }

        [Fact]
        public void SiftDeleterious()
        {
            var transcriptAllele = DataUtilities.GetTranscript("chr1_115256529_G_TAA_RefSeq84_pos.ndb",
                "chr1\t115256529\t.\tT\tA\t.\tPASS\t.\tGT:GQX:DP:DPF\t0/0:99:34:2", "NM_002524.4", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"siftScore\":0,\"siftPrediction\":\"deleterious\"", transcriptAllele.ToString());
        }
    }
}