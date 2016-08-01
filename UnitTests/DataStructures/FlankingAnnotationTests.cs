using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class FlankingAnnotationTests
    {
        [Fact]
        public void MissedUpstreamGeneVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000082723_UF_chr1_RefSeq84.ndb",
                "chr1\t59760493\t.\tAAA\tAA,A\t.\tPASS\t.", "ENSESTT00000082723", "");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("upstream_gene_variant", string.Join("&", transcriptAllele.Consequence));
        }
    }
}