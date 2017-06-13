using UnitTests.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.Algorithms
{
    public sealed class VariantEffectTests
    {
        [Fact]
        public void IsInframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000416272_chr1_Ensembl84"),
                "chr1\t1850627\t.\tCAGCGGCAGG\tC\t86.00\tLowGQXHetDel\t.", "ENST00000416272", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IsInframeDeletion2()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000257290_chr4_Ensembl84"),
                "chr4\t55141051\t.\tGCCCAGATGGACATGA\tG\t1000\tPASS\t.", "ENST00000257290", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IsSpliceRegionVariantFalseAcceptorSpliceSite()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NM_000644_chr1_RefSeq84"),
                "chr1\t100316589\t.\tA\tG\t71.00\tPASS\t.", "NM_000644", "G");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("splice_region_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void SpliceDonorVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NM_152665_chr1_RefSeq84"),
                "chr1\t67242087\t.\tG\tA\t71.00\tPASS\t.", "NM_152665", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("splice_donor_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedFeatureElongation()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_034014_chr1_RefSeq84"),
                "chr1\t59286434\t.\tACACACACAC\tACACACACACAC\t71.00\tPASS\t.", "NR_034014", "AC");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("intron_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedFeatureTruncation()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_034014_chr1_RefSeq84"),
                "chr1\t59270922\t.\tAACACAA\tA\t71.00\tPASS\t.", "NR_034014", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("intron_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedFeatureTruncationInDownstreamVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_027120_chr1_RefSeq84"),
                "chr1\t59597491\t.\tATAT\tA\t71.00\tPASS\t.", "NR_027120", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("downstream_gene_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void InframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000275493_chr7_Ensembl84"),
                "chr7\t55242468\t.\tATTAAGAGAAGCAACATCTC\tAT\t1000\tPASS\t.", "ENST00000275493", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }
    }
}