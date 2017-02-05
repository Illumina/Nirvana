using System.Linq;
using System.Text.RegularExpressions;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.InterfaceTests
{
    public sealed class BasicAnnotationTests
    {
        [Fact]
        public void RepeatExpansionNoConsequence()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000601841_chrX_Ensembl84"), "chrX	146993568	FMR1	G	<REPEAT:EXPANSION>	1.0	NoSuppReads	REPEAT_COUNT1=30,33");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);

            var transcript = altAllele.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcript);

            // ReSharper disable once PossibleNullReferenceException
            var observedConsequence = string.Join("&", transcript.Consequence);
            const string expectedConsequence = "upstream_gene_variant";
            Assert.Equal(expectedConsequence, observedConsequence);
        }

        [Fact]
        public void MultiAlleleProteinAlteringVariant()
        {
            // GGGACTGGA|protein_altering_variant|MODIFIER||91544|Transcript|NM_183008.2|protein_coding|16/16||NM_183008.2:c.1501_1533delinsTCCAGTCCC|NP_892120.2:p.Pro501_Pro509delinsSer|1774-1806|1501-1533|501-511|PSPGPGPGPSP/SSP|CCCAGTCCCGGTCCCGGTCCCGGCCCCAGTCCC/TCCAGTCCC|||-1|||YES||NP_892120.2|rseq_mrna_match&rseq_ens_match_cds||||||||
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("NM_183008_chr1_RefSeq84"),
                "chr1\t26608819\t.\tAGGGACTGGGGCCGGGACCGGGACCGGGACTGGG\tAGGGACTGGG,AGGGACTGGA\t60\tPASS\tCIGAR=1M24D9M,9M25D1I;RU=.,.;REFREP=3,.;IDREP=2,.;GMAF=A|0.3934;cosmic=COSM3749046;CSQT=1|CEP85|NM_022778.3|downstream_gene_variant,2|CEP85|NM_022778.3|downstream_gene_variant,1|SH3BGRL3|NM_031286.3|downstream_gene_variant,2|SH3BGRL3|NM_031286.3|downstream_gene_variant,1|UBXN11|NM_183008.2|inframe_deletion,2|UBXN11|NM_183008.2|\tGT:GQ:GQX:DPI:AD\t1/2:143:119:23:0,3,3");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckRefSeqTranscriptCount(1, altAllele);

            var transcript = altAllele.RefSeqTranscripts.FirstOrDefault();
            Assert.NotNull(transcript);

            // ReSharper disable once PossibleNullReferenceException
            var observedConsequence = string.Join("&", transcript.Consequence);
            const string expectedConsequence = "protein_altering_variant";
            Assert.Equal(expectedConsequence, observedConsequence);
        }

        [Fact]
        public void SpliceRegionAndNonCodingExonTranscript()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("NR_026752_chr1_RefSeq84"),
                "chr1	16952993	rs942268	C	G	.	.	CSQ=G|84809|NR_026752.1|Transcript|splice_region_variant&non_coding_transcript_exon_variant&non_coding_transcript_variant|347||||||4/7|||||||||||NR_026752.1:n.347G>C||YES|||");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckRefSeqTranscriptCount(1, altAllele);

            var transcript = altAllele.RefSeqTranscripts.FirstOrDefault();
            Assert.NotNull(transcript);

            // ReSharper disable once PossibleNullReferenceException
            var observedConsequence = string.Join("&", transcript.Consequence);
            const string expectedConsequence = "splice_region_variant&non_coding_transcript_exon_variant&non_coding_transcript_variant";
            Assert.Equal(expectedConsequence, observedConsequence);
        }

        [Fact]
        public void InsertionAtRegFeatureStart()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENSR00001625040_chr11_Ensembl84_reg"),
                "11\t121973583\t.\tC\tCA\t.\tRepeat;QSI_ref  SOMATIC;QSI=29;TQSI=1;NT=hom;QSI_NT=29;TQSI_NT=1;SGT=hom->het;RU=A;RC=9;IC=10;IHP=10     DP:DP2:TAR:TIR:TOR:DP50:FDP50:SUBDP50\t52:52:0,0:49,49:3,3:54.96:0.00:0.00\t131:131:34,34:77,77:19,19:130.89:1.24:0.00");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(1, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckRegulatoryCount(0, altAllele);
        }

        [Fact]
        public void VepMissingRegulatoryFeature()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENSR00000554042_chr1_Ensembl84_reg"),
                "1	225615945	.	T	C	.	PASS	SOMATIC;QSS=78;TQSS=1;NT=ref;QSS_NT=78;TQSS_NT=1;SGT=TT->CT;DP=159;MQ=60.00;MQ0=0;ALTPOS=44;ALTMAP=26;ReadPosRankSum=-0.92;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=9.59	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	48:0:0:0:0,0:1,1:1,1:46,46	111:6:0:0:0,0:29,33:1,2:75,76");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(1, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckRegulatoryCount(1, altAllele);

            var regulatoryRegion = altAllele.RegulatoryRegions.FirstOrDefault();
            Assert.NotNull(regulatoryRegion);
            Assert.Equal("ENSR00000554042", regulatoryRegion.ID);
        }

        // TODO: need more information for this test. it need mutiple genes
        [Fact]
        public void DuplicatedTranscripts()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000483270_chr1_Ensembl84"),
                "1	15910881	MantaBND:17185:0:1:0:9:0:0	G	[2:86827867[G	.	MinSomaticScore SVTYPE=BND;MATEID=MantaBND:17185:0:1:0:9:0:1;CIPOS=0,7;HOMLEN=7;HOMSEQ=TGATCCG;SOMATIC;SOMATICSCORE=19;BND_DEPTH=26;MATE_BND_DEPTH=21 PR:SR	30,0:20,0	64,2:173,8");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(1, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);
        }

        [Fact]
        public void GatkGenomeVcf()
        {
            const bool isGatkGenomeVcf = true;
            var vcfVariant = VcfUtilities.GetVcfVariant("1	10360	.	C	<NON_REF>	.	PASS	END=10362	GT:DP:GQ:MIN_DP:PL	0/0:198:99:196:0,120,1800", isGatkGenomeVcf);
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000483270_chr1_Ensembl84"), vcfVariant);

            Assert.NotNull(annotatedVariant);
            AssertUtilities.CheckAlleleCount(0, annotatedVariant);
        }

        [Fact]
        public void IntronNumbers()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000380060_chrX_Ensembl84"),
                "chrX	17705850	rs397759640;rs5901624	C	CT	1815.96	PASS	.	.	.");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(1, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);

            var transcript = altAllele.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcript);

            Assert.Equal("1/7", transcript.Introns);

            AssertUtilities.CheckJsonContains("\"introns\":\"1/7\"", annotatedVariant);
        }

        [Fact]
        public void DuplicatedFilterKey()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000601841_chrX_Ensembl84"),
                "chrX	2728221	rs6567647	A	C	1865.06	PASS	AC=8;AF=1.00;AN=8;DB;DP=85;Dels=0.00;FS=0.000;HaplotypeScore=1.1721;MLEAC=8;MLEAF=1.00;MQ=69.04;MQ0=0;QD=21.94;SB=-8.869e+02;VQSLOD=5.0286;culprit=FS;PLF;BGL_PR	GT:AD:DP:GQ:PL:AA	1:0,12:12:12:167,12,0:P1,.	1|1:0,19:19:27:368,27,0:M1,M2	1|1:0,23:23:39:523,39,0:M1,P1	1|1:0,31:31:63:846,63,0:M2,P1");
            Assert.NotNull(annotatedVariant);

            var json = annotatedVariant.ToString();

            var observedCount = Regex.Matches(json, "filters").Count;
            const int expectedCount = 1;
            Assert.Equal(expectedCount, observedCount);
        }

        [Fact]
        public void IndelVariantType()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, null,
                "chr1	23259022	MantaDEL:58:0:1:0:0:0	GAGGATCACTTGAGCCCAGGAGTTCCAGACCAGCCCGGGCAACATGGTGAAACCCCACCTCTACAAAAAATACAAAAGTTACCCAGGCATGGTGGCACATGCCTATAGTCCCAGCTGCTGGGAGGGTTGAGGTGGGAGGATCACTTGAGCCAGGGAGGTGGAGACTGCAGTGAGCCATGATCACACCACTGCATTCAAGCCTAGGCTGCAACCTCGAGATTTTTTTTTTTTTTGAGATCCTGTCTCAAAAAAAATTTTTTTTGGCCAGGTGCGGTGGCTCACGCCTGTAATCCCAGCACTTTGGGAGGCTGAGGCGGGC	GT	95	PASS	END=23259340;SVTYPE=DEL;SVLEN=-318;CIGAR=1M1I318D	GT:FT:GQ:PL:PR:SR	0/0:PASS:50:0,1,2:1,0:0,0	1/1:MinGQ:7:147,9,0:0,0:1,3	0/0:PASS:53:0,3,59:0,0:1,0");
            Assert.NotNull(annotatedVariant);
            AssertUtilities.CheckJsonContains("\"variantType\":\"indel\"", annotatedVariant);
        }

        [Fact]
        public void BreakEnd()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, null,
                "chr2	13394888	MantaBND:543:0:1:0:0:0:0	G	G]chr15:20404341]	114	MaxDepth	SVTYPE=BND;MATEID=MantaBND:543:0:1:0:0:0:1;CIPOS=0,3;HOMLEN=3;HOMSEQ=AGT;BND_DEPTH=10;MATE_BND_DEPTH=7	GT:FT:GQ:PL:PR:SR	0/1:PASS:83:133,0,93:2,0:1,3	0/1:PASS:30:80,0,68:3,2:0,3	0/0:MinGQ:11:40,0,153:2,0:2,1");
            Assert.NotNull(annotatedVariant);
            AssertUtilities.CheckJsonContains("\"altAlleles\":[\"G]chr15:20404341]\"]", annotatedVariant);
        }

        [Fact]
        public void BadVcfs()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, null, "chr2	13394888	.	g	a	.	.	.");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("A", annotatedVariant.AlternateAlleles.First());
            Assert.Contains("G", annotatedVariant.ReferenceAllele);
        }

        [Fact]
        public void DisableMitochondrialAnnotation()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000387314_chrM_Ensembl84"), null,
                "MT	589	.	C	A	.	PASS	.	.	.");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(1, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(0, altAllele);
            Assert.DoesNotContain("ENST00000387314", altAllele.ToString());
        }

        [Fact]
        public void EnableMitochondrialAnnotation()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000387314_chrM_Ensembl84"), null);
            annotationSource.EnableMitochondrialAnnotation();

            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "MT	589	.	C	A	.	PASS	.	.	.");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(1, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);
            Assert.Contains("ENST00000387314", altAllele.ToString());
        }

		[Fact]
		public void CdnaPositionShouldNotBeReported()
		{
			var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("NM_005101_chr1_RefSeq84"), null,
				"1	948846	.	T	TA	.	.	.");
			Assert.NotNull(annotatedVariant);

			AssertUtilities.CheckAlleleCount(1, annotatedVariant);

			var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
			Assert.NotNull(altAllele);

			AssertUtilities.CheckRefSeqTranscriptCount(1, altAllele);
			Assert.Null(altAllele.RefSeqTranscripts.First().ComplementaryDnaPosition);
		}

	    [Fact]
		[Trait("jira","NIR-1837")]
	    public void FuzzyInsertion()
	    {
			var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh38("ENST00000471857_chr2_Ensembl84"), null,
				"chr2\t90221381\tMantaINS:49750:2:2:0:6:0\tC\t<INS>\t282\tPASS\tEND=90221390;SVTYPE=INS;LEFT_SVINSSEQ=CCGTGGCCACTCAGTTTTAGCGTCTCTGCTCTATTTGGACATTTTGCAGTTCT;RIGHT_SVINSSEQ=GATGTTGCAACTTATTACGGTCAACGGACTTACAATGCCCCTGA;DQ=0\tGT:FT:GQ:PL:PR:SR\t0/1:PASS:107:157,0,975:1,0:19,7\t0/1:PASS:175:225,0,730:1,0:14,7\t0/0:PASS:112:0,62,899:1,0:14,0");
			Assert.NotNull(annotatedVariant);

			AssertUtilities.CheckAlleleCount(1, annotatedVariant);

			var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
			Assert.NotNull(altAllele);

			AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);
		}
	}
}
