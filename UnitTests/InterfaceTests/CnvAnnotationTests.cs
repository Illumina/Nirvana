using System.Collections.Generic;
using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.AnnotationSources;
using Xunit;

namespace UnitTests.InterfaceTests
{
    public sealed class CnvAnnotationTests
    {
        [Fact]
        public void PartiallyOverlappingGene()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19443800	14_19462000	G	<CNV>	.	PASS	SVTYPE=CNV;END=19462000;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("\"overlappingGenes\":[\"RP11-536C10.15\"]", annotatedVariant);
            AssertUtilities.CheckJsonContains("ENST00000546909", annotatedVariant);
        }

        [Fact]
        public void InternalGene()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19431000	14_19462000	G	<CNV>	.	PASS	SVTYPE=CNV;END=19462000;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("\"overlappingGenes\":[\"RP11-536C10.15\"]", annotatedVariant);
            AssertUtilities.CheckJsonDoesNotContain("ENST00000546909", annotatedVariant);
        }

        [Fact]
        public void InGene()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19443780	14_19443800	G	<CNV>	.	PASS	SVTYPE=CNV;END=19443800;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("\"overlappingGenes\":[\"RP11-536C10.15\"]", annotatedVariant);
            AssertUtilities.CheckJsonContains("ENST00000546909", annotatedVariant);
        }

        [Fact]
        public void IsTheSameAsGene()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19443725	14_19443847	G	<CNV>	.	PASS	SVTYPE=CNV;END=19443847;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("\"overlappingGenes\":[\"RP11-536C10.15\"]", annotatedVariant);
            AssertUtilities.CheckJsonDoesNotContain("ENST00000546909", annotatedVariant);
        }

        [Fact]
        public void NextToGene()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19443847	14_19462000	G	<CNV>	.	PASS	SVTYPE=CNV;END=19462000;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonDoesNotContain("\"overlappingGenes\":[\"RP11-536C10.15\"]", annotatedVariant);
            AssertUtilities.CheckJsonDoesNotContain("ENST00000546909", annotatedVariant);
        }

        [Fact]
        public void FlankingTranscript()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000427857_chr1_Ensembl84"), null as List<string>,
                "1	816800	Canvas:GAIN:1:816801:821943	N	<CNV>	2	q10;CLT10kb	SVTYPE=CNV;END=821943	RC:BC:CN	174:2:4");
            Assert.NotNull(annotatedVariant);

            Assert.DoesNotContain("overlappingGenes", annotatedVariant.ToString());
            Assert.DoesNotContain("ENST00000427857", annotatedVariant.ToString());
        }

        [Fact]
        public void SvDelPartiallyOverlappingGene()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19443800	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=19452000");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("\"overlappingGenes\":[\"RP11-536C10.15\"]", annotatedVariant);
            AssertUtilities.CheckJsonContains("ENST00000546909", annotatedVariant);
        }

        [Fact]
        public void InsertionReciprocalOverlap()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_756265_756269.nsa"),
                "1	756267	.	T	<INS>	.	PASS	SVTYPE=INS;END=756267");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonDoesNotContain("\"reciprocalOverlap\":NaN", annotatedVariant);
            AssertUtilities.CheckJsonContains("esv1032937", annotatedVariant);
            AssertUtilities.CheckJsonDoesNotContain("\"reciprocalOverlap\"", annotatedVariant);
        }

        [Fact]
        public void RefNameMismatch()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_672000_672092.nsa"),
                "chr1	672000	.	C	<DEL>	0.1	LowQ	SVTYPE=DEL;END=672092;ALTDEDUP=1;ALTDUP=0;REFDEDUP=0;REFDUP=0;INTERGENIC=False	.	.");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("dgv1n111", annotatedVariant);
        }

        [Fact]
        public void LossOfHeterozygosityTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, null as List<string>,
                "1	11131485	Canvas:REF:1:11131486:16833263	N	<CNV>	61	PASS	SVTYPE=LOH;END=16833263	RC:BC:CN:MCC	.	84:9227:2:2");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonDoesNotContain("\"variantType\":\"loss_of_heterozygosity\"", annotatedVariant);
            AssertUtilities.CheckJsonContains("\"variantType\":\"copy_number_variation\"", annotatedVariant);
            AssertUtilities.CheckJsonDoesNotContain("\"altAllele\":\"LOH\"", annotatedVariant);

            AssertUtilities.CheckSampleCount(2, annotatedVariant);
            var sample = JsonUtilities.GetSampleJson(annotatedVariant, 1);
            Assert.Contains("lossOfHeterozygosity", sample);
        }

        [Fact]
        public void ClinGenTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_10330000_10377970.nsa"),
                "chr1	10330000	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=10377969");
            Assert.NotNull(annotatedVariant);

            Assert.Contains(
                "{\"chromosome\":\"1\",\"begin\":10324455,\"end\":16107335,\"variantType\":\"copy_number_loss\",\"id\":\"nsv993553\",\"clinicalInterpretation\":\"pathogenic\",\"observedLosses\":1,\"phenotypes\":[\"Developmental delay AND/OR other significant developmental or morphological phenotypes\"],\"reciprocalOverlap\":0.0083}",
                annotatedVariant.ToString());
            Assert.DoesNotContain("nsv995427", annotatedVariant.ToString());
        }

        [Fact]
        public void MergedClinGenItemTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_145361282_146000000.nsa"),
                "chr1	145361282	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=146000000");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("{\"chromosome\":\"1\",\"begin\":145415190,\"end\":148809863,\"variantType\":\"copy_number_variation\",\"id\":\"nsv931930\",\"clinicalInterpretation\":\"pathogenic\",\"observedGains\":2,\"observedLosses\":2,\"validated\":true,\"phenotypes\":[\"Developmental delay AND/OR other significant developmental or morphological phenotypes\",\"Global developmental delay\",\"Microcephaly\"],\"phenotypeIds\":[\"HP:0001263\",\"MedGen:CN001157\",\"HP:0000252\",\"MedGen:C1845868\"],\"reciprocalOverlap\":0.17227}", annotatedVariant.ToString());
            Assert.DoesNotContain("nsv497249", annotatedVariant.ToString());
        }

        [Fact]
        public void DgvTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_12000_12300.nsa"),
                "chr1	12000	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=12300");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"chromosome\":\"1\",\"begin\":11189,\"end\":36787,\"variantType\":\"copy_number_gain\",\"variantFreqAll\":0.01081,\"id\":\"dgv1e59\",\"sampleSize\":185,\"observedGains\":2,\"reciprocalOverlap\":0.01172", annotatedVariant.ToString());
        }

        [Fact]
        public void FirstSuppIntervalInContigTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr14_19000001_19000101.nsa"),
                "chr14	19000001	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=19000101");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("{\"chromosome\":\"14\",\"begin\":19000001,\"end\":19057950,\"variantType\":\"copy_number_gain\",\"variantFreqAll\":0.02703,\"id\":\"dgv1139e59\",\"sampleSize\":185,\"observedGains\":5,\"reciprocalOverlap\":0.00173}", annotatedVariant.ToString());
        }

        //[Fact]
        //public void TranscriptNextToVariant()
        //{
        //    // ENST00000472353: chr1: 29486894-29495151
        //    // gene: SRSF4: chr1: 29474255-29508499
        //    var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000472353_chr1_Ensembl84"), null,
        //        "1	29495151	.	C	<DEL>	.	PASS	SVTYPE=DEL;END=29508157");
        //    Assert.NotNull(annotatedVariant);
        //    Assert.Contains("\"overlappingGenes\":[\"SRSF4\"]", annotatedVariant.ToString());
        //    Assert.DoesNotContain("ENST00000472353", annotatedVariant.ToString());
        //}

        [Fact]
        public void ReciprocalOverlapPrecsion()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr14_19000001_19000101.nsa"),
                "chr14	19000001	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=19000801");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("{\"chromosome\":\"14\",\"begin\":19000001,\"end\":19057950,\"variantType\":\"copy_number_gain\",\"variantFreqAll\":0.02703,\"id\":\"dgv1139e59\",\"sampleSize\":185,\"observedGains\":5,\"reciprocalOverlap\":0.01381}", annotatedVariant.ToString());
        }

        [Fact]
        public void ReciprocalOverlapPrecsion2()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr9_133748425_133753801.nsa"),
                "chr9	133748425	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=133753801");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("nsv615533", annotatedVariant.ToString());
            Assert.Contains("\"reciprocalOverlap\":0.07813", annotatedVariant.ToString());
        }

        [Fact]
        public void InsertionReciprocalOverlap2()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr11_15261851_15261853.nsa"),
                "11	15261851	.	T	<INS>	.	PASS	SVTYPE=INS;END=15261851");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("nsv477521", annotatedVariant.ToString());
            Assert.DoesNotContain("\"reciprocalOverlap\":NaN", annotatedVariant.ToString());
        }

        [Fact(Skip = "no overlapping transcripts are being found. May be an issue with generating mini cache")]
        public void OverlappingTranscripts()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("chr1_89500_911300_Ensembl84_pos"), null) as NirvanaAnnotationSource;
            annotationSource?.EnableReportAllSvOverlappingTranscripts();

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                VcfUtilities.GetVcfVariant("chr1\t900000\t.\tG\t<DEL>\t.\tPASS\tEND=911300;SVTYPE=DEL"));
            Assert.NotNull(annotatedVariant);

            var overlappingTranscriptIds =
                JsonUtilities.GetOverlappingTranscriptIds(annotatedVariant.AnnotatedAlternateAlleles.First());

            var expectedTranscripts = new List<string>
            {
                "ENST00000338591",
                "ENST00000379410",
                "ENST00000379409",
                "ENST00000379407",
                "ENST00000480267",
                "ENST00000491024",
                "ENST00000433179",
                "ENST00000341290",
                "ENST00000479361"
            };

            Assert.True(expectedTranscripts.OrderBy(t => t).SequenceEqual(overlappingTranscriptIds.OrderBy(t => t)));
            Assert.Contains("{\"transcript\":\"ENST00000338591\",\"hgnc\":\"KLHL17\",\"isCanonical\":true,\"partialOverlap\":true}", annotatedVariant.ToString());
            Assert.Contains("{\"transcript\":\"ENST00000379409\",\"hgnc\":\"PLEKHN1\"}", annotatedVariant.ToString());
        }
    }
}