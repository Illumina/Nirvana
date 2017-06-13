using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.FileHandling.JSON
{
    [Collection("ChromosomeRenamer")]
    public sealed class JsonOutputTests : RandomFileBase
    {
        private readonly IChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public JsonOutputTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void UnifiedJsonWriterTest()
        {
            var randomPath = GetRandomPath();
            using (var streamWriter = new StreamWriter(FileUtilities.GetCreateStream(randomPath)))
            using (var writer       = new UnifiedJsonWriter(streamWriter, DateTime.Now.ToString(CultureInfo.InvariantCulture), "testing", null, null, null))
            {
                writer.Write("test json string");
            }

            File.Delete(randomPath);
        }

        [Fact]
        public void AlleleSpecificCosmic()
        {
            JsonUtilities.AlleleContains(
               "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
               Resources.MiniSuppAnnot("chr1_111783982_111783983.nsa"), "{\"id\":\"COSM3996742\",\"refAllele\":\"C\",\"altAllele\":\"A\",\"gene\":\"CHI3L2\",\"sampleCount\":1,\"studies\":[{\"id\":544,\"histology\":\"haematopoietic neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"}],\"isAlleleSpecific\":true}");

            JsonUtilities.AlleleContains(
               "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
               Resources.MiniSuppAnnot("chr1_111783982_111783983.nsa"), "{\"id\":544,\"histology\":\"haematopoietic neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"}");

            JsonUtilities.AlleleContains(
               "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
               Resources.MiniSuppAnnot("chr1_111783982_111783983.nsa"), "\"id\":\"COSM4591038\",\"refAllele\":\"C\",\"altAllele\":\"T\",\"gene\":\"CHI3L2\",\"sampleCount\":4");

            JsonUtilities.AlleleContains(
               "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
               Resources.MiniSuppAnnot("chr1_111783982_111783983.nsa"), "{\"histology\":\"lymphoid neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"}");

            JsonUtilities.AlleleContains(
               "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
               Resources.MiniSuppAnnot("chr1_111783982_111783983.nsa"), "{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"}");

            JsonUtilities.AlleleContains(
               "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
               Resources.MiniSuppAnnot("chr1_111783982_111783983.nsa"), "{\"histology\":\"osteosarcoma\",\"primarySite\":\"bone\"}");
        }

        [Fact]
        public void FirstExacTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_13528_13529.nsa"),
                "1	13528	.	C	G,T	1771.54	VQSRTrancheSNP99.60to99.80	AC=21,11;AC_AFR=12,0");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            var altAllele2 = JsonUtilities.GetAllele(annotatedVariant, 1);
            Assert.NotNull(altAllele2);

            Assert.Contains("\"coverage\":28,\"allAf\":0.001247,\"afrAf\":0.030769,\"amrAf\":0.008621,\"easAf\":0,\"finAf\":0,\"nfeAf\":0,\"sasAf\":0,\"othAf\":0", altAllele);
            Assert.Contains("\"coverage\":28,\"allAf\":0.000863,\"afrAf\":0,\"amrAf\":0,\"easAf\":0,\"finAf\":0,\"nfeAf\":0.000765,\"sasAf\":0.000995,\"othAf\":0", altAllele2);
        }

        [Fact]
        public void CarryOverDbsnp()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr2_118565152_118565155.nsa"),
                "2	118565152	.	AGC	AGT,CGC	50	PASS	SNVSB=-8.7;SNVHPOL=5;CSQ=AGT|upstream_gene_variant|MODIFIER|AC009312.1|ENSG00000238207|Transcript|ENST00000457110|antisense|||||||||||3402|-1|Clone_based_vega_gene||YES|||||||||,CGC|upstream_gene_variant|MODIFIER|AC009312.1|ENSG00000238207|Transcript|ENST00000457110|antisense|||||||||||3402|-1|Clone_based_vega_gene||YES|||||||||	 GT:GQ:GQX:DP:DPF:AD	1/2:83:21:12:2:0,8,4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleContains(annotatedVariant,
				"{\"altAllele\":\"T\",\"refAllele\":\"C\",\"begin\":118565154,\"chromosome\":\"2\",\"end\":118565154,\"variantType\":\"SNV\",\"vid\":\"2:118565154:T\",\"dbsnp\":{\"ids\":[\"rs62192625\"]},\"globalAllele\":{\"globalMinorAllele\":\"T\",\"globalMinorAlleleFrequency\":0.3464}");

            JsonUtilities.AlleleEquals(annotatedVariant,
				"{\"altAllele\":\"C\",\"refAllele\":\"A\",\"begin\":118565152,\"chromosome\":\"2\",\"end\":118565152,\"variantType\":\"SNV\",\"vid\":\"2:118565152:C\",\"dbsnp\":{\"ids\":[\"rs754609911\"]}}",
                1);
        }

        [Fact]
        public void ClinVarNonEnglishChars()
        {
            JsonUtilities.AlleleContains("1	225592187	.	CTAGAAGA	CCTTCTAG	362	PASS	CIGAR=1M18D",
                Resources.MiniSuppAnnot("chr1_225592187_225592188.nsa"),
                "Pelger-Huët anomaly");
        }

        [Fact]
        public void MissingClinvarIdRefAllele()
        {
            JsonUtilities.AlleleEquals("1	8021910	.	GGTGCTGGACGGTGTCCCT	T	362	PASS	CIGAR=1M18D",
                Resources.MiniSuppAnnot("chr1_8021910_8021911.nsa"),
				"{\"altAllele\":\"-\",\"refAllele\":\"GGTGCTGGACGGTGTCCC\",\"begin\":8021910,\"chromosome\":\"1\",\"end\":8021927,\"variantType\":\"deletion\",\"vid\":\"1:8021910:8021927\",\"dbsnp\":{\"ids\":[\"rs767770365\"]}}");
        }



        [Fact]
        public void Uncleared1000GenomeValues()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_8383550_8383560.nsa"),
                "1	8383550	.	CAAAAAAAAA	C,CAAAAAAAAAAAAA	100	PASS	.");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"-\",\"refAllele\":\"AAAAAAAAA\",\"begin\":8383551,\"chromosome\":\"1\",\"end\":8383559,\"variantType\":\"deletion\",\"vid\":\"1:8383551:8383559\",\"dbsnp\":{\"ids\":[\"rs34956825\",\"rs774024202\",\"rs796237312\"]},\"oneKg\":{\"allAf\":0.921526,\"afrAf\":0.847958,\"amrAf\":0.903458,\"easAf\":0.985119,\"eurAf\":0.914513,\"sasAf\":0.97546,\"allAn\":5008,\"afrAn\":1322,\"amrAn\":694,\"easAn\":1008,\"eurAn\":1006,\"sasAn\":978,\"allAc\":4615,\"afrAc\":1121,\"amrAc\":627,\"easAc\":993,\"eurAc\":920,\"sasAc\":954}}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"AAAA\",\"refAllele\":\"-\",\"begin\":8383560,\"chromosome\":\"1\",\"end\":8383559,\"variantType\":\"insertion\",\"vid\":\"1:8383560:8383559:AAAA\"}",
                1);
        }

        [Fact]
        public void Extra1000GenomeValues()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr17_7432913_7432914.nsa"),
                "17	7432913	rs34130898	A	AATT,ATTATT	100	PASS	.");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"ATT\",\"refAllele\":\"-\",\"begin\":7432914,\"chromosome\":\"17\",\"end\":7432913,\"variantType\":\"insertion\",\"vid\":\"17:7432914:7432913:ATT\",\"dbsnp\":{\"ids\":[\"rs34130898\",\"rs397969947\"]},\"oneKg\":{\"allAf\":0.357827,\"afrAf\":0.303328,\"amrAf\":0.396254,\"easAf\":0.392857,\"eurAf\":0.422465,\"sasAf\":0.301636,\"allAn\":5008,\"afrAn\":1322,\"amrAn\":694,\"easAn\":1008,\"eurAn\":1006,\"sasAn\":978,\"allAc\":1792,\"afrAc\":401,\"amrAc\":275,\"easAc\":396,\"eurAc\":425,\"sasAc\":295}}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"TTATT\",\"refAllele\":\"-\",\"begin\":7432914,\"chromosome\":\"17\",\"end\":7432913,\"variantType\":\"insertion\",\"vid\":\"17:7432914:7432913:TTATT\"}",
                1);
        }

        [Fact]
        public void BreakEndBeginOutput()
        {
            JsonUtilities.AlleleEquals(
                "1	28722335	MantaBND:4051:0:2:0:0:0:0	T	[3:115024109[T	.	PASS	SVTYPE=BND;MATEID=MantaBND:4051:0:2:0:0:0:1;IMPRECISE;CIPOS=-209,210;SOMATIC;SOMATICSCORE=42;BND_DEPTH=23;MATE_BND_DEPTH=24     PR      25,0    71,10",
                null,
                "{\"altAllele\":\"[3:115024109[T\",\"refAllele\":\"T\",\"begin\":28722335,\"chromosome\":\"1\",\"end\":28722335,\"variantType\":\"translocation_breakend\",\"vid\":\"1:28722335:-:3:115024109:+\"}");
        }


        [Fact]
        public void CosmicAlleleContains()
        {
            JsonUtilities.AlleleContains(
                "1	898602	COSM2151955	GCG	G	.	.	GENE=KLHL17;STRAND=+;CDS=c.1157_1158delCG;AA=p.A386fs*12;CNT=1",
                Resources.MiniSuppAnnot("chr1_898602_898603.nsa"),
				"\"cosmic\":[{\"id\":\"COSM2151955\",\"refAllele\":\"CG\",\"altAllele\":\"-\",\"gene\":\"KLHL17\",\"sampleCount\":1,\"studies\":[{\"histology\":\"glioma\",\"primarySite\":\"central nervous system\"}],\"isAlleleSpecific\":true},{\"id\":\"COSM2150687\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"gene\":\"KLHL17\",\"sampleCount\":1,\"studies\":[{\"histology\":\"glioma\",\"primarySite\":\"central nervous system\"}]}]");
        }

        /// <summary>
        /// checking if various deletions in the same locations work correctly
        /// </summary>
        [Fact]
        public void CosmicMultiDelete()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr17_21319650_21319651.nsa"),
                "17	21319650	.	CGAG	C	101	PASS	CIGAR=1M3D;RU=GAG;REFREP=2;IDREP=1	GT:GQ:GQX:DPI:AD	0/1:141:101:29:22,4");

            AssertUtilities.CheckJsonContains(
                "{\"id\":\"COSM3735158\",\"refAllele\":\"G\",\"altAllele\":\"-\"",annotatedVariant);

            AssertUtilities.CheckJsonContains(
                "{\"id\":\"COSM278475\",\"refAllele\":\"GAG\"", annotatedVariant);
        }

        [Fact]
        public void CnvIgnoreFlankingGenes()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000427857_chr1_Ensembl84"), null as List<string>,
                "1	816800	Canvas:GAIN:1:816801:821943	N	<CNV>	2	q10;CLT10kb	SVTYPE=CNV;END=821943	RC:BC:CN	174:2:4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":816801,\"chromosome\":\"1\",\"end\":821943,\"variantType\":\"copy_number_variation\",\"vid\":\"1:816801:821943:4\"}");
        }

        [Fact]
        public void AnnotationCarryover()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr2_90472571_90472592.nsa"),
                "2	90472571	.	AAAAAAAAAAAAAAAAAAGTCC	AGTCT	177	PASS	CIGAR=1M21D4I;RU=.;REFREP=.;IDREP=.	GT:GQ:GQX:DPI:AD	0/1:220:177:46:40,7");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetFirstAlleleJson(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal("{\"altAllele\":\"GTCT\",\"refAllele\":\"AAAAAAAAAAAAAAAAAGTCC\",\"begin\":90472572,\"chromosome\":\"2\",\"end\":90472592,\"variantType\":\"indel\",\"vid\":\"2:90472572:90472592:GTCT\"}", altAllele);

            var annotatedVariant2 = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr2_90472571_90472592.nsa"),
                "2	90472592	.	C	.	.	PASS	RefMinor	GT:GQX:DP:DPF:AD	0:96:33:15:33");
            Assert.NotNull(annotatedVariant2);

            var altAllele2 = JsonUtilities.GetFirstAlleleJson(annotatedVariant2);
            Assert.NotNull(altAllele2);

            Assert.Equal("{\"refAllele\":\"C\",\"begin\":90472592,\"chromosome\":\"2\",\"end\":90472592,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"2:90472592:C\",\"globalAllele\":{\"globalMinorAllele\":\"C\",\"globalMinorAlleleFrequency\":0.006989}}", altAllele2);
        }

        [Fact]
        public void SvDelMissingTranscript()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000594233_chr1_Ensembl84"), null as List<string>,
                "1	823854	MantaDEL:60:0:0:0:2:4	AGGGAGAGAAAGGGAAAGAAGGAAAGAAACAAGGAAGCAGGGAGGGAGAGAAAGAGGAAGGGAG	A	49	MaxDepth;MaxMQ0Frac	END=823917;SVTYPE=DEL;SVLEN=-63;CIGAR=1M63D;CSQT=1|AL645608.2|ENST00000594233|  GT:GQ:PR:SR 0/1:49:21,0:35,8");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"-\",\"refAllele\":\"GGGAGAGAAAGGGAAAGAAGGAAAGAAACAAGGAAGCAGGGAGGGAGAGAAAGAGGAAGGGAG\",\"begin\":823855,\"chromosome\":\"1\",\"end\":823917,\"variantType\":\"deletion\",\"vid\":\"1:823855:823917\",\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000594233.1\",\"bioType\":\"protein_coding\",\"geneId\":\"ENSG00000269308\",\"hgnc\":\"AL645608.2\",\"consequence\":[\"downstream_gene_variant\"],\"isCanonical\":true,\"proteinId\":\"ENSP00000470877.1\"}]}}");
        }

        [Fact]
        public void TrimClinvarAllele()
        {
            // 13      40298638        rs66629036      TA      T       .       .       RS=66629036;RSPOS=40298641;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000080005000002000200;GENEINFO=COG6:57511;WGT=1;VC=DIV;INT;ASP;CLNALLE=1;CLNHGVS=NC_000013.10:g.40298641delA;CLNSRC=.;CLNORIGIN=1;CLNSRCID=.;CLNSIG=2;CLNDSDB=MedGen;CLNDSDBID=CN169374;CLNDBN=not_specified;CLNREVSTAT=single;CLNACC=RCV000082045.4
            JsonUtilities.AlleleContains(
                "13	40298637	rs66629036	TTA	TT	.	.	PASS	RefMinor;GMAF=T|0.01877 GT:GQX:DP:DPF:AD        0/0:69:24:3:24",
                Resources.MiniSuppAnnot("chr13_40298638_40298639.nsa"),
				"{\"id\":\"RCV000082045.4\"");
        }

        [Fact]
        public void CosmicMissingStudy()
        {
            JsonUtilities.AlleleContains(
                "chr2	203066048	.	G	T	39.00	PASS	SNVSB=-4.1;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:72:39:37:1:30,7",
                Resources.MiniSuppAnnot("chr2_203066046_203066050.nsa"),
				"\"cosmic\":[{\"id\":\"COSN166383\",\"refAllele\":\"G\",\"altAllele\":\"T\",\"isAlleleSpecific\":true}]");
        }

        [Fact]
        public void CosmicLongDelInsAlleleError()
        {
            JsonUtilities.AlleleContains(
                "chr18	58484430	.	AAT	A	39.00	PASS	SNVSB=-4.1;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:72:39:37:1:30,7",
                Resources.MiniSuppAnnot("chr18_58484430_58484431.nsa"),
                "\"cosmic\":[{\"id\":\"COSN197004\",\"refAllele\":\"ATGTGAAAAATATATTTTATATAATTTCAATATTTTTAACA\",\"altAllele\":\"TTGAAAAATATATTTTATATAATTTCAATATTTTTAACAT\"}]");
        }

        [Fact]
        public void BadClivarRef()
        {
            JsonUtilities.AlleleEquals(
                "11	109157259	.	T	.	.	PASS	RefMinor;GMAF=T|0.01877 GT:GQX:DP:DPF:AD        0/0:69:24:3:24",
                Resources.MiniSuppAnnot("chr11_109157259_109157260.nsa"),
				"{\"refAllele\":\"T\",\"begin\":109157259,\"chromosome\":\"11\",\"end\":109157259,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"11:109157259:T\",\"globalAllele\":{\"globalMinorAllele\":\"T\",\"globalMinorAlleleFrequency\":0.01877}}");
        }

        [Fact]
        public void CosmicAlleleSpecificFlag()
        {
            JsonUtilities.AlleleContains(
                "chr1	565591	rs7416152	C	T	39.00	PASS	SNVSB=-4.1;SNVHPOL=4;cosmic=COSN210317	GT:GQ:GQX:DP:DPF:AD	0/1:72:39:37:1:30,7",
                Resources.MiniSuppAnnot("chr1_565591_565592.nsa"),
				"\"cosmic\":[{\"id\":\"COSN210317\",\"refAllele\":\"C\",\"altAllele\":\"T\",\"isAlleleSpecific\":true}]");
        }

        [Fact]
        public void RefAlleleDash()
        {
            JsonUtilities.AlleleEquals(
                "17	46107	.	A	G	153	LowGQX SNVSB=-20.1;SNVHPOL=4;CSQ=G|ENSG00000262836|ENST00000576171|Transcript|upstream_gene_variant||||||||4729|||||||YES|||||||| GT:GQ:GQX:DP:DPF:AD	1/1:18:18:7:0:0,7",
                Resources.MiniSuppAnnot("chr17_77263_77265.nsa"),
                "{\"altAllele\":\"G\",\"refAllele\":\"A\",\"begin\":46107,\"chromosome\":\"17\",\"end\":46107,\"variantType\":\"SNV\",\"vid\":\"17:46107:G\"}");
        }

        [Fact]
        public void CosmicAltAlleleN()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_17086085_17086086.nsa"),
                "1	17086085	.	G	GC	209	LowGQX	.	GT:GQ:GQX:DPI:AD	1/1:12:9:5:0,4");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains(
                "{\"id\":\"COSM424569\",\"refAllele\":\"-\",\"altAllele\":\"N\",\"gene\":\"Q13209_HUMAN\",\"sampleCount\":8",
                annotatedVariant);

            AssertUtilities.CheckJsonContains(
                "{\"id\":\"COSM424570\",\"refAllele\":\"-\",\"altAllele\":\"N\",\"gene\":\"MST1P9\",\"sampleCount\":8",
                annotatedVariant);
        }

		[Fact(Skip = "unknown reason")]
        public void IsAlleleSpecificNotSetJson()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_24122986_24122987.nsa"),
                "1	24122986	rs760941	C	G,T	.	.	.");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            var altAllele2 = JsonUtilities.GetAllele(annotatedVariant, 1);
            Assert.NotNull(altAllele2);

            const string asClinVar = "\"clinvar\":[{\"id\":\"RCV000078694.4\",\"reviewStatus\":\"criteria provided, single submitter\",\"alleleOrigins\":[\"germline\"],\"refAllele\":\"C\",\"altAllele\":\"T\",\"phenotypes\":[\"not specified\"],\"medGenIDs\":[\"CN169374\"],\"significance\":\"benign\",\"lastUpdatedDate\":\"2016-08-26\",\"pubMedIds\":[\"23757202\",\"25741868\"],\"isAlleleSpecific\":true";
            const string asCosmic = "\"cosmic\":[{\"id\":\"COSN1100872\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"gene\":\"GALE\",\"isAlleleSpecific\":true}";

            Assert.DoesNotContain(asClinVar, altAllele);
            Assert.Contains(asCosmic, altAllele);

            Assert.Contains(asClinVar, altAllele2);
            Assert.DoesNotContain(asCosmic, altAllele2);
           
        }

        [Fact]
        public void DuplicateDbSnpJson()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_8121167_8121168.nsa"),
                "1	8121167	.	C	CAAT,CAATAAT	.	.	.");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
				"{\"altAllele\":\"AAT\",\"refAllele\":\"-\",\"begin\":8121168,\"chromosome\":\"1\",\"end\":8121167,\"variantType\":\"insertion\",\"vid\":\"1:8121168:8121167:AAT\",\"dbsnp\":{\"ids\":[\"rs34500567\",\"rs59792241\"]}}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"AATAAT\",\"refAllele\":\"-\",\"begin\":8121168,\"chromosome\":\"1\",\"end\":8121167,\"variantType\":\"insertion\",\"vid\":\"1:8121168:8121167:AATAAT\"}",
                1);
        }

        [Fact]
        public void MissingRsid()
        {
            JsonUtilities.AlleleContains(
                "chr1	129010	rs377161483	AATG	A	32	LowGQXHetAltDel	CIGAR=1M1D1M,2M1D;RU=C,A;REFREP=1,17;IDREP=0,16	GT:GQ:GQX:DPI:AD	1/2:162:2:22:4,8,1",
                Resources.MiniSuppAnnot("chr1_129010_129012.nsa"),
				"{\"altAllele\":\"-\",\"refAllele\":\"ATG\",\"begin\":129011,\"chromosome\":\"chr1\",\"end\":129013,\"variantType\":\"deletion\",\"vid\":\"1:129011:129013\",\"dbsnp\":{\"ids\":[\"rs377161483\"]}");
        }

		[Fact(Skip = "class change")]
        public void MultipleDbSnpIds()
        {
            //var sa = new SupplementaryAnnotationPosition(115256529);

            //var dbSnp = new DbSnpAnnotation
            //{
            //    DbSnp = new List<long> { 111, 222, 333 }
            //};

            //var saCreator = new SupplementaryPositionCreator(sa);
            //saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp, "C", dbSnp);

            //var saReader = new MockSupplementaryAnnotationReader(sa);

            //var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, saReader, "chr1	115256529	.	T	C	1000	PASS	.	GT	0/1");
            //Assert.NotNull(annotatedVariant);

            //Assert.Contains("\"dbsnp\":[\"rs111\",\"rs222\",\"rs333\"]", annotatedVariant.ToString());
        }



        [Fact]
        public void MissingCosmicId()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_26608814_26608815.nsa"),
                "1	26608811	.	TCCAGGACAGGGACTGGGGCCGGGACCGGGACC	TCCGGGACC,TCCAGGACA	139	LowGQXHetAltDel	CIGAR=1M24D8M,9M24");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            var altAllele2 = JsonUtilities.GetAllele(annotatedVariant, 1);

            Assert.Contains(
                "\"cosmic\":[{\"id\":\"COSM4143711\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"gene\":\"UBXN11\",\"sampleCount\":22,\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"},{\"id\":589,\"histology\":\"other\",\"primarySite\":\"thyroid\"}]}]",
                altAllele);

            Assert.DoesNotContain("\"cosmic\":", altAllele2);
        }

        [Fact]
        public void MissingClinvarId()
        {
            JsonUtilities.AlleleContains("1	55518316	rs2483205	C	T	.	.	RS=2483205;RSPOS=55518316",
                Resources.MiniSuppAnnot("chr1_55518316_55518317.nsa"),
				"{\"id\":\"RCV000030351");

        }

        [Fact]
        public void MissingRegulatoryFeatureJson()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENSR00000669067_chr1_Ensembl84_reg"), null as List<string>,
                "1	16833395	.	G	A,T	.	LowQscore	SOMATIC;QSS=268;TQSS=2;NT=conflict;QSS_NT=0;TQSS_NT=2;SGT=GT->AT;DP=431;MQ=53.95;MQ0=60;ALTPOS=41;ALTMAP=25;ReadPosRankSum=0.74;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=0.00;CSQ=A|regulatory_region_variant|MODIFIER|||RegulatoryFeature|ENSR00000669067|,T|regulatory_region_variant|MODIFIER|||RegulatoryFeature|ENSR00000669067| DP:FDP:SDP:SUBDP:AU:CU:GU:TU	130:1:0:0:17,18:0,0:27,27:85,111	236:3:0:0:52,54:1,1:27,27:153,191");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"A\",\"refAllele\":\"G\",\"begin\":16833395,\"chromosome\":\"1\",\"end\":16833395,\"variantType\":\"SNV\",\"vid\":\"1:16833395:A\",\"regulatoryRegions\":[{\"id\":\"ENSR00000669067\",\"type\":\"promoter_flanking_region\",\"consequence\":[\"regulatory_region_variant\"]}]}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"T\",\"refAllele\":\"G\",\"begin\":16833395,\"chromosome\":\"1\",\"end\":16833395,\"variantType\":\"SNV\",\"vid\":\"1:16833395:T\",\"regulatoryRegions\":[{\"id\":\"ENSR00000669067\",\"type\":\"promoter_flanking_region\",\"consequence\":[\"regulatory_region_variant\"]}]}",
                1);
        }

        [Fact]
        public void MissingRefAllele()
        {
            JsonUtilities.AlleleEquals(
                "chr1	15274	.	A	.	279.00	PASS	SNVSB=-39.1;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	1/2:58:55:20:1:0,5,15",
                Resources.MiniSuppAnnot("chr1_15274_15275.nsa"),
				"{\"refAllele\":\"A\",\"begin\":15274,\"chromosome\":\"chr1\",\"end\":15274,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"1:15274:A\",\"globalAllele\":{\"globalMinorAllele\":\"G\",\"globalMinorAlleleFrequency\":0.3472}}");
        }

        [Fact]
        public void OutputStrandBias()
        {
            var unifiedJson =
                JsonUtilities.GetJson(
                    "chrX	2699246	rs148553620	C	A	250.95	VQSRTrancheSNP99.00to99.90	AC=2;AF=0.250;AN=8;BaseQRankSum=1.719;DB;DP=106;Dels=0.00;FS=20.202;HaplotypeScore=0.0000;MLEAC=2;MLEAF=0.250;MQ=43.50;MQ0=52;MQRankSum=2.955;QD=4.73;ReadPosRankSum=1.024;SB=-1.368e+02;VQSLOD=-0.3503;culprit=MQ;PLF  GT:AD:DP:GQ:PL:AA   0:10,6:16:9:0,9,118:P1,.	   0|1:12,11:23:27:115,0,27:M1,M2   0|0:37,0:37:18:0,18,236:M1,P1   1|0:13,17:30:59:177,0,59:M2,P1", _renamer);

            const string expectedStrandBias = "-136.8";
            Assert.Equal(expectedStrandBias, unifiedJson.StrandBias);

            var observedEntry = unifiedJson.ToString();
            var expectedEntry = $"\"strandBias\":{expectedStrandBias}";
            Assert.Contains(expectedEntry, observedEntry);
        }

        [Fact]
        public void MantaDelWithoutSymbolicAllele()
        {
            JsonUtilities.AlleleEquals(
                "1	1530648	MantaDEL:116:0:0:0:3:1	GAGACAGAGAGAAACAGAGACAGAGACAGAGAGGCAGACAGAGAGAGAGACAGACAGAGAGCAGAACAGGGAGAGACAAAGAGACAGAGAGAGAGAGAGACACAGAGAGAGAGAGATAGAGAGAGGCAGACAGAGACAGAGAGACAGACAGACACAGAGCAGAACAGGGAGAGACAGAGAGAGAGAGACAGAGAGAGGCAGAC	GA	.	MinSomaticScore	END=1530850;SVTYPE=DEL;SVLEN=-202;CIGAR=1M202D;CIPOS=0,3;HOMLEN=3;HOMSEQ=AGA;SOMATIC;SOMATICSCORE=17	PR:SR	13,0:25,3	24,0:33,10",
                null,
                "{\"altAllele\":\"-\",\"refAllele\":\"GACAGAGAGAAACAGAGACAGAGACAGAGAGGCAGACAGAGAGAGAGACAGACAGAGAGCAGAACAGGGAGAGACAAAGAGACAGAGAGAGAGAGAGACACAGAGAGAGAGAGATAGAGAGAGGCAGACAGAGACAGAGAGACAGACAGACACAGAGCAGAACAGGGAGAGACAGAGAGAGAGAGACAGAGAGAGGCAGAC\",\"begin\":1530650,\"chromosome\":\"1\",\"end\":1530850,\"variantType\":\"deletion\",\"vid\":\"1:1530650:1530850\"}");
        }

        [Fact]
        public void AlleleSpecificDbsnpId()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr17_3124877_3124878.nsa"),
                "17	3124877	rs182093170	T	A,C	87	LowGQX	SNVSB=-11.7;SNVHPOL=2;phyloP=0.058	GT:GQ:GQX:DP:DPF:AD	1/2:7:5:5:1:0,1,4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
				"{\"altAllele\":\"A\",\"refAllele\":\"T\",\"begin\":3124877,\"chromosome\":\"17\",\"end\":3124877,\"variantType\":\"SNV\",\"vid\":\"17:3124877:A\",\"dbsnp\":{\"ids\":[\"rs182093170\"]}}");

            JsonUtilities.AlleleEquals(annotatedVariant,
				"{\"altAllele\":\"C\",\"refAllele\":\"T\",\"begin\":3124877,\"chromosome\":\"17\",\"end\":3124877,\"variantType\":\"SNV\",\"vid\":\"17:3124877:C\"}",
                1);
        }

        [Fact]
        public void MultipleEntryForGenomicVariant()
        {
            // reference no-call
            UnifiedJson.NeedsVariantComma = false;
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_22426387_22426388.nsa"),
                "1	22426387	.	A	.	.	LowGQX	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", true);

            Assert.Contains(
				"{\"chromosome\":\"1\",\"refAllele\":\"A\",\"position\":22426387,\"svEnd\":22426406,\"filters\":[\"LowGQX\"],\"samples\":[{\"totalDepth\":0}],\"variants\":[{\"refAllele\":\"A\",\"begin\":22426387,\"chromosome\":\"1\",\"end\":22426406,\"variantType\":\"reference_no_call\",\"vid\":\"1:22426387:22426406:NC\"}]}",
                annotatedVariant.ToString());

            // reference
            var annotatedVariant2 = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_22426387_22426388.nsa"),
                "1	22426387	.	A	.	.	PASS	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", true);

            Assert.Equal(null, annotatedVariant2.ToString());
        }

        [Fact]
        public void VariantTypeMissing()
        {
            JsonUtilities.AlleleEquals(
                "1	17224554	Canvas:GAIN:1:17224555:17275816	N	<CNV>	31	PASS	SVTYPE=CNV;END=17275816	RC:BC:CN:MCC	.	151:42:5:4",
                null,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":17224555,\"chromosome\":\"1\",\"end\":17275816,\"variantType\":\"copy_number_variation\",\"vid\":\"1:17224555:17275816:5\"}");
        }

        [Fact]
        public void OutputStrelkaSnvRelevantFields()
        {
            var json =
                JsonUtilities.GetJson(
                    "2	167760371	rs267598980	G	A	.	PASS	SOMATIC;QSS=34;TQSS=1;NT=ref;QSS_NT=34;TQSS_NT=1;SGT=GG->AG;DP=45;MQ=60.00;MQ0=0;ALTPOS=43;ALTMAP=19;ReadPosRankSum=-1.64;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=5.54;cosmic=COSM36673;clinvar=other;CSQT=A|XIRP2|ENST00000409195|missense_variant	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	19:0:0:0:0,0:0,0:19,19:0,0	26:0:0:0:6,6:0,0:20,20:0,0", _renamer);
            Assert.NotNull(json);

            const string expectedRecalibratedQuality = "5.54";
            Assert.Equal(expectedRecalibratedQuality, json.RecalibratedQuality);

            const string expectedJointSomaticNormalQuality = "34";
            Assert.Equal(expectedJointSomaticNormalQuality, json.JointSomaticNormalQuality);


            var expectedJointSomaticNormalQualityEntry = $"\"jointSomaticNormalQuality\":{expectedJointSomaticNormalQuality}";
            var expectedRecalibratedQualityEntry = $"\"recalibratedQuality\":{expectedRecalibratedQuality}";

            Assert.Contains(expectedRecalibratedQualityEntry, json.ToString());
            Assert.Contains(expectedJointSomaticNormalQualityEntry, json.ToString());
        }

        [Fact]
        public void SupportRefNoCallTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_22426387_22426388.nsa"),
                "1	22426387	.	A	.	.	LowGQX	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", true);
            Assert.NotNull(annotatedVariant);

            Assert.Contains("{\"chromosome\":\"1\",\"refAllele\":\"A\",\"position\":22426387,\"svEnd\":22426406,\"filters\":[\"LowGQX\"],\"samples\":[{\"totalDepth\":0}],\"variants\":[{\"refAllele\":\"A\",\"begin\":22426387,\"chromosome\":\"1\",\"end\":22426406,\"variantType\":\"reference_no_call\",\"vid\":\"1:22426387:22426406:NC\"}]}", annotatedVariant.ToString());
        }

        [Fact]
        public void RefNoCallStrelkaFields()
        {
            var json =
                JsonUtilities.GetJson(
                    "1	205664649	.	C	.	.	LowQscore	SOMATIC;QSS=3;TQSS=1;NT=ref;QSS_NT=3;TQSS_NT=1;SGT=CC->CC;DP=116;MQ=57.63;MQ0=9;ALTPOS=89;ALTMAP=55;ReadPosRankSum=-1.14;SNVSB=6.63;PNOISE=0.00;PNOISE2=0.00;VQSR=0.15	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	11:4:0:0:0,0:7,8:0,0:0,0	92:28:0:0:5,17:58,62:1,1:0,0", _renamer);
            Assert.NotNull(json);

            const string expectedRecalibratedQuality = "0.15";
            Assert.Equal(expectedRecalibratedQuality, json.RecalibratedQuality);

            const string expectedJointSomaticNormalQuality = "3";
            Assert.Equal(expectedJointSomaticNormalQuality, json.JointSomaticNormalQuality);


            var expectedJointSomaticNormalQualityEntry = $"\"jointSomaticNormalQuality\":{expectedJointSomaticNormalQuality}";
            var expectedRecalibratedQualityEntry = $"\"recalibratedQuality\":{expectedRecalibratedQuality}";

            Assert.Contains(expectedRecalibratedQualityEntry, json.ToString());
            Assert.Contains(expectedJointSomaticNormalQualityEntry, json.ToString());
        }

        [Fact]
        public void OutputCanvasCnvRelevantField()
        {
            var vcfLine = "1	9314201	Canvas:GAIN:1:9314202:9404148	N	<CNV>	36	PASS	SVTYPE=CNV;END=9404148;ensembl_gene_id=ENSG00000049239,ENSG00000252841,ENSG00000171621	RC:BC:CN:MCC	.	151:108:6:4";

            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000377403_chr1_Ensembl84"), null as List<string>, vcfLine);
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":9314202,\"chromosome\":\"1\",\"end\":9404148,\"variantType\":\"copy_number_variation\",\"vid\":\"1:9314202:9404148:6\",\"overlappingGenes\":[\"H6PD\"],\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000377403.2\",\"bioType\":\"protein_coding\",\"exons\":\"4-5/5\",\"introns\":\"3-4/4\",\"geneId\":\"ENSG00000049239\",\"hgnc\":\"H6PD\",\"consequence\":[\"copy_number_increase\"],\"isCanonical\":true,\"proteinId\":\"ENSP00000366620.1\"}]}}");

            var cols = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(cols);
            var samples = extractor.ExtractSamples();
            Assert.Equal(2, samples.Count);

            var sample = samples[1];

            var observedCn = sample?.CopyNumber;
            Assert.Equal("6", observedCn);
        }

        [Fact]
        public void OutputSenecaCnvRelevantField()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000546909_chr14_Ensembl84"), null as List<string>,
                "14	19431000	14_19462000	G	<CNV>	.	PASS	SVTYPE=CNV;END=19462000;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"G\",\"begin\":19431001,\"chromosome\":\"14\",\"end\":19462000,\"variantType\":\"copy_number_variation\",\"vid\":\"14:19431001:19462000:?\",\"overlappingGenes\":[\"RP11-536C10.15\"]}");

            var observedCopyNumber = Convert.ToInt32(annotatedVariant.CopyNumber);

            Assert.Equal(0, observedCopyNumber);
            Assert.Contains("\"copyNumber\":0", annotatedVariant.ToString());
        }

        [Fact]
        public void AddExonTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000427857_chr1_Ensembl84"), null as List<string>,
                "1	803780	Canvas:GAIN:1:803781:821943	N	<CNV>	2	q10;CLT10kb	SVTYPE=CNV;END=821943	RC:BC:CN	174:2:4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":803781,\"chromosome\":\"1\",\"end\":821943,\"variantType\":\"copy_number_variation\",\"vid\":\"1:803781:821943:4\",\"overlappingGenes\":[\"FAM41C\"],\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000427857.1\",\"bioType\":\"lincRNA\",\"exons\":\"1-3/3\",\"introns\":\"1-2/2\",\"geneId\":\"ENSG00000230368\",\"hgnc\":\"FAM41C\",\"consequence\":[\"transcript_amplification\",\"copy_number_increase\"]}]}}");
        }

		[Fact]
		public void RepeatExpansionAltAlleleAndVidOutput()
		{
			var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000370457_chrX_Ensembl84"), null as List<string>,
				"chrX\t147912050\tFMR1\tG\t<STR8>\t.\tPASS\tSVTYPE=STR;END=147912110;REF=20;RL=60;RU=CGG\tGT:SO:SP:CN:CI\t1:FLANKING:10:8:5-11");
			Assert.NotNull(annotatedVariant);

			JsonUtilities.AlleleContains(annotatedVariant,
				"\"altAllele\":\"STR8\",");
			JsonUtilities.AlleleContains(annotatedVariant, "\"vid\":\"X:147912051:147912110:CGG:8\"");
		}

	    [Fact]
	    public void Output_repeat_unit_and_ref_repeat_length()
	    {
		    var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000370457_chrX_Ensembl84"), null as List<string>,
			    "chrX\t147912050\tFMR1\tG\t<STR8>\t.\tPASS\tSVTYPE=STR;END=147912110;REF=20;RL=60;RU=CGG\tGT:SO:SP:CN:CI\t1:FLANKING:10:8:5-11");
		    Assert.NotNull(annotatedVariant);

			Assert.Contains("\"end\":147912110", annotatedVariant.ToString());
		    Assert.Contains("\"repeatUnit\":\"CGG\"", annotatedVariant.ToString());
		    Assert.Contains("\"refRepeatCount\":20", annotatedVariant.ToString());
	    }

	    // this is a made up example
	    [Fact]
	    public void Repeat_expansion_annotation_does_not_report_amino_acids()
	    {
		    var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000370457_chrX_Ensembl84"), null as List<string>,
				"chrX	148661918	FMR1	G	<STR8>	.	PASS	SVTYPE=STR;END=148661978;REF=20;RL=60;RU=CGG	GT:SO:SP:CN:CI	1:FLANKING:10:8:5-11");
		    Assert.NotNull(annotatedVariant);
			Assert.DoesNotContain("codons",annotatedVariant.ToString());
		    Assert.DoesNotContain("aminoAcids", annotatedVariant.ToString());
		}
		[Fact]
        public void FullSvDeletionSupport()
        {
            JsonUtilities.AlleleEquals(
                "chr1	964001	.	A	<DEL>	.	PASS	SVTYPE=DEL;SVLEN=-7;IMPRECISE;CIPOS=-170,170;CIEND=-175,175	GT:GQX:DP:DPF	0/0:99:34:2",
                Resources.MiniSuppAnnot("chr1_964001_964008.nsa"),
                "{\"altAllele\":\"deletion\",\"refAllele\":\"A\",\"begin\":964002,\"chromosome\":\"chr1\",\"end\":964008,\"variantType\":\"deletion\",\"vid\":\"1:964002:964008\"}");
        }

        [Fact]
        public void SplitReadInSample()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, null as List<string>,
                "chr7	127717248	MantaINV:267944:0:1:2:0:0	T	<INV>	.	PASS	END=140789466;SVTYPE=INV;SVLEN=13072218;INV5	PR:SR	78,0:65,0	157,42:252,63");
            Assert.NotNull(annotatedVariant);

            var sample1 = JsonUtilities.GetSampleJson(annotatedVariant, 0);

            Assert.Equal("{\"splitReadCounts\":[65,0],\"pairedEndReadCounts\":[78,0]}", sample1);

            var sample2 = JsonUtilities.GetSampleJson(annotatedVariant, 1);

            Assert.Equal("{\"splitReadCounts\":[252,63],\"pairedEndReadCounts\":[157,42]}", sample2);

        }
        [Fact]
        [Trait("jira", "NIR-2032")]
        public void ClinVarAnnotateCrash()
        {
            JsonUtilities.AlleleContains(
                "13	111335401	.	GCTC	G	0	.	.	.",
                Resources.MiniSuppAnnot("chr13_111335401_111335402.nsa"),
                "clinvar");
        }

		[Fact]
		public void SvEndOutput()
		{
			var observedJosn=JsonUtilities.GetJson("chr3	62431401	.	A	<DEL>	.	.	END=62431801;SVTYPE=DEL", _renamer);
			Assert.Contains("\"svEnd\":62431801", observedJosn.ToString());
		}

		[Fact]
		public void NoSvEndOutput()
		{
			var observedJosn = JsonUtilities.GetJson("chr3	62431401	.	A	G	.	.	.", _renamer);
			Assert.DoesNotContain("\"svEnd\"", observedJosn.ToString());
		}

        [Fact]
        public void RecomposedGenotypeOutput()
        {
            var jsonSample = new JsonSample()
            {
                RecomposedGenotype = new List<string> { "2:25:27:CA","2:25:26:AC"}
            };

            Assert.Contains("\"recomposedGenotype\":[\"2:25:27:CA\",\"2:25:26:AC\"]", jsonSample.ToString());
        }
        [Fact]
        public void JsonContainsIsRecomposedVariant()
        {
            var annotationSource = DataUtilities.EmptyAnnotationSource;
            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                VcfUtilities.GetVcfVariant(
                    "chr2	27	rs113815251	G	A	108	PASS	SNVHPOL=3;RAL=chr2:25-27:GTG->GCA,chr2:25-27:GTG->ACG	GT:GQ:GQX:DP:DPF:AD:ADF:ADR:SB:FT:PL:PS:RGT	1|0:141:60:22:7:14,8:8,2:6,6:-10.7:PASS:142,0,216:172541424:1/2"));

            Assert.Equal(3, annotatedVariant.AnnotatedAlternateAlleles.Count);

            Assert.DoesNotContain("\"isRecomposedVariant\"",annotatedVariant.AnnotatedAlternateAlleles[0].ToString());
            Assert.Contains("\"isRecomposedVariant\":true",annotatedVariant.AnnotatedAlternateAlleles[1].ToString());
            Assert.Contains("\"isRecomposedVariant\":true", annotatedVariant.AnnotatedAlternateAlleles[2].ToString());


        }

        [Fact]
        public void GeneAnnotationsAreFormattedCorrectly()
        {
            var omimAnnotation1 = new OmimAnnotation("gene1","gene1 omim Annotation1",12345,new List<OmimAnnotation.Phenotype>());
            var omimAnnotation2 = new OmimAnnotation("gene1", "gene1 omim Annotation2", 12347, new List<OmimAnnotation.Phenotype>());
            var omimAnnotation3 = new OmimAnnotation("gene2", "gene2 omim Annotation1", 12350, new List<OmimAnnotation.Phenotype>());

            var omimiAnnotations = new List<IGeneAnnotation>{omimAnnotation1,omimAnnotation2,omimAnnotation3};

            var obsevedJson = UnifiedJson.FormatGeneAnnotations(omimiAnnotations);
            var expectedOut =
                "\n],\"genes\":["+"\n"+"{\"name\":\"gene1\",\"omim\":[{\"mimNumber\":12345,\"description\":\"gene1 omim Annotation1\"},{\"mimNumber\":12347,\"description\":\"gene1 omim Annotation2\"}]},\n{\"name\":\"gene2\",\"omim\":[{\"mimNumber\":12350,\"description\":\"gene2 omim Annotation1\"}]}";

            Assert.Equal(expectedOut,obsevedJson);
        }
}
}