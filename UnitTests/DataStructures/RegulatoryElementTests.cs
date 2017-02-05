using System.IO;
using UnitTests.Utilities;
using VariantAnnotation.Algorithms.Consequences;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class RegulatoryElementTests
    {
        [Fact]
        public void ReadWriteTests()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var regulatoryFeature1 = new RegulatoryElement(2, 100, 200, CompactId.Convert("1"), RegulatoryElementType.promoter);
            var regulatoryFeature2 = new RegulatoryElement(3, 100, 200, CompactId.Convert("2"), RegulatoryElementType.enhancer);
            var regulatoryFeature3 = new RegulatoryElement(4, 105, 201, CompactId.Convert("3"), RegulatoryElementType.CTCF_binding_site);

            using (var writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(randomPath)))
            {
                regulatoryFeature1.Write(writer);
                regulatoryFeature2.Write(writer);
                regulatoryFeature3.Write(writer);
            }

            using (var reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(randomPath)))
            {
                Assert.Equal(regulatoryFeature1, RegulatoryElement.Read(reader));
                Assert.Equal(regulatoryFeature2, RegulatoryElement.Read(reader));
                Assert.Equal(regulatoryFeature3, RegulatoryElement.Read(reader));
            }

            File.Delete(randomPath);
        }

        [Fact]
        public void RegulatoryFeatureEqualityTests()
        {
            var regulatoryFeature1 = new RegulatoryElement(2, 100, 200, CompactId.Convert("1"), RegulatoryElementType.promoter);
            var regulatoryFeature2 = new RegulatoryElement(2, 100, 200, CompactId.Convert("1"), RegulatoryElementType.promoter);
            var regulatoryFeature3 = new RegulatoryElement(4, 105, 201, CompactId.Convert("3"), RegulatoryElementType.CTCF_binding_site);

            Assert.Equal(regulatoryFeature1, regulatoryFeature2);
            Assert.False(regulatoryFeature1 == regulatoryFeature3);
        }

        [Fact]
        public void RegulatoryFeatureToStringTests()
        {
            var regulatoryFeature1 = new RegulatoryElement(2, 100, 200, CompactId.Convert("1"), RegulatoryElementType.promoter);
            Assert.NotNull(regulatoryFeature1.ToString());
        }

        [Fact]
        public void RegulatoryRegionAblation()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg"),
                "1\t53103974\t.\tAGGCCCCTTTCTATCCAGGAACCCAGAGTTGTCCCACACAC\tA\t71.00\tPASS\t.", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_ablation&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void MissingRegulatoryFeature()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001346772_chr17_Ensembl84_reg"),
                "17\t41276247\t.\tA\tG\t71.00\tPASS\t.", "ENSR00001346772");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains(Consequences.RegulatoryRegionVariantKey, string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void MistakenAlleleSpecificRegulatoryFeature()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENSR00001734900_chr22_Ensembl84_reg"),
                "22	49090610	.	AGACCTCCGCCCCCACCCGCCGCCGCTCC	A,AGACCTCCGCCCCCACCCGCCGCCGCTCCA	131	LowGQXHetAltDel	CIGAR=1M28D,29M1I;RU=.,A;REFREP=1,0;IDREP=0,1	GT:GQ:GQX:DPI:AD	1/2:185:0:17:1,5,4");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles[0];
            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles[1];

            Assert.Contains("regulatoryRegions", altAllele.ToString());
            Assert.DoesNotContain("regulatoryRegions", altAllele2.ToString());
        }

        [Fact]
        public void RegulatoryRegionAblationForSV()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg"),
                "chr1	53103974	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=53104013", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_ablation&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void RegulatoryRegionAmplificationForSVDup()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg"),
                "chr1	53103974	.	G	<DUP>	.	PASS	SVTYPE=DUP;END=53104013", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_amplification&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void RegulatoryRegionVariantDeletion()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg"),
                "chr1	53103983	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=53104013", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        [Trait("jira", "NIR-1600")]
        public void RegulatoryRegionInsideIndel()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg"),
                "chr1\t53103975\t.\tGCCCCTTTCTATCCAGGAACCCAGAGTTGTCCCACACACGG\tGA\t515\tPASS\t.", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Equal("regulatory_region_ablation&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }
    }
}