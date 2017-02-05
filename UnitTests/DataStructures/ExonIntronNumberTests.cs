using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class ExonIntronNumberTests
    {
        [Fact]
        public void UnwantedExonNumber()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000569425_chr1_Ensembl84"),
                "chr1\t56714872\t.\tT\tTC\t1087.00\tPASS\t.", "ENST00000569425", "C");
            Assert.NotNull(transcriptAllele);

            Assert.Null(transcriptAllele.Exons);
            Assert.DoesNotContain("\"exons\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ExonNumber11()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000255416_chr1_Ensembl84"),
                "chr1\t203136983\t.\tA\tT\t1087.00\tPASS\t.", "ENST00000255416", "T");
            Assert.NotNull(transcriptAllele);

            Assert.Equal("11/11", transcriptAllele.Exons);
            Assert.Contains("\"exons\":\"11/11\"", transcriptAllele.ToString());
        }

        [Fact]
        public void IntronNumber18()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000327044_chr1_Ensembl84"),
                "chr1\t880238\t.\tA\tG\t1087.00\tPASS\t.", "ENST00000327044", "G");
            Assert.NotNull(transcriptAllele);

            Assert.Equal("18/18", transcriptAllele.Introns);
            Assert.Contains("\"introns\":\"18/18\"", transcriptAllele.ToString());
        }
    }
}