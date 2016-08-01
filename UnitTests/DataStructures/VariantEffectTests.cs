using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class VariantEffectTests
    {
        [Fact]
        public void FivePrimeUtrVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000008349_UF_chr1_RefSeq84.ndb",
                "chr1\t90286636\t.\tAGCTGCC\tA\t71.00\tPASS\t.", "ENSESTT00000008349", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("splice_donor_variant&5_prime_UTR_variant&intron_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IsAfterCodingSpecialCase()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000064869_UF_chr1_RefSeq84.ndb",
                "chr1\t178514560\t.\tA\tAT\t71.00\tPASS\t.", "ENSESTT00000064869", "T");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("3_prime_UTR_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IsInframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000416272_chr1_Ensembl84.ndb",
                "chr1\t1850627\t.\tCAGCGGCAGG\tC\t86.00\tLowGQXHetDel\t.", "ENST00000416272", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IsInframeDeletion2()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000257290_chr4_Ensembl84.ndb",
                "chr4\t55141051\t.\tGCCCAGATGGACATGA\tG\t1000\tPASS\t.", "ENST00000257290", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IsSpliceRegionVariantFalseAcceptorSpliceSite()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NM_000644.2_chr1_RefSeq84.ndb",
                "chr1\t100316589\t.\tA\tG\t71.00\tPASS\t.", "NM_000644.2", "G");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("splice_region_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void SpliceDonorVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NM_152665.2_chr1_RefSeq84.ndb",
                "chr1\t67242087\t.\tG\tA\t71.00\tPASS\t.", "NM_152665.2", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("splice_donor_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopLost()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000012399_UF_chr1_RefSeq84.ndb",
                "chr1\t111773568\t.\tG\tC\t71.00\tPASS\t.", "ENSESTT00000012399", "C");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("stop_lost", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedFeatureElongation()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NR_034014.1_chr1_RefSeq84.ndb",
                "chr1\t59286434\t.\tACACACACAC\tACACACACACAC\t71.00\tPASS\t.", "NR_034014.1", "AC");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("intron_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedFeatureTruncation()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NR_034014.1_chr1_RefSeq84.ndb",
                "chr1\t59270922\t.\tAACACAA\tA\t71.00\tPASS\t.", "NR_034014.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("intron_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedFeatureTruncationInDownstreamVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NR_027120.1_chr1_RefSeq84.ndb",
                "chr1\t59597491\t.\tATAT\tA\t71.00\tPASS\t.", "NR_027120.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("downstream_gene_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void InframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000275493_chr7_Ensembl84.ndb",
                "chr7\t55242468\t.\tATTAAGAGAAGCAACATCTC\tAT\t1000\tPASS\t.", "ENST00000275493", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }
    }
}