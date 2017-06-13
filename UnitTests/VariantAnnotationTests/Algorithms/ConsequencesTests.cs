using UnitTests.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.Algorithms
{
    public sealed class ConsequenceTests
    {
        [Fact]
        public void RepeatExpansionNoConsequence()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000601841_chrX_Ensembl84"),
                "chrX	146993568	FMR1	G	<REPEAT:EXPANSION>	1.0	NoSuppReads	REPEAT_COUNT1=30,33", "ENST00000601841");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("upstream_gene_variant", string.Join("&", transcriptAllele.Consequence));
        }

        //var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("ENST00000370457_chrX_Ensembl84"), null as List<string>,
        //"chrX\t147912050\tFMR1\tG\t<STR8>\t.\tPASS\tSVTYPE=STR;END=147912110;REF=20;RL=60;RU=CGG\tGT:SO:SP:CN:CI\t1:FLANKING:10:8:5-11");
        [Fact]
        public void RepeatExpansionConsequence()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000370457_chrX_Ensembl84"),
                "chrX	147912050	FMR1	G	<STR8>	.	PASS	SVTYPE=STR;END=147912110;REF=20;RL=60;RU=CGG	GT:SO:SP:CN:CI	1:FLANKING:10:8:5-11", "ENST00000370457");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("short_tandem_repeat_contraction", string.Join("&", transcriptAllele.Consequence));
        }

		
		[Fact]
        public void CodingRegionVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000006101_chr17_Ensembl84"),
                "chr17	46115124	.	C	G	.	.	.	.", "ENST00000006101");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("coding_sequence_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MirnaCrash()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_039768_chr1_RefSeq84"),
                "chr1	17604437	rs72646786	C	T	341	PASS	SNVSB=-39.8;SNVHPOL=4;AA=C;GMAF=T|0.1591;AF1000=0.159145;phyloP=0.009;CSQT=1|PADI3|NM_016233.2|intron_variant,1|MIR3972|NR_039768.1|non_coding_exon_variant&nc_transcrpt_variant      GT:GQ:GQX:DP:DPF:AD	  0/1:294:34:87:9:41,46",
                "NR_039768");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopCodonInframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000442529_chr1_Ensembl84"),
                "1	3350369	COSM4170642	CCTCTGA	C	.	.	.", "ENST00000442529");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_lost&inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void FrameShift_Ensembl()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000355439_chr1_Ensembl84"),
                "chr1\t1669844\t.\tCCA\tC\t636.00\tPASS\t.", "ENST00000355439", "");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("frameshift_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedInframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000514944_chr17_Ensembl84"),
                "chr17\t7577534\t.\tCC\tC\t1000\tPASS\t.", "ENST00000514944", "");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("incomplete_terminal_codon_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedMissenseVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000507166_chr4_Ensembl84"),
                "chr4\t55144167\t.\tAA\tA\t1000\tPASS\t.", "ENST00000507166", "");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("frameshift_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void UnwantedNonCodingTranscriptExonVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000505014_chr17_Ensembl84"),
                "chr17\t7579590\t.\tA\tACT\t1000\tPASS\t.", "ENST00000505014", "CT");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("splice_region_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void InframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000412167_chr4_Ensembl84"),
                "chr4\t55593613\t.\tTTGAGGAG\tTT\t1000\tPASS\t.", "ENST00000412167", "");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MissingInframeDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000378404_chr1_Ensembl84"),
                "1	2938405	COSM4967507	ACCAGAAGAAGTA	ACCAGAAGTA	.	.	.", "ENST00000378404");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("inframe_deletion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void InframeInsertion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000257290_chr4_Ensembl84"),
                "chr4\t55141035\t.\tG\tGGAGAGG\t1000\tPASS\t.", "ENST00000257290", "GAGAGG");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("inframe_insertion", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopGained()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000344720_chr1_Ensembl84"),
                "1	78024350	COSM1741202	T	TAGT	.	.	.", "ENST00000344720", "AGT");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("stop_gained", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopGain2()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000375759_chr1_Ensembl84"),
                "chr1	16255007	.	C	T	265.00	PASS	SNVSB=-31.9;SNVHPOL=2;CSQ=T|ENSG00000065526|ENST00000375759|Transcript|stop_gained|2476|2272|758|R/*|Cga/Tga||CCDS164.1|ENST00000375759.3:c.2272C>T|ENSP00000364912.3:p.Arg758Ter||YES|||ENSP00000364912||11/15||SPEN|||||Low_complexity_(Seg):Seg&PROSITE_profiles:PS50323     GT:GQ:GQX:DP:DPF:AD     0/1:254:40:56:1:26,30",
                "ENST00000375759");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_gained", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void ProteinAlteringVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000412167_chr4_Ensembl84"),
                "chr4\t55589766\t.\tGACTTAC\tGTTTCGATTG\t1000\tPASS\t.", "ENST00000412167",
                "TTTCGATTG");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("protein_altering_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopCodonProteinAlteringVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000535569_chr1_Ensembl84"),
                "1	203186949	COSM4603562	C	CAGACCATGGCCCCGCCCAGTCCCT	.	.	.", "ENST00000535569",
                "AGACCATGGCCCCGCCCAGTCCCT");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_gained", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void InframeInsertionCodingSeqVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000269305_chr17_Ensembl84"),
                "chr17\t7577508\t.\tT\tTNNN\t1000\tPASS\t.", "ENST00000269305", "NNN");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("inframe_insertion&coding_sequence_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
		// current VEP84 result is incomplete_terminal_codon_variant&coding_sequence_variant 
		public void InframeInsertionTerminalVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000514944_chr17_Ensembl84"),
                "chr17\t7577536\t.\tT\tTN\t1000\tPASS\t.", "ENST00000514944", "N");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("incomplete_terminal_codon_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IncompleteTerminalCodon()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000514944_chr17_Ensembl84"),
                "chr17\t7577512\t.\tGTGTGATGATGGTGAGGATGGGCCT\tG\t1000\tPASS\t.", "ENST00000514944", "");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("incomplete_terminal_codon_variant&3_prime_UTR_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MitochondriaStopGained()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000362079_chrM_Ensembl84"), null);
            annotationSource.EnableMitochondrialAnnotation();

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                VcfUtilities.GetVcfVariant("chrM\t9378\t.\tG\tA\t3070.00\tPASS\t."));
            Assert.NotNull(annotatedVariant);

            var transcriptAllele = DataUtilities.GetTranscript(annotatedVariant, "ENST00000362079", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_gained", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void NonCodingExonAndNonCodingTranscript()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_039983_chr1_RefSeq84"),
                "chr1	136595	.	AG	A	.	.	CSQ=-|729737|NR_039983.2|Transcript|non_coding_transcript_exon_variant&non_coding_transcript_variant&feature_truncation|3651||||||3/3|||||||||||NR_039983.2:n.3651delC||YES|||", "NR_039983");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void NonCodingExonAndNonCodingTranscript2()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_024321_chr1_RefSeq84"),
                "chr1	761957	rs59038458	A	AT	.	.	CSQ=T|79854|NR_024321.1|Transcript|non_coding_transcript_exon_variant&non_coding_transcript_variant&feature_elongation|945-946||||||1/1|||||||||||NR_024321.1:n.945_946insA||YES|||", "NR_024321");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void NonCodingExonAndTranscriptVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_046018_chr1_RefSeq84"),
                "chr1	13302	rs180734498	C	T	.	.	CSQ=T|100287102|NR_046018.2|Transcript|non_coding_transcript_exon_variant&non_coding_transcript_variant|545||||||3/3|||||||||||NR_046018.2:n.545C>T||YES|||", "NR_046018");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void NotMatureMirnaVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000582732_chr1_Ensembl84"),
                "chr1\t17604437\t.\tC\tTs\t344.00\tPASS\tSNVSB=-40.5;SNVHPOL=4;CSQ=T|non_coding_transcript_exon_variant&non_coding_transcript_variant|MODIFIER|MIR3972|ENSG00000266634|Transcript|ENST00000582732|miRNA|1/1||ENST00000582732.1:n.54C>T||54|||||||1|HGNC|41876|YES|||||||||\tGT:GQ:GQX:DP:DPF:AD\t0/1:287:20:86:8:40,46", "ENST00000582732");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MatureMirnaVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000582609_chr17_Ensembl84"),
                "17\t6558768\t.\tC\tT\t99\tPASS\tSNVSB=-11.8;SNVHPOL=3;CSQ=T|ENSG00000264468|ENST00000582609|Transcript|mature_miRNA_variant|61||||||||1/1||ENST00000582609.1:n.61G>A||||YES|||||||MIR4520A|\tGT:GQ:GQX:DP:DPF:AD\t0/1:42:42:7:1:2,5",
                "ENST00000582609", "T");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("mature_miRNA_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MultiAlleleProteinAlteringVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NM_183008_chr1_RefSeq84"),
                "chr1\t26608819\t.\tAGGGACTGGGGCCGGGACCGGGACCGGGACTGGG\tAGGGACTGGG,AGGGACTGGA\t60\tPASS\tCIGAR=1M24D9M,9M25D1I;RU=.,.;REFREP=3,.;IDREP=2,.;GMAF=A|0.3934;cosmic=COSM3749046;CSQT=1|CEP85|NM_022778.3|downstream_gene_variant,2|CEP85|NM_022778.3|downstream_gene_variant,1|SH3BGRL3|NM_031286.3|downstream_gene_variant,2|SH3BGRL3|NM_031286.3|downstream_gene_variant,1|UBXN11|NM_183008.2|inframe_deletion,2|UBXN11|NM_183008.2|\tGT:GQ:GQX:DPI:AD\t1/2:143:119:23:0,3,3",
                "NM_183008", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("protein_altering_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void SpliceDonorVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000327057_chr1_Ensembl84"),
                "chr1\t54500460\t.\tG\tA\t512.0\tPASS\t.", "ENST00000327057", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("splice_donor_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void SpliceRegionAndNonCodingExonTranscript()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NR_026752_chr1_RefSeq84"),
                "chr1	16952993	rs942268	C	G	.	.	CSQ=G|84809|NR_026752.1|Transcript|splice_region_variant&non_coding_transcript_exon_variant&non_coding_transcript_variant|347||||||4/7|||||||||||NR_026752.1:n.347G>C||YES|||", "NR_026752");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("splice_region_variant&non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopRetainedVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000371614_chr1_Ensembl84"),
                "chr1\t52498370\t.\tC\tT\t136.00\tPASS\t.\tGT:GQ:GQX:DP:DPF:AD\t0/1:169:33:55:3:34,21", "ENST00000371614");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_retained_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopRetainedWithFrameSift()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000326813_chr1_Ensembl84"),
                "1	85598679	COSM3727466	T	TA	.	.	.", "ENST00000326813", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_retained_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopRetainedWithSift()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000543887_chr15_Ensembl84"),
                "chr15\t76030993\t.\tA\tG\t87.00\tPASS\t.\tGT:GQ:GQX:DP:DPF:AD\t0/1:120:31:26:11:15,11", "ENST00000543887");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("stop_retained_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MnvSynomymousVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000379410_chr1_Ensembl84"),
                "1	909352	COSM363547	CCC	CAT	.	.	", "ENST00000379410");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("synonymous_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void TranscriptAblationTest()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000356200_chr1_Ensembl84"),
                "1	1634160	MantaDEL:164:0:1:1:0:0	C	<DEL>	.	MGE10kb	END=1654290;SVTYPE=DEL;SVLEN=-65919;IMPRECISE;CIPOS=-285,285;CIEND=-205,205;SOMATIC;SOMATICSCORE=36;ColocalizedCanvas",
                "ENST00000356200");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("transcript_ablation", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void WrongIntronVariantForSmallIntrons()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000356969_chr22_Ensembl84"),
                "22	33210325	.	G	GAA	11	LowGQXHetIns	CIGAR=1M2I;RU=A;REFREP=1;IDREP=3	GT:GQ:GQX:DPI:AD	0/1:53:4:33:29,3",
                "ENST00000356969", "AA");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("splice_region_variant&non_coding_transcript_exon_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void TranscriptTruncationTest()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000517870_chr1_Ensembl84"),
                "chr1	53103983	.	G	<DEL>	.	PASS	SVTYPE=DEL;END=53104013;", "ENST00000517870");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("transcript_truncation", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void MissingSpliceRegionVariant()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000370070_chr1_Ensembl84"),
                "1	107946262	rs12126267	G	A	.	.	.", "ENST00000370070");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("missense_variant&splice_region_variant", string.Join("&", transcriptAllele.Consequence));
        }

	    [Fact]
	    public void SvInvertionUseDefaultTranscriptVariant()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000370070_chr1_Ensembl84"),
				"1	107946262	.	G	<INV>	.	.	SVTYPE=INV;END=107947000;SVLEN=1000", "ENST00000370070");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("transcript_variant", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    public void CnvTranscriptAmplification()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000579787_chr1_Ensembl84"),
				"1	723707	Canvas:GAIN:1:723708:2581225	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2", "ENST00000579787");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("transcript_amplification&copy_number_increase", string.Join("&", transcriptAllele.Consequence));

		}

	    [Fact]
	    [Trait("jira", "NIR-1787")]
	    public void NoBioTypeConsequenceForSV()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000480326_chr1_Ensembl84"),
				"chr1\t204086319\t.\tG\t<DEL>\t1\tPASS\tSVTYPE=DEL;END=204086723;ALTDEDUP=26;ALTDUP=12;REFDEDUP=49;REFDUP=15;INTERGENIC=False\t.\t.", "ENST00000480326");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("transcript_truncation", string.Join("&", transcriptAllele.Consequence));
		}

		[Fact]
		[Trait("jira", "NIR-1787")]
		public void NoBioTypeConsequenceForSV2()
		{
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000384891_chr1_Ensembl84"),
				"chr1\t209603846\t.\tG\t<DEL>\t1\tPASS\tSVTYPE=DEL;END=209605548;ALTDEDUP=26;ALTDUP=15;REFDEDUP=0;REFDUP=0;INTERGENIC=False\t.\t.", "ENST00000384891");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("transcript_truncation", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    public void CanvasCnvConsequence()
	    {
	        var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh38("ENST00000538422_chrX_Ensembl84"),
	            "chrX	114954877	Canvas:GAIN:chrX:114954878:115162839	N	<CNV>	3	q10	SVTYPE=CNV;END=115162839	RC:BC:CN	109:5:2",
	            "ENST00000538422");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("copy_number_increase", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    [Trait("jira", "NIR-1379")]
	    public void WrongInframeInsertionWhenStopGained()
	    {
	        var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh38("ENST00000620739_chr17_Ensembl84"),
	            "chr17\t7676211\t.\tC\tCATT\t0\tLowGQX;HighDepth\tCIGAR=1M3I;RU=ATT;REFREP=1;IDREP=2\tGT:GQ:GQX:DPI:AD\t0/0:7:7:3:3,0",
	            "ENST00000620739");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("stop_gained", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    [Trait("jira", "NIR-1599")]
	    public void WrongInframeInsertion()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NM_000090_chr2_RefSeq84"),
				"chr2\t189858762\t.\tAGGGCCCTCCTGGGATTAA\tACCGTCCTGGGCCCTGGTGGTATACAAACCTGGTATACCACC\t0\tPASS\tCIGAR=1M18D41I;RU=.;REFREP=.;IDREP=.\t.\t.", "NM_000090");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("splice_acceptor_variant&coding_sequence_variant", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    [Trait("jira", "NIR-1377")]
	    public void WrongInframeInsertion2()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("NM_152564_chr8_RefSeq84"), "chr8\t100779197\t.\tAGG\tAATGGAGC\t0\tPASS\tCIGAR=1M2D7I;RU=.;REFREP=.;IDREP=.\t.\t.", "NM_152564");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("splice_donor_variant&coding_sequence_variant", string.Join("&", transcriptAllele.Consequence));
		}

		[Fact]
		public void CanvasCnvLossConsequence()
		{
		    var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh38("ENST00000538422_chrX_Ensembl84"),
		        "chrX	114954877	Canvas:LOSS:chrX:114954878:115162839	N	<CNV>	3	q10	SVTYPE=CNV;END=115162839	RC:BC:CN	109:5:0",
		        "ENST00000538422");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("transcript_truncation&copy_number_decrease", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    public void CnvGainChrY()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000431238_chrY_Ensembl84"),
				"chrY	120430	.	N	<CNV>	3	q10	SVTYPE=CNV;END=121760	RC:BC:CN	109:5:2", "ENST00000431238");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("copy_number_increase", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    public void InsertionBeforeCdna()
	    {
		    var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000442529_chr1_Ensembl84"),
			    "1	2985823	.	C	CAGG	.	.	.", "ENST00000442529");
		    Assert.NotNull(transcriptAllele);
		    Assert.Equal("5_prime_UTR_variant", string.Join("&", transcriptAllele.Consequence));
	    }


	    [Theory]
		[InlineData("170819932","T","TTAG","stop_gained")]
		[InlineData("170819932", "T", "TCTT", "inframe_insertion")]
		[InlineData("170819932", "T", "TCTTTAG", "stop_gained&inframe_insertion")]
		[InlineData("170819928","A","AGTT","inframe_insertion")]
		[InlineData("170819928", "A", "ACTT", "inframe_insertion")]
		[InlineData("170819928","A","AGTG","stop_gained")]
		[InlineData("170819928", "A", "ACTG", "stop_gained&inframe_insertion")]
		[InlineData("170819928", "A", "ACTGAAA", "stop_gained&inframe_insertion")]
		public void TripletsInsertionTests(string pos, string refAllele, string altAllele, string consequence)
		{
			var vcfline = "chr5\t" + pos + "\t.\t" + refAllele + "\t" + altAllele + "\t.\t.\t.";

			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000351986_chr5_Ensembl84"),
				vcfline, "ENST00000351986");
			Assert.NotNull(transcriptAllele);
			Assert.Equal(consequence, string.Join("&", transcriptAllele.Consequence));
		}

		[Theory]
		[InlineData("78024344", "T", "TAG", "stop_gained")]
		[InlineData("78024344", "T", "TAGA", "stop_gained")]
		[InlineData("78024344", "T", "TAGT", "stop_gained")]
		[InlineData("78024344", "T", "TGTCCGT", "inframe_insertion")]
		[InlineData("78024344", "T", "TGTCCGA", "inframe_insertion")]
		[InlineData("78024344", "T", "TGTCCGAACTAGT", "stop_gained&inframe_insertion")]
		[InlineData("78024344", "T", "TGTCCGAACTAGA", "stop_gained&inframe_insertion")]
		[InlineData("78024346", "T","TTAG", "stop_gained")]
		[InlineData("78024346", "T", "TCATTAG", "stop_gained&inframe_insertion")]
		[InlineData("78024346", "T", "TCATCGTTAG", "stop_gained&inframe_insertion")]
		[InlineData("78024346", "T", "TCATCGTAGTTAG", "stop_gained&inframe_insertion")]
		[InlineData("78024352", "C", "CTAG", "stop_retained_variant")]
		[InlineData("78024352", "C", "CAAC", "inframe_insertion")]
		[InlineData("78024353", "T", "TA", "stop_retained_variant")]
		[InlineData("78024353", "T", "TAA", "stop_retained_variant")]
		[InlineData("78024353", "T", "TAAT", "stop_retained_variant")]
		[InlineData("78024353", "T", "TAAG", "stop_retained_variant")]
		public void InsertionConsequences(string pos, string refAllele, string altAllele, string consequence)
	    {
			var vcfline = "chr1\t" + pos + "\t.\t" + refAllele + "\t" + altAllele + "\t.\t.\t.";

			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000344720_chr1_Ensembl84"),
				vcfline, "ENST00000344720");
			Assert.NotNull(transcriptAllele);
			Assert.Equal(consequence, string.Join("&", transcriptAllele.Consequence));
		}

		[Fact]
	    public void IndelAsInframeInsertion()
	    {
		    var vcfline = "chr2\t169510236\t.\tCT\tCACTC\t.\t.\t.";
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh38("ENST00000284669_chr2_Ensembl84"), vcfline, "ENST00000284669");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("protein_altering_variant", string.Join("&", transcriptAllele.Consequence));

		}

	    [Fact]
	    public void StopGainedIndel()
	    {
			var vcfline = "chr4\t55359409\t.\tTCAA\tTTGAGTAAGGC\t.\t.\t.";
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh38("ENST00000505210_chr4_Ensembl84"), vcfline, "ENST00000505210");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("stop_gained", string.Join("&", transcriptAllele.Consequence));
		}

	    [Fact]
	    [Trait("jira", "NIR-1842")]
	    public void InsertionFeatureElongation()
	    {
			var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000503000_chr4_Ensembl84"),
				 "4	2332672	MantaINS:539825:0:0:0:10:0	C	<INS>	.	MinSomaticScore	END=2332672;SVTYPE=INS;CIPOS=0,17;CIEND=0,17;HOMLEN=17;HOMSEQ=AGGTGGATGGGTGAGGG;LEFT_SVINSSEQ=AGGTGGATGGGTGAGGGGGCAGTGTGGGGAGAGCATGGGCAGGCTGGCTTATGTCCAC;RIGHT_SVINSSEQ=ACACAGGGGGGCAAAGGGAAATATCTGCGTGGGGTTTCTGGGTGGTTCTGGTGAAAGGGTTCCAAGAAAG;SOMATIC;SOMATICSCORE=10	PR:SR	0,0:15,0	7,0:89,5", "ENST00000503000");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("feature_elongation", string.Join("&", transcriptAllele.Consequence));
		}

		[Fact]
		[Trait("jira", "NIR-1346")]
		public void InsertionInStartCodon()
		{
		    var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000449250_chr17_Ensembl84"),
		        "chr17\t65989261\t.\tA\tATCT\t.\t.\t.", "ENST00000449250", "TCT", "");
			Assert.NotNull(transcriptAllele);
			Assert.Equal("transcript_variant", string.Join("&", transcriptAllele.Consequence));
		}
	}
}