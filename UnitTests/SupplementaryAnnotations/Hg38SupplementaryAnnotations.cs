using System.Linq;
using System.Text.RegularExpressions;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.SupplementaryAnnotations
{
    public sealed class Hg38SupplementaryAnnotations
    {
        [Fact]
        public void AlleleSpecificCosmic()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chr1_111241360_111241361.nsa"),
                "chr1	111241360	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14");
            Assert.NotNull(annotatedVariant);

            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"id\":\"COSM3996742\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"id\":\"COSM4591038\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"id\":544").Count);
            Assert.Equal(2, Regex.Matches(annotatedVariant.ToString(), "\"gene\":\"CHI3L2\"").Count);

            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"histology\":\"haematopoietic neoplasm\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"histology\":\"osteosarcoma\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"histology\":\"carcinoma\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"histology\":\"lymphoid neoplasm\"").Count);

            Assert.Equal(2, Regex.Matches(annotatedVariant.ToString(), "\"primarySite\":\"haematopoietic and lymphoid tissue\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"primarySite\":\"upper aerodigestive tract\"").Count);
            Assert.Equal(1, Regex.Matches(annotatedVariant.ToString(), "\"primarySite\":\"bone\"").Count);
        }

        [Fact]
        public void UnwantedRefMinor()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnotGRCh38("chr1_26213904_26213905.nsa"));
            VcfUtilities.FieldContains(saReader,
                "chr1	26213904	.	C	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	0/0:3:1:1", "RefMinor", VcfCommon.InfoIndex);
        }

        [Fact]
        public void RefMinorChr1()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chr1_15274_15275.nsa"),
                "chr1	15274	.	C	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	0/0:3:1:1");
            Assert.NotNull(annotatedVariant);
            Assert.True(annotatedVariant.AnnotatedAlternateAlleles.First().IsReferenceMinor);
        }

		[Fact]
		public void ClinvarPubmed()
		{
			//4       15587928        rs386833761     AG      A       .       .       RS=386833761;RSPOS=15587929;
			var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chr4_15587928_15587929.nsa"),
				"4	15587928	.	AG	A	.	.	.");
			Assert.NotNull(annotatedVariant);

			Assert.Contains("pubMedIds\":[\"19466712\",\"19777577\"]", annotatedVariant.ToString());
			Assert.Contains("pubMedIds\":[\"25741868\",\"26092869\"]", annotatedVariant.ToString());
		}

		[Fact]
        public void RefMinorCh2()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chr2_13862188_13862189.nsa"),
                "chr2	13862188	.	C	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	0/0:3:1:1");
            Assert.NotNull(annotatedVariant);
            Assert.True(annotatedVariant.AnnotatedAlternateAlleles.First().IsReferenceMinor);
        }

        [Fact]
        public void RefMinorChr4()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chr4_18442594_18442594.nsa"),
                "chr4	18442594	.	C	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	0/0:3:1:1");
            Assert.NotNull(annotatedVariant);
            Assert.True(annotatedVariant.AnnotatedAlternateAlleles.First().IsReferenceMinor);
        }

        [Fact]
        public void DuplicateOneKentry()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chrX_5331877_5331879.nsa"),
                "X	5331877	.	AAC	A	100	PASS	AC=2620;AF=0.523163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("oneKgAll", annotatedVariant.ToString());
        }

        [Fact]
        public void MissingClinVarVcf()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnotGRCh38("chr17_75516562_75516563.nsa"));
            VcfUtilities.FieldEquals(saReader, "17	75516562	rs398124622	T	TGGAGCC	.	.	.",
                "AF1000G=0.059105;clinvar=1|conflicting_interpretations_of_pathogenicity", VcfCommon.InfoIndex);
        }

        [Fact]
        public void ClinVarUnknownAllele()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnotGRCh38("chr13_39724500_39724501.nsa"),
                "chr13	39724500	.	TTA	T	222	PASS	CIGAR=1M2D");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal(
				"{\"altAllele\":\"-\",\"refAllele\":\"TA\",\"begin\":39724501,\"chromosome\":\"chr13\",\"dbsnp\":[\"rs796887249\"],\"end\":39724502,\"globalMinorAllele\":\"A\",\"gmaf\":0.008786,\"variantType\":\"deletion\",\"vid\":\"13:39724501:39724502\",\"cosmic\":[{\"id\":\"COSM4214738\",\"refAllele\":\"T\",\"altAllele\":\"A\",\"gene\":\"COG6\",\"sampleCount\":1,\"studies\":[{\"id\":646,\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"}]},{\"id\":\"COSM3730300\",\"isAlleleSpecific\":true,\"refAllele\":\"TA\",\"altAllele\":\"-\",\"gene\":\"COG6\",\"sampleCount\":1,\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"oesophagus\"}]},{\"id\":\"COSM3730301\",\"isAlleleSpecific\":true,\"refAllele\":\"TA\",\"altAllele\":\"-\",\"gene\":\"COG6_ENST00000416691\",\"sampleCount\":1,\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"oesophagus\"}]},{\"id\":\"COSM4214739\",\"refAllele\":\"T\",\"altAllele\":\"A\",\"gene\":\"COG6_ENST00000416691\",\"sampleCount\":1,\"studies\":[{\"id\":646,\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"}]}]}",
                altAllele);
        }
    }
}
