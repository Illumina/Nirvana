using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.SupplementaryAnnotations
{
    [Collection("Chromosome 1 collection")]
    public sealed class SupplementaryAnnotationTests : RandomFileBase
    {
        #region members

        private readonly VcfUtilities _vcfUtilities = new VcfUtilities();

        #endregion

        [Fact]
        public void DuplicatePositionalAlleles()
        {
            var asa1 = new SupplementaryAnnotation.AlleleSpecificAnnotation
            {
                AncestralAllele = "C",
				OneKgAllAn = 5008,
				OneKgAllAc = 2049
            };
            asa1.DbSnp.Add(11078537);

            var asa2 = new SupplementaryAnnotation.AlleleSpecificAnnotation
            {
                AncestralAllele = "C",
				OneKgAllAn = 5008,
				OneKgAllAc = 1200
            };
            asa2.DbSnp.Add(11078537);

            var sa = new SupplementaryAnnotation(4634317)
            {
                GlobalMinorAllele = "A",
                GlobalMinorAlleleFrequency = "0.4091",
                AlleleSpecificAnnotations =
                {
                    ["A"] = asa1,
                    ["T"] = asa2
                }
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr17", saFilename);

            const string vcfLine = "17	4634317	.	C	A,T	256	PASS	SNVSB=-27.1;SNVHPOL=7	GT	1/2";
            var annotatedVariant = DataUtilities.GetVariant(null, saFilename, vcfLine);
            Assert.NotNull(annotatedVariant);

            var outputVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);
            Assert.NotNull(outputVcfLine);

            var vcfColumns = outputVcfLine.Split('\t');
            var idField = vcfColumns[VcfCommon.IdIndex];
            var infoField = vcfColumns[VcfCommon.InfoIndex];

            // AF1000G
            var observedAf1000G = VcfUtilities.FindInfoValue("AF1000G", infoField);
            const string expectedAf1000G = "0.409145,0.239617";
            Assert.Equal(expectedAf1000G, observedAf1000G);

            // AA
            var observedAncestralAllele = VcfUtilities.FindInfoValue("AA", infoField);
            const string expectedAncestralAllele = "C,C";
            Assert.Equal(expectedAncestralAllele, observedAncestralAllele);

            // dbSNP
            const string expectedDbSnp = "rs11078537";
            Assert.Equal(expectedDbSnp, idField);

            // GMAF
            var observedGlobalMinorAllele = VcfUtilities.FindInfoValue("GMAF", infoField);
            const string expectedGlobalMinorAllele = "A|0.4091,A|0.4091";
            Assert.Equal(expectedGlobalMinorAllele, observedGlobalMinorAllele);
        }

        [Fact]
        public void DuplicateOneKentry()
        {
            // X       1619046.C       A       100     PASS AC = 2620; AF = 0.523163; AN = 5008; NS = 2504; DP = 15896; AMR_AF = 0.6412; AFR_AF = 0.1415; EUR_AF = 0.6153; SAS_AF = 0.5419; EAS_AF = 0.8323; AA = c |||; VT = SNP
            // X       1619046.C       A,G     100     PASS AC = 2163,730; AF = 0.431909,0.145767; AN = 5008; NS = 2504; DP = 15896; AMR_AF = 0.428,0.3372; AFR_AF = 0.1422,0.0159; EUR_AF = 0.4036,0.3419; SAS_AF = 0.4622,0.1299; EAS_AF = 0.8135,0.004; AA = c |||; VT = SNP; MULTI_ALLELIC
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_1619046_1619046.nsa",
                "X	1619046	.	C	A	100	PASS	AC=2620;AF=0.523163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("oneKgAll", annotatedVariant.ToString());
        }

        [Fact]
        public void NoGlobalMinorAllele()
        {
            _vcfUtilities.FieldEquals(
                "1	241369	.	C	T	77	LowGQXHomSNP	SNVSB=0.0;SNVHPOL=3;CSQ=T|intron_variant&non_coding_transcript_variant|MODIFIER|AP006222.2|ENSG00000228463|Transcript|ENST00000424587|lincRNA||2/3|ENST00000424587.2:n.264-2802G>A|||||||||-1|Clone_based_vega_gene||YES|||||||||,T|upstream_gene_variant|MODIFIER|AP006222.2|ENSG00000228463|Transcript|ENST00000448958|lincRNA|||||||||||2811|-1|Clone_based_vega_gene||||||||||| GT:GQ:GQX:DP:DPF:AD 1/1:12:13:5:2:0,5",
                "chr1_241369_241370.nsa", "SNVSB=0.0;SNVHPOL=3;AF1000G=1",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void ExtraExac()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_3592884_3592885.nsa", "X	3592884	.	G	A	.	.	.");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("exacAll", annotatedVariant.ToString());
        }

        [Fact]
        public void DuplicateOneK()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_2740323_2740323.nsa",
                "X	2740323	.	G	A	100	PASS	AC=39;AF=0.0103311;AN=3775;NS=2504;DP=16253;AMR_AF=0.0029;AFR_AF=0.025;EUR_AF=0.004;SAS_AF=0;EAS_AF=0;AA=.|||;VT=SNP");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("oneKgAll", annotatedVariant.ToString());
        }

        [Fact]
        public void DuplicateOneK2()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_5331877_5331879.nsa",
                "X	5331877	.	AAC	A	100	PASS	AC=2620;AF=0.523163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("oneKgAll", annotatedVariant.ToString());
        }

        [Fact]
        public void ExacClearing()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr9_21974694_21974696.nsa",
                "9	21974694	.	CGT	C,CT	5216.02	PASS	.");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);

            Assert.Contains("exac", altAllele);
            Assert.DoesNotContain("exac", altAllele2.ToString());
        }

        [Fact]
        public void MissingClinVarVcf()
        {
            _vcfUtilities.FieldEquals("17	73512643	rs398124622	T	TGGAGCC	.	.	.", "chr17_73512643_73512644.nsa",
                "AF1000G=0.059105;clinvar=1|other",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void OnekGenArbitrationVcf2()
        {
            _vcfUtilities.FieldEquals("X	534468	.	T	C	100	PASS	.", "chrX_534468_534469.nsa", "GMAF=G|0.2891", VcfCommon.InfoIndex);
        }

        [Fact]
        public void OnekGenArbitrationVcf()
        {
            _vcfUtilities.FieldEquals("X	129354240	rs1160681	C	A	100	PASS	.", "chrX_129354240_129354241.nsa", "GMAF=C|0.4713",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void AlleleSpecificClinvarVcf()
        {
            _vcfUtilities.FieldEquals(
                "17	2266812	rs2003968	T	C	112	LowGQX	SNVSB=-8.7;SNVHPOL=3;AA=C;GMAF=C|0.5663;AF1000G=0.566294;EVS=0.4839|27|6502;phyloP=-5.078;cosmic=COSM1563374;clinvar=1|other;CSQT=1|SGSM2|ENST00000268989|synonymous_variant	GT:GQ:GQX:DP:DPF:AD	1/1:21:21:8:1:0,8",
                "chr17_2266812_2266813.nsa",
                "SNVSB=-8.7;SNVHPOL=3;AA=C;GMAF=T|0.4337;AF1000G=0.566294;EVS=0.483928|27|6502;cosmic=1|COSM1563374",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void ClinvarAlleleSpecific()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr5_112064826_112064826.nsa",
                "chr5	112064826	.	G	.	222	PASS	CIGAR=1M2D");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.True(altAllele.IsReferenceMinor);
            Assert.Null(altAllele.ClinVarEntries.First().IsAlleleSpecific);
        }

        [Fact]
        public void ClinVarUnknownAlleleShouldNotBeReported()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr13_40298637_40298638.nsa",
                "chr13	40298637	.	TTA	T	222	PASS	CIGAR=1M2D");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            //Assert.Contains(
                //"{\"altAllele\":\"-\",\"refAllele\":\"TA\",\"begin\":40298638,\"chromosome\":\"chr13\",\"dbsnp\":[\"rs796887249\"],\"end\":40298639,\"globalMinorAllele\":\"A\",\"gmaf\":0.008786,\"variantType\":\"deletion\",\"vid\":\"13:40298638:40298639\",\"cosmic\":[{\"id\":\"COSM4214738\",\"refAllele\":\"T\",\"altAllele\":\"A\",\"gene\":\"COG6\",\"studies\":[{\"id\":646,\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"}]},{\"id\":\"COSM3730300\",\"isAlleleSpecific\":true,\"refAllele\":\"TA\",\"altAllele\":\"-\",\"gene\":\"COG6\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"oesophagus\"}]},{\"id\":\"COSM3730301\",\"isAlleleSpecific\":true,\"refAllele\":\"TA\",\"altAllele\":\"-\",\"gene\":\"COG6_ENST00000416691\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"oesophagus\"}]},{\"id\":\"COSM4214739\",\"refAllele\":\"T\",\"altAllele\":\"A\",\"gene\":\"COG6_ENST00000416691\",\"studies\":[{\"id\":646,\"histology\":\"carcinoma\",\"primarySite\":\"large intestine\"}]}]",                altAllele);
				Assert.DoesNotContain("clinVar",altAllele);
        }

	    [Fact]
	    public void ClinvarBlankRefAllele()
	    {
			var annotatedVariant = DataUtilities.GetVariant(null, "chrX_17705850_17705851.nsa",
							"chrX	17705850	.	C	CT	222	PASS	CIGAR=1M2D");
			Assert.NotNull(annotatedVariant);

			var altAllele = JsonUtilities.GetAllele(annotatedVariant);
			Assert.NotNull(altAllele);

			Assert.True(altAllele.Contains("\"clinVar\":[{\"id\":\"RCV000082806.4\",\"reviewStatus\":\"single submitter\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"-\",\"altAllele\":\"T\",\"phenotype\":\"not specified\",\"medGenId\":\"CN169374\",\"significance\":\"benign\",\"lastEvaluatedDate\":\"2013-07-12\",\"pubMedIds\":[\"23757202\"]}]"));

		}

		[Fact]
        public void Wrong1000GArbitration()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr12_17752945_17752946.nsa",
                "12	17752945	rs113134577	TTGTA	T	100	PASS	AC=1426;AF=0.284744;");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal(
                "{\"altAllele\":\"-\",\"refAllele\":\"TGTA\",\"begin\":17752946,\"chromosome\":\"12\",\"dbsnp\":[\"rs113134577\",\"rs200438154\",\"rs74874317\",\"rs780786944\"],\"end\":17752949,\"variantType\":\"deletion\",\"vid\":\"12:17752946:17752949\"}",
                altAllele);
        }

        [Fact]
        public void DuplicateClinVarJson()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr11_5247992_5247994.nsa",
                "11	5247992	rs281864900	CAAAG	C	.	.	RS=281864900;RSPOS=5247993;RV");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal(
				"{\"altAllele\":\"-\",\"refAllele\":\"AAAG\",\"begin\":5247993,\"chromosome\":\"11\",\"dbsnp\":[\"rs281864900\",\"rs80356821\"],\"end\":5247996,\"variantType\":\"deletion\",\"vid\":\"11:5247993:5247996\",\"clinVar\":[{\"id\":\"RCV000016673.25\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"AAAG\",\"altAllele\":\"-\",\"phenotype\":\"beta^0^ Thalassemia\",\"medGenId\":\"C0271980\",\"significance\":\"pathogenic\",\"snoMedCtId\":\"86715000\",\"lastEvaluatedDate\":\"2012-12-25\",\"pubMedIds\":[\"12000828\",\"12383672\",\"20301599\",\"6714226\",\"6826539\",\"9113933\",\"9845707\"]},{\"id\":\"RCV000020328.1\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"AAAG\",\"altAllele\":\"-\",\"phenotype\":\"alpha Thalassemia\",\"medGenId\":\"C0002312\",\"omimId\":\"604131\",\"orphanetId\":\"ORPHA846\",\"significance\":\"pathogenic\",\"snoMedCtId\":\"68913001\",\"lastEvaluatedDate\":\"2012-12-25\",\"pubMedIds\":[\"12000828\",\"12383672\",\"20301599\",\"6714226\",\"6826539\",\"9113933\",\"9845707\"]},{\"id\":\"RCV000016277.2\",\"reviewStatus\":\"no criteria\",\"alleleOrigin\":\"germline\",\"refAllele\":\"AAA\",\"altAllele\":\"-\",\"phenotype\":\"Hemoglobinopathy\",\"medGenId\":\"C0019045\",\"significance\":\"pathogenic\",\"snoMedCtId\":\"80141007\",\"lastEvaluatedDate\":\"2014-11-07\",\"pubMedIds\":[\"2599881\"]},{\"id\":\"RCV000016278.25\",\"reviewStatus\":\"no criteria\",\"alleleOrigin\":\"germline\",\"refAllele\":\"AAA\",\"altAllele\":\"-\",\"phenotype\":\"Heinz body anemia\",\"medGenId\":\"C0700299\",\"omimId\":\"140700\",\"orphanetId\":\"ORPHA178330\",\"significance\":\"pathogenic\",\"lastEvaluatedDate\":\"2014-11-07\",\"pubMedIds\":[\"2599881\"]}],\"oneKgAll\":0.000998,\"oneKgAfr\":0,\"oneKgAmr\":0,\"oneKgEas\":0.00496,\"oneKgEur\":0,\"oneKgSas\":0,\"oneKgAllAn\":5008,\"oneKgAfrAn\":1322,\"oneKgAmrAn\":694,\"oneKgEasAn\":1008,\"oneKgEurAn\":1006,\"oneKgSasAn\":978,\"oneKgAllAc\":5,\"oneKgAfrAc\":0,\"oneKgAmrAc\":0,\"oneKgEasAc\":5,\"oneKgEurAc\":0,\"oneKgSasAc\":0,\"exacCoverage\":51,\"exacAll\":0.000272,\"exacAfr\":0,\"exacAmr\":0,\"exacEas\":0.002201,\"exacFin\":0,\"exacNfe\":0,\"exacOth\":0.001101,\"exacSas\":0.000787,\"exacAllAn\":121370,\"exacAfrAn\":10404,\"exacAmrAn\":11562,\"exacEasAn\":8632,\"exacFinAn\":6614,\"exacNfeAn\":66738,\"exacOthAn\":908,\"exacSasAn\":16512,\"exacAllAc\":33,\"exacAfrAc\":0,\"exacAmrAc\":0,\"exacEasAc\":19,\"exacFinAc\":0,\"exacNfeAc\":0,\"exacOthAc\":1,\"exacSasAc\":13}", altAllele);
        }

        [Fact]
        public void RefMinorChr1()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_169519049_169519050.nsa",
                "1	169519049	rs6025	T	.	.	.	.");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("isReferenceMinorAllele", annotatedVariant.ToString());
        }

        [Fact]
        public void ClearingAncestralAllele()
        {
            _vcfUtilities.FieldDoesNotContain("chrX	2957282	.	A	G	100	PASS	.", "chrX_2957282_2957284.nsa", "AA=",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void ConflictingOneKgen()
        {
            _vcfUtilities.FieldEquals("22	17996285	rs35048606	A	ATCTC	100	PASS	.", "chr22_17996285_17996286.nsa",
                ".", VcfCommon.InfoIndex);
        }

        [Fact]
        public void SmallVariantPosingAsSv()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_101155257_101155258.nsa",
                "X	101155257	rs373174489	GTGCAAAAGCTCTTTAGTTTAATTAGGTCTCAGCTATTTATCTTTGTTCTTAT	G	100	PASS	AC=1723;AF=0.456424;AN=3775;NS=2504;DP=19960;AMR_AF=0.2594;AFR_AF=0.6346;EUR_AF=0.4364;SAS_AF=0.1789;EAS_AF=0.0893;END=101155309;SVTYPE=DEL;CS=DEL_pindel;VT=SV");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            //Assert.Equal(0.456424, altAllele?.AlleleFrequencies["oneKgAll"]);
			Assert.Equal("0.456424", altAllele.AlleleFrequencyAll);
        }

		//22	16069994	C	CA	
	    [Fact]
	    public void UpdateDbSnp147()
	    {
			var annotatedVariant = DataUtilities.GetVariant(null, "chr22_16069994_16069995.nsa",
				"22	16069994	.	C	CA	100	PASS	VT=SV");
			Assert.NotNull(annotatedVariant);

			var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
			Assert.NotNull(altAllele);

			//Assert.Equal(3, altAllele?.Ids["dbsnp"].Count());
			Assert.Equal(3,altAllele.DbSnpIds.Length);
		}

	    [Fact]
        public void MissingRefMinorOneKgen()
        {
            _vcfUtilities.FieldEquals("1	17639993	rs560856316	C	.	.	.	.", "chr1_17639993_17639994.nsa",
                "RefMinor;GMAF=C|0.002796", VcfCommon.InfoIndex);
        }

        [Fact]
        public void RefMinorMissingCosmic()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_144904882_144904882.nsa",
                "X	144904882	.	T	.	.	PASS	RefMinor;phyloP=-0.312	GT:GQX:DP:DPF:AD	0:509:35:2:35");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal(
				"{\"refAllele\":\"T\",\"begin\":144904882,\"chromosome\":\"X\",\"end\":144904882,\"globalMinorAllele\":\"T\",\"gmaf\":0.04185,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"X:144904882:T\",\"cosmic\":[{\"id\":\"COSM391442\",\"refAllele\":\"T\",\"altAllele\":\"-\",\"gene\":\"SLITRK2\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"lung\"}]}]}",
                altAllele);
        }

        [Fact]
        public void MissingCosmicNonCoding()
        {
            JsonUtilities.AlleleContains(
                "2	203066048	rs12693962	G	T	.	LowQscore	SOMATIC;QSS=1;TQSS=1;NT=het;QSS_NT=1;TQSS_NT=1;SGT=GT->GT;DP=150;MQ=59.50;MQ0=2;ALTPOS=50;ALTMAP=23;ReadPosRankSum=0.18;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=0.00;AA=G;AF1000G=0.235823;phyloP=2.166;CSQT=1|DAZAP2P1|ENST00000475212|downstream_gene_variant,1|SUMO1|ENST00000392246|downstream_gene_variant	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	55:1:0:0:0,0:0,0:34,35:20,20	91:1:0:0:0,0:0,0:71,72:19,19",
                "chr2_203066048_203066049.nsa",
				"{\"ancestralAllele\":\"G\",\"altAllele\":\"T\",\"refAllele\":\"G\",\"begin\":203066048,\"chromosome\":\"2\",\"dbsnp\":[\"rs12693962\"],\"end\":203066048,\"globalMinorAllele\":\"T\",\"gmaf\":0.2358,\"variantType\":\"SNV\",\"vid\":\"2:203066048:T\",\"cosmic\":[{\"id\":\"COSN166383\",\"isAlleleSpecific\":true,\"refAllele\":\"G\",\"altAllele\":\"T\"}]");
        }

        [Fact]
        public void MultiplePhylopScores()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, null,
                "1	817487	.	G	A,C	.	LowQscore	SOMATIC;QSS=49;TQSS=1;NT=het;QSS_NT=49;TQSS_NT=1;SGT=AG->CG;DP=255;MQ=36.56;MQ0=146;ALTPOS=55;ALTMAP=15;ReadPosRankSum=-0.98;SNVSB=1.73;PNOISE=0.00;PNOISE2=0.00;VQSR=0.00	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	44:2:0:0:15,71:0,22:27,50:0,0	55:10:0:0:1,19:2,30:42,60:0,0");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            Assert.NotNull(altAllele2);

            DataUtilities.SetConservationScore(altAllele, "0.394");
            DataUtilities.SetConservationScore(altAllele2, "0.394");

            Assert.Contains(
                "{\"altAllele\":\"A\",\"refAllele\":\"G\",\"begin\":817487,\"chromosome\":\"1\",\"phylopScore\":0.394,\"end\":817487,\"variantType\":\"SNV\",\"vid\":\"1:817487:A\"},{\"altAllele\":\"C\",\"refAllele\":\"G\",\"begin\":817487,\"chromosome\":\"1\",\"phylopScore\":0.394,\"end\":817487,\"variantType\":\"SNV\",\"vid\":\"1:817487:C\"}",
                annotatedVariant.ToString());
        }

        [Fact]
        public void CosmicIndel()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr4_55589767_55589770.nsa",
                "4	55589767	.	ACTTACGACAGG	AGCGTCATTGTGG	.	.	.");
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            var json = altAllele;

            Assert.Contains("\"id\":\"COSM1148\"", json);
            Assert.Contains("\"id\":\"COSM1149\"", json);
            Assert.Contains("\"id\":\"COSM1150\"", json);
            Assert.Contains("\"id\":\"COSM1151\"", json);
            Assert.Contains("\"id\":\"COSM22397\"", json);
            Assert.Contains("\"id\":\"COSM29819\"", json);
            Assert.Contains("\"id\":\"COSM30709\"", json);
            Assert.Contains("\"id\":\"COSM30711\"", json);
            Assert.Contains("\"id\":\"COSM34145\"", json);

            Assert.Equal(9, DataUtilities.GetCount(json, "\"gene\":\"KIT\""));
            Assert.Equal(9, DataUtilities.GetCount(json, "\"histology\":\"haematopoietic neoplasm\""));
            Assert.Equal(9, DataUtilities.GetCount(json, "\"primarySite\":\"haematopoietic and lymphoid tissue\""));
        }

        [Fact]
        public void CosmicAlleleSpecificIndel()
        {
            JsonUtilities.AlleleContains("chr3	10188320	.	G	A	.	.	.", "chr3_10188320_10188322.nsa",
                "\"id\":\"COSM18152\",\"isAlleleSpecific\":true,\"refAllele\":\"G\",\"altAllele\":\"A\",\"gene\":\"VHL\"");
        }

        [Fact]
        public void MissingAFinX()
        {
            _vcfUtilities.FieldEquals(
                "X	857972	.	A	C	.	LowQscore	SOMATIC;QSS=1;TQSS=2;NT=ref;QSS_NT=1;TQSS_NT=2;SGT=AC->AC;DP=143;MQ=58.73;MQ0=5;ALTPOS=6;ALTMAP=5;ReadPosRankSum=-2.92;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=1.98;phyloP=-1.187	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	58:1:0:0:57,58:0,4:0,0:0,0	74:1:0:0:69,70:4,11:0,0:0,0",
                "chrX_857972_857973.nsa",
				"SOMATIC;QSS=1;TQSS=2;NT=ref;QSS_NT=1;TQSS_NT=2;SGT=AC->AC;DP=143;MQ=58.73;MQ0=5;ALTPOS=6;ALTMAP=5;ReadPosRankSum=-2.92;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=1.98;GMAF=A|0.4323;AF1000G=0.567692",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void MissingCosmicNoCall()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_115256577_115256578.nsa",
                "1	115256577	COSM14200	A	.	.	.	GENE=NRAS;STRAND=-;CDS=c.134T>C;AA=p.V45A;CNT=1");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("isReferenceMinorAllele", annotatedVariant.ToString());
        }

        [Fact]
        public void UniqueCosmicStudy()
        {
            JsonUtilities.AlleleContains(
                "17	1183338	.	C	T	97	PASS	SNVSB=-13.0;SNVHPOL=2;CSQ=T|ENSG00000184811|ENST00000333813|Transcript|missense_variant|382|43|15|P/S|Cca/Tca||||1/3||ENST00000333813.3:c.43C>T|ENSP00000329548.3:p.Pro15Ser|ENSP00000329548|benign(0.213)|YES|tolerated(0.13)||||||TUSC5|CCDS42225.1	GT:GQ:GQX:DP:DPF:AD	0/1:96:93:17:0:7,10",
                "chr17_1183338_1183339.nsa",
				"{\"id\":\"COSM4129566\",\"isAlleleSpecific\":true,\"refAllele\":\"C\",\"altAllele\":\"T\",\"gene\":\"TUSC5\",\"studies\":[{\"id\":544,\"histology\":\"haematopoietic neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"},{\"id\":589,\"histology\":\"other\",\"primarySite\":\"thyroid\"}]}]");
        }

        [Fact]
        public void MissingCosmicStudies()
        {
            JsonUtilities.AlleleContains("19	33792363	COSM249867	CACTGGTC	C	.	.	.", "chr19_33792363_33792364.nsa",
                "{\"id\":\"COSM249867\",\"isAlleleSpecific\":true,\"refAllele\":\"ACTGGTC\",\"altAllele\":\"-\",\"gene\":\"CEBPA\",\"studies\":[{\"histology\":\"haematopoietic neoplasm\",\"primarySite\":\"haematopoietic and lymphoid tissue\"}]}");
        }

        [Fact]
        public void CosmicInsDel()
        {
            JsonUtilities.AlleleContains(
                "3	10188320	COSM18152	G	A	.	.	GENE=VHL;STRAND=+;CDS=c.463G>A;AA=p.V155M;CNT=7",
                "chr3_10188320_10188321.nsa", "\"id\":\"COSM14426\",\"refAllele\":\"GGTACTGAC\",\"altAllele\":\"A\"");
        }

        [Fact]
        public void CosmicTsvChrXname23()
        {
            JsonUtilities.AlleleContains("X	2856155	COSM5003595	C	T	.	.	GENE=ARSE", "chrX_2856155_2856156.nsa",
                "\"cosmic\":[{\"id\":\"COSM5003595\",\"isAlleleSpecific\":true,\"refAllele\":\"C\",\"altAllele\":\"T\",\"gene\":\"ARSE\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"pancreas\"}]},{\"id\":\"COSM5003596\",\"isAlleleSpecific\":true,\"refAllele\":\"C\",\"altAllele\":\"T\",\"gene\":\"ARSE_ENST00000540563\",\"studies\":[{\"histology\":\"carcinoma\",\"primarySite\":\"pancreas\"}]}]");
        }

        [Fact]
        public void MissingDbsnpId()
        {
            _vcfUtilities.FieldEquals(
                "17	3616153	rs34081014	C	G	48	PASS	SNVSB=0.0;SNVHPOL=2;AA=C;GMAF=G|0.07029;AF1000G=0.0702875;phyloP=0.444;CSQT=1|ITGAE|ENST00000263087|downstream_gene_variant;CSQR=1|ENSR00001339304|regulatory_region_variant	GT:GQ:GQX:DP:DPF:AD	0/1:47:44:6:0:2,4",
                "chr17_3616153_3616154.nsa", "rs34081014;rs71362546", VcfCommon.IdIndex);
        }

        [Fact]
        public void MissingEvs()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, Path.Combine("hg38", "chr2_242793_242794.nsa"),
                "2	242793	rs62114506	G	C	.	.	GENE=ARSE");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("evsAll", annotatedVariant.ToString());
        }

	    [Fact]
	    public void ExACShouldNotBeReportedWhenAN0()
	    {
			var annotatedVariant = DataUtilities.GetVariant(null, "chr19_3121451_3121454.nsa",
				"19	3121452	.	TA	T,TAA	.	.	.");
			Assert.NotNull(annotatedVariant);
			Assert.DoesNotContain("exacFinAn", annotatedVariant.ToString());
			Assert.DoesNotContain("exacFinAll", annotatedVariant.ToString());
		}

	    [Fact]
	    public void ExACMultipleAlleles()
	    {
			var annotatedVariant = DataUtilities.GetVariant(null, "chr1_138909_138909.nsa",
				"1	138909	.	C	T,A	.	.	.");
			Assert.NotNull(annotatedVariant);

		    Assert.Equal(2, Regex.Matches(annotatedVariant.ToString(), "exacAllAn").Count);
			Assert.DoesNotContain("exacFinAn", annotatedVariant.ToString());


		}

	    [Fact]
	    public void SaDirectoryTest()
	    {
			var saDir = new SupplementaryAnnotationDirectory(SupplementaryAnnotationCommon.DataVersion, GenomeAssembly.GRCh37);

			Assert.Equal(SupplementaryAnnotationCommon.DataVersion, saDir.DataVersion);
			Assert.Equal(GenomeAssembly.GRCh37, saDir.GenomeAssembly);
		}
    }
}
