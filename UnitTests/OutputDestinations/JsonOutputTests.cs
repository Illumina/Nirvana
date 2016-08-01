using System;
using System.Collections.Generic;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.DataStructures.VCF;
using Xunit;

namespace UnitTests.OutputDestinations
{
    [Collection("Chromosome 1 collection")]
    public sealed class JsonOutputTests : RandomFileBase
    {
        [Fact]
        public void AlleleSpecificFlagFalse()
        {
	        JsonUtilities.AlleleContains(
		        "chr1	118165691	rs1630312	C	T	156.00	PASS	SNVSB=-21.8;SNVHPOL=2;AF=0.25;EVS=0.3063|85.0|6503;GMAF=T|0.2523;phastCons;CSQT=FAM46C|NM_017709.3|synonymous_variant	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
		        "chr1_118165691_118165692.nsa",
				"dbsnp\":[\"rs1630312\"],\"end\":118165691,\"globalMinorAllele\":\"T\",\"gmaf\":0.2071,\"variantType\":\"SNV\",\"vid\":\"1:118165691:T\",\"clinVar\":[{\"id\":\"RCV000120902.1\",\"reviewStatus\":\"no assertion\",\"alleleOrigin\":\"germline\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"phenotype\":\"not specified\",\"medGenId\":\"CN169374\",\"significance\":\"not provided\",\"lastEvaluatedDate\":\"2013-09-19\",\"pubMedIds\":[\"24728327\"]}],\"cosmic\":[{\"id\":\"COSM3750276\",\"isAlleleSpecific\":true,\"refAllele\":\"C\",\"altAllele\":\"T\",\"gene\":\"FAM46C\",\"studies\":[{\"id\":544,\"histology\":\"haematopoietic neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"}]},{\"id\":\"COSM4599336\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"gene\":\"FAM46C\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"}]}]");
        }

        [Fact]
        public void AlleleSpecificCosmic()
        {
           	JsonUtilities.AlleleContains(
			   "chr1	111783982	rs1630312	C	A	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
			   "chr1_111783982_111783983.nsa",
			   "\"cosmic\":[{\"id\":\"COSM3996742\",\"isAlleleSpecific\":true,\"refAllele\":\"C\",\"altAllele\":\"A\",\"gene\":\"CHI3L2\",\"studies\":[{\"id\":544,\"histology\":\"haematopoietic neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"}]},{\"id\":\"COSM4591038\",\"refAllele\":\"C\",\"altAllele\":\"T\",\"gene\":\"CHI3L2\",\"studies\":[{\"histology\":\"lymphoid neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"},{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"},{\"histology\":\"osteosarcoma\",\"primarySite\":\"bone\"}]}]");
		}

        [Fact]
        public void FirstExacTest()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_13528_13529.nsa",
                "1	13528	.	C	G,T	1771.54	VQSRTrancheSNP99.60to99.80	AC=21,11;AC_AFR=12,0");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            var altAllele2 = JsonUtilities.GetAllele(annotatedVariant, 1);
            Assert.NotNull(altAllele2);

            Assert.Contains("\"exacCoverage\":28,\"exacAll\":0.001247,\"exacAfr\":0.030769,\"exacAmr\":0.008621,\"exacEas\":0,\"exacFin\":0,\"exacNfe\":0,\"exacOth\":0,\"exacSas\":0", altAllele);
            Assert.Contains("\"exacCoverage\":28,\"exacAll\":0.000863,\"exacAfr\":0,\"exacAmr\":0,\"exacEas\":0,\"exacFin\":0,\"exacNfe\":0.000765,\"exacOth\":0,\"exacSas\":0.000995", altAllele2);
        }

        [Fact]
        public void CarryOverDbsnp()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr2_118565152_118565155.nsa",
                "2	118565152	.	AGC	AGT,CGC	50	PASS	SNVSB=-8.7;SNVHPOL=5;CSQ=AGT|upstream_gene_variant|MODIFIER|AC009312.1|ENSG00000238207|Transcript|ENST00000457110|antisense|||||||||||3402|-1|Clone_based_vega_gene||YES|||||||||,CGC|upstream_gene_variant|MODIFIER|AC009312.1|ENSG00000238207|Transcript|ENST00000457110|antisense|||||||||||3402|-1|Clone_based_vega_gene||YES|||||||||	 GT:GQ:GQX:DP:DPF:AD	1/2:83:21:12:2:0,8,4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleContains(annotatedVariant,
				"{\"altAllele\":\"T\",\"refAllele\":\"C\",\"begin\":118565154,\"chromosome\":\"2\",\"dbsnp\":[\"rs62192625\",\"rs77494680\"],\"end\":118565154,\"globalMinorAllele\":\"T\",\"gmaf\":0.3464,\"variantType\":\"SNV\",\"vid\":\"2:118565154:T");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"C\",\"refAllele\":\"A\",\"begin\":118565152,\"chromosome\":\"2\",\"dbsnp\":[\"rs754609911\"],\"end\":118565152,\"variantType\":\"SNV\",\"vid\":\"2:118565152:C\"}",
                1);
        }

        [Fact]
        public void ClinVarNonEnglishChars()
        {
            JsonUtilities.AlleleContains("1	225592187	.	CTAGAAGA	CCTTCTAG	362	PASS	CIGAR=1M18D",
                "chr1_225592187_225592188.nsa",
                "Pelger-Huët anomaly");
        }

        [Fact]
        public void MissingClinvarIdRefAllele()
        {
            JsonUtilities.AlleleEquals("1	8021910	.	GGTGCTGGACGGTGTCCCT	T	362	PASS	CIGAR=1M18D",
                "chr1_8021910_8021911.nsa",
                "{\"altAllele\":\"-\",\"refAllele\":\"GGTGCTGGACGGTGTCCC\",\"begin\":8021910,\"chromosome\":\"1\",\"dbsnp\":[\"rs767770365\"],\"end\":8021927,\"variantType\":\"deletion\",\"vid\":\"1:8021910:8021927\"}");
        }

        [Fact]
        public void MissingClinvarAlleleOrigin()
        {
            JsonUtilities.AlleleContains(
                "9	120475302	.	A	G	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
                "chr9_120475302_120475303.nsa",
				"clinVar\":[{\"id\":\"RCV000007040.4\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"phenotype\":\"TLR4 POLYMORPHISM\",\"significance\":\"benign\",\"lastEvaluatedDate\":\"2007-09-01\",\"pubMedIds\":[\"10835634\",\"12124407\",\"15547160\",\"15829498\",\"16879199\",\"17704786\"]},{\"id\":\"RCV000007041.2\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"phenotype\":\"MACULAR DEGENERATION, AGE-RELATED, 10, SUSCEPTIBILITY TO\",\"significance\":\"other\",\"lastEvaluatedDate\":\"2007-09-01\",\"pubMedIds\":[\"10835634\",\"12124407\",\"15547160\",\"15829498\",\"16879199\",\"17704786\"]},{\"id\":\"RCV000007042.2\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"phenotype\":\"Colorectal cancer, susceptibility to\",\"medGenId\":\"C1858438\",\"significance\":\"other\",\"lastEvaluatedDate\":\"2007-09-01\",\"pubMedIds\":[\"10835634\",\"12124407\",\"15547160\",\"15829498\",\"16879199\",\"17704786\"]}],\"cosmic\":[{\"id\":\"COSM4988203\",\"isAlleleSpecific\":true,\"refAllele\":\"A\",\"altAllele\":\"G\",\"gene\":\"TLR4\",\"studies\":[{\"histology\":\"rhabdomyosarcoma\",\"primarySite\":\"soft tissue\"},{\"histology\":\"haemangioblastoma\",\"primarySite\":\"soft tissue\"}]}]");
        }

        [Fact]
        public void Uncleared1000GenomeValues()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_8383550_8383560.nsa",
                "1	8383550	.	CAAAAAAAAA	C,CAAAAAAAAAAAAA	100	PASS	.");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
				"{\"altAllele\":\"-\",\"refAllele\":\"AAAAAAAAA\",\"begin\":8383551,\"chromosome\":\"1\",\"dbsnp\":[\"rs34956825\",\"rs774024202\",\"rs796237312\"],\"end\":8383559,\"variantType\":\"deletion\",\"vid\":\"1:8383551:8383559\",\"oneKgAll\":0.921526,\"oneKgAfr\":0.847958,\"oneKgAmr\":0.903458,\"oneKgEas\":0.985119,\"oneKgEur\":0.914513,\"oneKgSas\":0.97546,\"oneKgAllAn\":5008,\"oneKgAfrAn\":1322,\"oneKgAmrAn\":694,\"oneKgEasAn\":1008,\"oneKgEurAn\":1006,\"oneKgSasAn\":978,\"oneKgAllAc\":4615,\"oneKgAfrAc\":1121,\"oneKgAmrAc\":627,\"oneKgEasAc\":993,\"oneKgEurAc\":920,\"oneKgSasAc\":954}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"AAAA\",\"refAllele\":\"-\",\"begin\":8383560,\"chromosome\":\"1\",\"end\":8383559,\"variantType\":\"insertion\",\"vid\":\"1:8383560:8383559:AAAA\"}",
                1);
        }

        [Fact]
        public void Extra1000GenomeValues()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr17_7432913_7432914.nsa",
                "17	7432913	rs34130898	A	AATT,ATTATT	100	PASS	.");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
				"{\"altAllele\":\"ATT\",\"refAllele\":\"-\",\"begin\":7432914,\"chromosome\":\"17\",\"dbsnp\":[\"rs34130898\",\"rs397969947\"],\"end\":7432913,\"variantType\":\"insertion\",\"vid\":\"17:7432914:7432913:ATT\",\"oneKgAll\":0.357827,\"oneKgAfr\":0.303328,\"oneKgAmr\":0.396254,\"oneKgEas\":0.392857,\"oneKgEur\":0.422465,\"oneKgSas\":0.301636,\"oneKgAllAn\":5008,\"oneKgAfrAn\":1322,\"oneKgAmrAn\":694,\"oneKgEasAn\":1008,\"oneKgEurAn\":1006,\"oneKgSasAn\":978,\"oneKgAllAc\":1792,\"oneKgAfrAc\":401,\"oneKgAmrAc\":275,\"oneKgEasAc\":396,\"oneKgEurAc\":425,\"oneKgSasAc\":295}");

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
                "{\"altAllele\":\"[3:115024109[T\",\"refAllele\":\"T\",\"begin\":28722335,\"chromosome\":\"1\",\"end\":28722335,\"variantType\":\"translocation_breakend\",\"vid\":\"1:28722335:+:3:115024109:+\"}");
        }

        [Fact]
        public void ClinvarPhenotype()
        {
            JsonUtilities.AlleleContains(
                "17	2266812	.	T	C	112	LowGQX	SNVSB=-8.7;SNVHPOL=3	GT:GQ:GQX:DP:DPF:AD	1/1:21:21:8:1:0,8",
                "chr17_2266812_2266813.nsa",
                "{\"ancestralAllele\":\"C\",\"altAllele\":\"C\",\"refAllele\":\"T\",\"begin\":2266812,\"chromosome\":\"17\",\"dbsnp\":[\"rs2003968\"],\"end\":2266812,\"globalMinorAllele\":\"T\",\"gmaf\":0.4337,\"variantType\":\"SNV\",\"vid\":\"17:2266812:C\",\"cosmic\":[{\"id\":\"COSM1563374\",\"isAlleleSpecific\":true,\"refAllele\":\"T\",\"altAllele\":\"C\",\"gene\":\"SGSM2\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"},{\"id\":589,\"histology\":\"other\",\"primarySite\":\"thyroid\"}]}]");
        }

        [Fact]
        public void ClinVarAltAllele()
        {
            JsonUtilities.AlleleContains(
                "1	9305316	rs398122818	AC	A	.	.	RS=398122818;RSPOS=9305318;dbSNPBuildID=138;SSR=0;SAO=0;VP=0x050060001205000002110200;GENEINFO=H",
                "chr1_9305316_9305317.nsa",
				"\"clinVar\":[{\"id\":\"RCV000024293.28\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"C\",\"altAllele\":\"-\",\"phenotype\":\"Cortisone reductase deficiency 1\",\"medGenId\":\"C3551716\",\"omimId\":\"604931\",\"significance\":\"pathogenic\",\"lastEvaluatedDate\":\"2008-10-01\",\"pubMedIds\":[\"11150889\",\"18628520\"]}]"
				);
        }

        [Fact]
        public void CosmicAlleleContains()
        {
            JsonUtilities.AlleleContains(
                "1	898602	COSM2151955	GCG	G	.	.	GENE=KLHL17;STRAND=+;CDS=c.1157_1158delCG;AA=p.A386fs*12;CNT=1",
                "chr1_898602_898603.nsa",
                "\"cosmic\":[{\"id\":\"COSM2150687\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"gene\":\"KLHL17\",\"studies\":[{\"histology\":\"glioma\",\"primarySite\":\"central nervous system\"}]},{\"id\":\"COSM2151955\",\"isAlleleSpecific\":true,\"refAllele\":\"CG\",\"altAllele\":\"-\",\"gene\":\"KLHL17\",\"studies\":[{\"histology\":\"glioma\",\"primarySite\":\"central nervous system\"}]}]");
        }

        [Fact]
        public void CosmicMultiDelete()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr17_21319650_21319651.nsa",
                "17	21319650	.	CGAG	C	101	PASS	CIGAR=1M3D;RU=GAG;REFREP=2;IDREP=1	GT:GQ:GQX:DPI:AD	0/1:141:101:29:22,4");

            AssertUtilities.CheckJsonContains(
                "{\"id\":\"COSM3735158\",\"refAllele\":\"G\",\"altAllele\":\"-\",\"gene\":\"KCNJ12\",\"studies\":[{\"histology\":\"malignant melanoma\",\"primarySite\":\"skin\"}]}",
                annotatedVariant);

            AssertUtilities.CheckJsonContains(
                "{\"id\":\"COSM278475\",\"isAlleleSpecific\":true,\"refAllele\":\"GAG\",\"altAllele\":\"-\",\"gene\":\"KCNJ12\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"},{\"id\":376,\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"}]}",
                annotatedVariant);
        }

        [Fact]
        public void CnvIgnoreFlankingGenes()
        {
            var annotatedVariant = DataUtilities.GetVariant("ENST00000427857_chr1_Ensembl84.ndb", null,
                "1	816800	Canvas:GAIN:1:816801:821943	N	<CNV>	2	q10;CLT10kb	SVTYPE=CNV;END=821943	RC:BC:CN	174:2:4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":816801,\"chromosome\":\"1\",\"end\":821943,\"variantType\":\"copy_number_variation\",\"vid\":\"1:816801:821943:4\"}");
        }

        [Fact]
        public void AnnotationCarryover()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr2_90472571_90472592.nsa",
                "2	90472571	.	AAAAAAAAAAAAAAAAAAGTCC	AGTCT	177	PASS	CIGAR=1M21D4I;RU=.;REFREP=.;IDREP=.	GT:GQ:GQX:DPI:AD	0/1:220:177:46:40,7");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetFirstAlleleJson(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal("{\"altAllele\":\"GTCT\",\"refAllele\":\"AAAAAAAAAAAAAAAAAGTCC\",\"begin\":90472572,\"chromosome\":\"2\",\"end\":90472592,\"variantType\":\"indel\",\"vid\":\"2:90472572:90472592:GTCT\"}", altAllele);

            var annotatedVariant2 = DataUtilities.GetVariant(null, "chr2_90472571_90472592.nsa",
                "2	90472592	.	C	.	.	PASS	RefMinor	GT:GQX:DP:DPF:AD	0:96:33:15:33");
            Assert.NotNull(annotatedVariant2);

            var altAllele2 = JsonUtilities.GetFirstAlleleJson(annotatedVariant2);
            Assert.NotNull(altAllele2);

            Assert.Equal("{\"refAllele\":\"C\",\"begin\":90472592,\"chromosome\":\"2\",\"end\":90472592,\"globalMinorAllele\":\"C\",\"gmaf\":0.006989,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"2:90472592:C\"}", altAllele2);
        }

        [Fact]
        public void SvDelMissingTranscript()
        {
            var annotatedVariant = DataUtilities.GetVariant("ENST00000594233_chr1_Ensembl84.ndb", null,
                "1	823854	MantaDEL:60:0:0:0:2:4	AGGGAGAGAAAGGGAAAGAAGGAAAGAAACAAGGAAGCAGGGAGGGAGAGAAAGAGGAAGGGAG	A	49	MaxDepth;MaxMQ0Frac	END=823917;SVTYPE=DEL;SVLEN=-63;CIGAR=1M63D;CSQT=1|AL645608.2|ENST00000594233|  GT:GQ:PR:SR 0/1:49:21,0:35,8");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"-\",\"refAllele\":\"GGGAGAGAAAGGGAAAGAAGGAAAGAAACAAGGAAGCAGGGAGGGAGAGAAAGAGGAAGGGAG\",\"begin\":823855,\"chromosome\":\"1\",\"end\":823917,\"variantType\":\"deletion\",\"vid\":\"1:823855:823917\",\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000594233\",\"geneId\":\"ENSG00000269308\",\"hgnc\":\"AL645608.2\",\"consequence\":[\"downstream_gene_variant\"],\"isCanonical\":true,\"proteinId\":\"ENSP00000470877\"}]}}");
        }

        [Fact]
        public void TrimClinvarAllele()
        {
            // 13      40298638        rs66629036      TA      T       .       .       RS=66629036;RSPOS=40298641;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000080005000002000200;GENEINFO=COG6:57511;WGT=1;VC=DIV;INT;ASP;CLNALLE=1;CLNHGVS=NC_000013.10:g.40298641delA;CLNSRC=.;CLNORIGIN=1;CLNSRCID=.;CLNSIG=2;CLNDSDB=MedGen;CLNDSDBID=CN169374;CLNDBN=not_specified;CLNREVSTAT=single;CLNACC=RCV000082045.4
            JsonUtilities.AlleleContains(
                "13	40298637	rs66629036	TTA	TT	.	.	PASS	RefMinor;GMAF=T|0.01877 GT:GQX:DP:DPF:AD        0/0:69:24:3:24",
                "chr13_40298638_40298639.nsa",
				"{\"id\":\"RCV000082045.4\",\"reviewStatus\":\"single submitter\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"-\",\"phenotype\":\"not specified\",\"medGenId\":\"CN169374\",\"significance\":\"benign\",\"lastEvaluatedDate\":\"2013-05-01\",\"pubMedIds\":[\"23757202\"]}");
        }

        [Fact]
        public void CosmicMissingStudy()
        {
            JsonUtilities.AlleleContains(
                "chr2	203066048	.	G	T	39.00	PASS	SNVSB=-4.1;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:72:39:37:1:30,7",
                "chr2_203066046_203066050.nsa",
                "\"cosmic\":[{\"id\":\"COSN166383\",\"isAlleleSpecific\":true,\"refAllele\":\"G\",\"altAllele\":\"T\"}]");
        }

        [Fact]
        public void CosmicLongDelInsAlleleError()
        {
            JsonUtilities.AlleleContains(
                "chr18	58484430	.	AAT	A	39.00	PASS	SNVSB=-4.1;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:72:39:37:1:30,7",
                "chr18_58484430_58484431.nsa",
                "\"cosmic\":[{\"id\":\"COSN197004\",\"refAllele\":\"ATGTGAAAAATATATTTTATATAATTTCAATATTTTTAACA\",\"altAllele\":\"TTGAAAAATATATTTTATATAATTTCAATATTTTTAACAT\"}]");
        }

        [Fact]
        public void BadClivarRef()
        {
            JsonUtilities.AlleleEquals(
                "11	109157259	.	T	.	.	PASS	RefMinor;GMAF=T|0.01877 GT:GQX:DP:DPF:AD        0/0:69:24:3:24",
                "chr11_109157259_109157260.nsa",
                "{\"refAllele\":\"T\",\"begin\":109157259,\"chromosome\":\"11\",\"end\":109157259,\"globalMinorAllele\":\"T\",\"gmaf\":0.01877,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"11:109157259:T\"}");
        }

        [Fact]
        public void CosmicAlleleSpecificFlag()
        {
            JsonUtilities.AlleleContains(
                "chr1	565591	rs7416152	C	T	39.00	PASS	SNVSB=-4.1;SNVHPOL=4;cosmic=COSN210317	GT:GQ:GQX:DP:DPF:AD	0/1:72:39:37:1:30,7",
                "chr1_565591_565592.nsa",
                "\"cosmic\":[{\"id\":\"COSN210317\",\"isAlleleSpecific\":true,\"refAllele\":\"C\",\"altAllele\":\"T\"}]");
        }

        [Fact]
        public void RefAlleleDash()
        {
            JsonUtilities.AlleleEquals(
                "17	46107	.	A	G	153	LowGQX SNVSB=-20.1;SNVHPOL=4;CSQ=G|ENSG00000262836|ENST00000576171|Transcript|upstream_gene_variant||||||||4729|||||||YES|||||||| GT:GQ:GQX:DP:DPF:AD	1/1:18:18:7:0:0,7",
                "chr17_77263_77265.nsa",
                "{\"altAllele\":\"G\",\"refAllele\":\"A\",\"begin\":46107,\"chromosome\":\"17\",\"end\":46107,\"variantType\":\"SNV\",\"vid\":\"17:46107:G\"}");
        }

        [Fact]
        public void CosmicAltAlleleN()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_17086085_17086086.nsa",
                "1	17086085	.	G	GC	209	LowGQX	.	GT:GQ:GQX:DPI:AD	1/1:12:9:5:0,4");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains(
				"{\"id\":\"COSM424569\",\"refAllele\":\"-\",\"altAllele\":\"N\",\"gene\":\"Q13209_HUMAN\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"pancreas\"},{\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"},{\"id\":419,\"histology\":\"carcinoma\",\"primarySite\":\"endometrium\"}]}",
                annotatedVariant);

            AssertUtilities.CheckJsonContains(
				"{\"id\":\"COSM424570\",\"refAllele\":\"-\",\"altAllele\":\"N\",\"gene\":\"MST1P9\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"pancreas\"},{\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"},{\"id\":419,\"histology\":\"carcinoma\",\"primarySite\":\"endometrium\"}]}",
                annotatedVariant);
        }

        [Fact]
        public void CosmicAltAllele()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr17_6928019_6928021.nsa",
                "17	6928019	.	C	CCAG	209	LowGQX	.	GT:GQ:GQX:DPI:AD	1/1:12:9:5:0,4");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains(
				"{\"id\":\"COSM983707\",\"isAlleleSpecific\":true,\"refAllele\":\"-\",\"altAllele\":\"CAG\",\"gene\":\"BCL6B_ENST00000293805\",\"studies\":[{\"id\":419,\"histology\":\"carcinoma\",\"primarySite\":\"endometrium\"},{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"}]}",
                annotatedVariant);

            AssertUtilities.CheckJsonContains(
				"{\"id\":\"COSM983708\",\"isAlleleSpecific\":true,\"refAllele\":\"-\",\"altAllele\":\"CAG\",\"gene\":\"BCL6B\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"},{\"id\":419,\"histology\":\"carcinoma\",\"primarySite\":\"endometrium\"}]}",
                annotatedVariant);
        }

        [Fact]
        public void IsAlleleSpecificNotSetJson()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_24122986_24122987.nsa",
                "1	24122986	rs760941	C	G,T	.	.	.");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            var altAllele2 = JsonUtilities.GetAllele(annotatedVariant, 1);
            Assert.NotNull(altAllele2);

            const string asClinVar = "\"clinVar\":[{\"id\":\"RCV000078694.4\",\"reviewStatus\":\"single submitter\",\"isAlleleSpecific\":true";
            const string asCosmic = "\"cosmic\":[{\"id\":\"COSN1100872\",\"isAlleleSpecific\":true";

            Assert.DoesNotContain(asClinVar, altAllele);
            Assert.Contains(asCosmic, altAllele);

            Assert.Contains(asClinVar, altAllele2);
            Assert.DoesNotContain(asCosmic, altAllele2);
		    //Assert.True(observedJsonLine.Contains("cosmic\":[{\"id\":\"COSN1100872\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"gene\":\"GALE\"},{\"id\":\"COSN1100873\",\"refAllele\":\"C\",\"altAllele\":\"G\",\"gene\":\"GALE\"}]"));
        }

        [Fact]
        public void DuplicateDbSnpJson()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_8121167_8121168.nsa",
                "1	8121167	.	C	CAAT,CAATAAT	.	.	.");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"AAT\",\"refAllele\":\"-\",\"begin\":8121168,\"chromosome\":\"1\",\"dbsnp\":[\"rs34500567\",\"rs59792241\"],\"end\":8121167,\"variantType\":\"insertion\",\"vid\":\"1:8121168:8121167:AAT\"}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"AATAAT\",\"refAllele\":\"-\",\"begin\":8121168,\"chromosome\":\"1\",\"end\":8121167,\"variantType\":\"insertion\",\"vid\":\"1:8121168:8121167:AATAAT\"}",
                1);
        }

        [Fact]
        public void MissingRsid()
        {
            JsonUtilities.AlleleContains(
                "chr1	129010	rs377161483	AATG	A	32	LowGQXHetAltDel	CIGAR=1M1D1M,2M1D;RU=C,A;REFREP=1,17;IDREP=0,16	GT:GQ:GQX:DPI:AD	1/2:162:2:22:4,8,1",
                "chr1_129010_129012.nsa",
                "{\"altAllele\":\"-\",\"refAllele\":\"ATG\",\"begin\":129011,\"chromosome\":\"chr1\",\"dbsnp\":[\"rs377161483\"],\"end\":129013,\"variantType\":\"deletion\",\"vid\":\"1:129011:129013\"");
        }

        [Fact]
        public void MultipleDbSnpIds()
        {
            var sa = new SupplementaryAnnotation(115256529)
            {
                AlleleSpecificAnnotations =
                           {
                               ["C"] = new SupplementaryAnnotation.AlleleSpecificAnnotation()
                           }
            };
            sa.AlleleSpecificAnnotations["C"].DbSnp.AddRange(new List<long> { 111, 222, 333 });

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            JsonUtilities.AlleleContains(
                "chr1	115256529	.	T	C	1000	PASS	.	GT	0/1",
                saFilename,
                "\"dbsnp\":[\"rs111\",\"rs222\",\"rs333\"]");
        }

        [Fact]
        public void ClinvarAlleleOrigin()
        {
            JsonUtilities.AlleleContains(
                "9	120475302	.	A	G	156.00	PASS	SNVSB=-21.8;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14",
                "chr9_120475302_120475303.nsa",
				"\"clinVar\":[{\"id\":\"RCV000007040.4\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"phenotype\":\"TLR4 POLYMORPHISM\",\"significance\":\"benign\",\"lastEvaluatedDate\":\"2007-09-01\",\"pubMedIds\":[\"10835634\",\"12124407\",\"15547160\",\"15829498\",\"16879199\",\"17704786\"]},{\"id\":\"RCV000007041.2\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"phenotype\":\"MACULAR DEGENERATION, AGE-RELATED, 10, SUSCEPTIBILITY TO\",\"significance\":\"other\",\"lastEvaluatedDate\":\"2007-09-01\",\"pubMedIds\":[\"10835634\",\"12124407\",\"15547160\",\"15829498\",\"16879199\",\"17704786\"]},{\"id\":\"RCV000007042.2\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"phenotype\":\"Colorectal cancer, susceptibility to\",\"medGenId\":\"C1858438\",\"significance\":\"other\",\"lastEvaluatedDate\":\"2007-09-01\",\"pubMedIds\":[\"10835634\",\"12124407\",\"15547160\",\"15829498\",\"16879199\",\"17704786\"]}]");
        }

        [Fact]
        public void MissingCosmicId()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_26608814_26608815.nsa",
                "1	26608811	.	TCCAGGACAGGGACTGGGGCCGGGACCGGGACC	TCCGGGACC,TCCAGGACA	139	LowGQXHetAltDel	CIGAR=1M24D8M,9M24");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele  = JsonUtilities.GetAllele(annotatedVariant);
            var altAllele2 = JsonUtilities.GetAllele(annotatedVariant, 1);

            Assert.Contains(
                "\"cosmic\":[{\"id\":\"COSM4143711\",\"refAllele\":\"A\",\"altAllele\":\"G\",\"gene\":\"UBXN11\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"upper aerodigestive tract\"},{\"id\":589,\"histology\":\"other\",\"primarySite\":\"thyroid\"}]}]",
                altAllele);

            Assert.DoesNotContain("\"cosmic\":", altAllele2);
        }

        [Fact]
        public void MissingClinvarId()
        {
            JsonUtilities.AlleleContains("1	55518316	rs2483205	C	T	.	.	RS=2483205;RSPOS=55518316",
                "chr1_55518316_55518317.nsa",
				"\"clinVar\":[{\"id\":\"RCV000030351.1\",\"reviewStatus\":\"single submitter\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"unknown\",\"refAllele\":\"C\",\"altAllele\":\"T\",\"phenotype\":\"Familial hypercholesterolemia\",\"medGenId\":\"C0020445\",\"omimId\":\"143890\",\"significance\":\"benign\",\"snoMedCtId\":\"397915002,398036000\",\"lastEvaluatedDate\":\"2011-08-18\"}]"
				);

        }

        [Fact]
        public void MissingRegulatoryFeatureJson()
        {
            var annotatedVariant = DataUtilities.GetVariant("ENSR00000669067_chr1_Ensembl84_reg.ndb", null,
                "1	16833395	.	G	A,T	.	LowQscore	SOMATIC;QSS=268;TQSS=2;NT=conflict;QSS_NT=0;TQSS_NT=2;SGT=GT->AT;DP=431;MQ=53.95;MQ0=60;ALTPOS=41;ALTMAP=25;ReadPosRankSum=0.74;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=0.00;CSQ=A|regulatory_region_variant|MODIFIER|||RegulatoryFeature|ENSR00000669067|,T|regulatory_region_variant|MODIFIER|||RegulatoryFeature|ENSR00000669067| DP:FDP:SDP:SUBDP:AU:CU:GU:TU	130:1:0:0:17,18:0,0:27,27:85,111	236:3:0:0:52,54:1,1:27,27:153,191");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"A\",\"refAllele\":\"G\",\"begin\":16833395,\"chromosome\":\"1\",\"end\":16833395,\"variantType\":\"SNV\",\"vid\":\"1:16833395:A\",\"regulatoryRegions\":[{\"id\":\"ENSR00000669067\",\"consequence\":[\"regulatory_region_variant\"]}]}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"T\",\"refAllele\":\"G\",\"begin\":16833395,\"chromosome\":\"1\",\"end\":16833395,\"variantType\":\"SNV\",\"vid\":\"1:16833395:T\",\"regulatoryRegions\":[{\"id\":\"ENSR00000669067\",\"consequence\":[\"regulatory_region_variant\"]}]}",
                1);
        }

        [Fact]
        public void MissingRefAllele()
        {
            JsonUtilities.AlleleEquals(
                "chr1	15274	.	A	.	279.00	PASS	SNVSB=-39.1;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	1/2:58:55:20:1:0,5,15",
                "chr1_15274_15275.nsa",
                "{\"refAllele\":\"A\",\"begin\":15274,\"chromosome\":\"chr1\",\"end\":15274,\"globalMinorAllele\":\"G\",\"gmaf\":0.3472,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"1:15274:A\"}");
        }

        [Fact]
        public void OutputStrandBias()
        {
            var unifiedJson =
                JsonUtilities.GetJson(
                    "chrX	2699246	rs148553620	C	A	250.95	VQSRTrancheSNP99.00to99.90	AC=2;AF=0.250;AN=8;BaseQRankSum=1.719;DB;DP=106;Dels=0.00;FS=20.202;HaplotypeScore=0.0000;MLEAC=2;MLEAF=0.250;MQ=43.50;MQ0=52;MQRankSum=2.955;QD=4.73;ReadPosRankSum=1.024;SB=-1.368e+02;VQSLOD=-0.3503;culprit=MQ;PLF  GT:AD:DP:GQ:PL:AA   0:10,6:16:9:0,9,118:P1,.	   0|1:12,11:23:27:115,0,27:M1,M2   0|0:37,0:37:18:0,18,236:M1,P1   1|0:13,17:30:59:177,0,59:M2,P1");

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
            var annotatedVariant = DataUtilities.GetVariant(null, "chr17_3124877_3124878.nsa",
                "17	3124877	rs182093170	T	A,C	87	LowGQX	SNVSB=-11.7;SNVHPOL=2;phyloP=0.058	GT:GQ:GQX:DP:DPF:AD	1/2:7:5:5:1:0,1,4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"A\",\"refAllele\":\"T\",\"begin\":3124877,\"chromosome\":\"17\",\"dbsnp\":[\"rs182093170\"],\"end\":3124877,\"variantType\":\"SNV\",\"vid\":\"17:3124877:A\"}");

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"C\",\"refAllele\":\"T\",\"begin\":3124877,\"chromosome\":\"17\",\"end\":3124877,\"variantType\":\"SNV\",\"vid\":\"17:3124877:C\"}",
                1);
        }

        [Fact]
        public void MultipleEntryForGenomicVariant()
        {
            // reference no-call
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_22426387_22426388.nsa",
                "1	22426387	.	A	.	.	LowGQX	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", null, true);

            Assert.Equal(
                "{\"chromosome\":\"1\",\"refAllele\":\"A\",\"position\":22426387,\"filters\":[\"LowGQX\"],\"samples\":[{\"totalDepth\":0}],\"variants\":[{\"refAllele\":\"A\",\"begin\":22426387,\"chromosome\":\"1\",\"end\":22426406,\"variantType\":\"reference_no_call\",\"vid\":\"1:22426387:22426406:NC\"}]}",
                annotatedVariant.ToString());

            // reference
            var annotatedVariant2 = DataUtilities.GetVariant(null, "chr1_22426387_22426388.nsa",
                "1	22426387	.	A	.	.	PASS	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", null, true);

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
                    "2	167760371	rs267598980	G	A	.	PASS	SOMATIC;QSS=34;TQSS=1;NT=ref;QSS_NT=34;TQSS_NT=1;SGT=GG->AG;DP=45;MQ=60.00;MQ0=0;ALTPOS=43;ALTMAP=19;ReadPosRankSum=-1.64;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=5.54;cosmic=COSM36673;clinvar=other;CSQT=A|XIRP2|ENST00000409195|missense_variant	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	19:0:0:0:0,0:0,0:19,19:0,0	26:0:0:0:6,6:0,0:20,20:0,0");
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
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_22426387_22426388.nsa",
                "1	22426387	.	A	.	.	LowGQX	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", null, true);
            Assert.NotNull(annotatedVariant);

            Assert.Equal("{\"chromosome\":\"1\",\"refAllele\":\"A\",\"position\":22426387,\"filters\":[\"LowGQX\"],\"samples\":[{\"totalDepth\":0}],\"variants\":[{\"refAllele\":\"A\",\"begin\":22426387,\"chromosome\":\"1\",\"end\":22426406,\"variantType\":\"reference_no_call\",\"vid\":\"1:22426387:22426406:NC\"}]}", annotatedVariant.ToString());
        }

        [Fact]
        public void RefNoCallStrelkaFields()
        {
            var json =
                JsonUtilities.GetJson(
                    "1	205664649	.	C	.	.	LowQscore	SOMATIC;QSS=3;TQSS=1;NT=ref;QSS_NT=3;TQSS_NT=1;SGT=CC->CC;DP=116;MQ=57.63;MQ0=9;ALTPOS=89;ALTMAP=55;ReadPosRankSum=-1.14;SNVSB=6.63;PNOISE=0.00;PNOISE2=0.00;VQSR=0.15	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	11:4:0:0:0,0:7,8:0,0:0,0	92:28:0:0:5,17:58,62:1,1:0,0");
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

            var annotatedVariant = DataUtilities.GetVariant("ENST00000377403_chr1_Ensembl84.ndb", null, vcfLine);
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":9314202,\"chromosome\":\"1\",\"end\":9404148,\"variantType\":\"copy_number_variation\",\"vid\":\"1:9314202:9404148:6\",\"overlappingGenes\":[\"H6PD\"],\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000377403\",\"exons\":\"4-5/5\",\"introns\":\"3-4/4\",\"geneId\":\"ENSG00000049239\",\"hgnc\":\"H6PD\",\"consequence\":[\"copy_number_increase\"],\"isCanonical\":true,\"proteinId\":\"ENSP00000366620\"}]}}");

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
            var annotatedVariant = DataUtilities.GetVariant("ENST00000546909_chr14_Ensembl84.ndb", null,
                "14	19431000	14_19462000	G	<CNV>	.	PASS	SVTYPE=CNV;END=19462000;CN=0;CNscore=13.41;LOH=0;ensembl_gene_id=ENSG00000257990,ENSG00000257558");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"G\",\"begin\":19431001,\"chromosome\":\"14\",\"end\":19462000,\"variantType\":\"copy_number_variation\",\"vid\":\"14:19431001:19462000:?\",\"overlappingGenes\":[\"RP11-536C10.15\"]}");

            int observedCopyNumber =Convert.ToInt32(annotatedVariant.CopyNumber);

            Assert.Equal(0, observedCopyNumber);
            Assert.Contains("\"copyNumber\":0", annotatedVariant.ToString());
        }

        [Fact]
        public void AddExonTest()
        {
            var annotatedVariant = DataUtilities.GetVariant("ENST00000427857_chr1_Ensembl84.ndb", null,
                "1	803780	Canvas:GAIN:1:803781:821943	N	<CNV>	2	q10;CLT10kb	SVTYPE=CNV;END=821943	RC:BC:CN	174:2:4");
            Assert.NotNull(annotatedVariant);

            JsonUtilities.AlleleEquals(annotatedVariant,
                "{\"altAllele\":\"CNV\",\"refAllele\":\"N\",\"begin\":803781,\"chromosome\":\"1\",\"end\":821943,\"variantType\":\"copy_number_variation\",\"vid\":\"1:803781:821943:4\",\"overlappingGenes\":[\"FAM41C\"],\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000427857\",\"exons\":\"1-3/3\",\"introns\":\"1-2/2\",\"geneId\":\"ENSG00000230368\",\"hgnc\":\"FAM41C\",\"consequence\":[\"transcript_amplification\",\"copy_number_increase\"]}]}}");
        }

        [Fact]
        public void FullSvDeletionSupport()
        {
            JsonUtilities.AlleleEquals(
                "chr1	964001	.	A	<DEL>	.	PASS	SVTYPE=DEL;SVLEN=-7;IMPRECISE;CIPOS=-170,170;CIEND=-175,175	GT:GQX:DP:DPF	0/0:99:34:2",
                "chr1_964001_964008.nsa",
                "{\"altAllele\":\"deletion\",\"refAllele\":\"A\",\"begin\":964002,\"chromosome\":\"chr1\",\"end\":964008,\"variantType\":\"deletion\",\"vid\":\"1:964002:964008\"}");
        }
    }
}