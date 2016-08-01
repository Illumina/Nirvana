using System.Linq;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class RegulatoryFeatureTests
    {
        [Fact]
        public void RegulatoryRegionAblation()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion("ENSR00001584270_chr1_Ensembl84_reg.ndb",
                "1\t53103974\t.\tAGGCCCCTTTCTATCCAGGAACCCAGAGTTGTCCCACACAC\tA\t71.00\tPASS\t.", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_ablation&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void MissingRegulatoryFeature()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion("ENSR00001346772_chr17_Ensembl84_reg.ndb",
                "17\t41276247\t.\tA\tG\t71.00\tPASS\t.", "ENSR00001346772");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void MistakenAlleleSpecificRegulatoryFeature()
        {
            var annotatedVariant = DataUtilities.GetVariant("ENSR00001734900_chr22_Ensembl84_reg.ndb",
                "22	49090610	.	AGACCTCCGCCCCCACCCGCCGCCGCTCC	A,AGACCTCCGCCCCCACCCGCCGCCGCTCCA	131	LowGQXHetAltDel	CIGAR=1M28D,29M1I;RU=.,A;REFREP=1,0;IDREP=0,1	GT:GQ:GQX:DPI:AD	1/2:185:0:17:1,5,4");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);

            Assert.Contains("regulatoryRegions", altAllele);
            Assert.DoesNotContain("regulatoryRegions", altAllele2.ToString());
        }

        [Fact]
        public void RegulatoryRegionAblationForSV()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion("ENSR00001584270_chr1_Ensembl84_reg.ndb",
                "chr1	53103974	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=53104013", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_ablation&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void RegulatoryRegionAmplificationForSVDup()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion("ENSR00001584270_chr1_Ensembl84_reg.ndb",
                "chr1	53103974	.	G	<DUP>	.	PASS	SVTYPE=DUP;END=53104013", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_amplification&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

        [Fact]
        public void RegulatoryRegionVariantDeletion()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion("ENSR00001584270_chr1_Ensembl84_reg.ndb",
                "chr1	53103983	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=53104013", "ENSR00001584270");
            Assert.NotNull(regulatoryRegion);
            Assert.Contains("regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
        }

		[Fact]
		[Trait("jira", "NIR-1600")]
		public void RegulatoryRegionInsideIndel()
		{
			var regulatoryRegion = DataUtilities.GetRegulatoryRegion("ENSR00001584270_chr1_Ensembl84_reg.ndb",
				"chr1\t53103975\t.\tGCCCCTTTCTATCCAGGAACCCAGAGTTGTCCCACACACGG\tGA\t515\tPASS\t.", "ENSR00001584270");
			Assert.NotNull(regulatoryRegion);
			Assert.Equal("regulatory_region_ablation&regulatory_region_variant", string.Join("&", regulatoryRegion.Consequence));
		}
	}
}