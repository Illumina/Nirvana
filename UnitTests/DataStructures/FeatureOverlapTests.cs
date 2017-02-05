using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class FeatureOverlapTests
    {
        [Fact]
        public void NonOverlappingTranscript()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000577711_chr17_Ensembl84"),
                "17\t73406762\t.\tT\tTTC\t356\tLowGQX\tCIGAR=1M2I;RU=TC;REFREP=10;IDREP=11;CSQ=TC|ENSG00000264511|ENST00000578455|Transcript|downstream_gene_variant||||||||4519|||||||YES|||||||MIR3678|,TC|ENSG00000177885|ENST00000316615|Transcript|upstream_gene_variant||||||||4972|||||ENSP00000317360|||||||||GRB2|CCDS11722.1,TC|ENSG00000223217|ENST00000411285|Transcript|upstream_gene_variant||||||||1493|||||||YES||||||||\tGT:GQ:GQX:DPI:AD\t1/1:23:20:14:0,6",
                "ENST00000577711");
            Assert.Null(transcriptAllele);
        }

        [Fact]
        public void MissingRegulatoryAnnotation()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001519570_chr1_Ensembl84_reg"),
                "1	42103988	rs370323701	GGAAAGAAAGAAAGAAAGGGAGAGAGAGAGAAAGAAAGAAAAAG	GGAAAGAAAGAAAGGGAGAGAGAGAGAAAGAAAGAAAAAG,GGAAA	398	LowGQXHetAltDel	CIGAR=1M4D39M,5M39D;RU=GAAA,.;REFREP=4,1;IDREP=3,0;CSQT=1|HIVEP3|ENST00000372583|intron_variant,2|HIVEP3|ENST00000372583|intron_variant;CSQR=2|ENSR00001519570|regulatory_region_variant GT:GQ:GQX:DPI:AD 1/2:533:7:29:5,12,10",
                "ENSR00001519570");
            Assert.NotNull(regulatoryRegion);
        }

        [Fact]
        public void MissingSvTranscript()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000422725_chr1_Ensembl84"),
                "1	1530659	MantaDEL:116:0:1:0:4:0	AAACAGAGACAGAGACAGAGAGGCAGACAGAGAGAGAGACAGACAGAGAGCAGAACAGGGAGAGACAAAGAGACAGAGAGAGAGAGAGACACAGAGAGAGAGAGATAGAGAGAGGCAGACAGAGACAGAGAGACAGACAGACACAGAGCAGAACAGGGAGAGACAGAGAGAGAGAGACAGAGAGAGGCAGACAGAGAGAGAGAGAGACAGAC	AGAG	.	MinSomaticScore	END=1530870;SVTYPE=DEL;SVLEN=-211;CIGAR=1M3I211D;SOMATIC;SOMATICSCORE=28;CSQ=deletion|downstream_gene_variant|MODIFIER|C1orf233|ENSG00000228594|Transcript|ENST00000422725|protein_coding|||||||||||2522|-1|HGNC|42951|YES|CCDS55559.1|ENSP00000389111||||||| PR:SR 13,0:24,1 21,0:32,4",
                "ENST00000422725");
            Assert.NotNull(transcriptAllele);
        }

        [Fact]
        public void FeatureOverlapDeletion()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00000539447_chr1_Ensembl84_reg"),
                "1\t79156589\t.\tCT\tC\t.\tRepeat;iHpol;QSI_ref    SOMATIC;QSI=6;TQSI=2;NT=ref;QSI_NT=6;TQSI_NT=2;SGT=ref->het;RU=T;RC=15;IC=14;IHP=18;CSQ=-|downstream_gene_variant|MODIFIER|AC104837.1|ENSG00000221683|Transcript|ENST00000408756|miRNA|||||||||||3745|1|Clone_based_ensembl_gene||YES|||||||||\tDP:DP2:TAR:TIR:TOR:DP50:FDP50:SUBDP50\t37:37:22,26:0,0:16,14:38.28:4.63:0.00   104:104:70,77:3,4:32,26:106.24:10.80:0.00",
                "ENSR00000539447");
            Assert.Null(regulatoryRegion);
        }

        [Fact]
        public void InsertionAtFeatureStart()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001625040_chr11_Ensembl84_reg"),
                "11\t121973583\t.\tC\tCA\t.\tRepeat;QSI_ref  SOMATIC;QSI=29;TQSI=1;NT=hom;QSI_NT=29;TQSI_NT=1;SGT=hom->het;RU=A;RC=9;IC=10;IHP=10     DP:DP2:TAR:TIR:TOR:DP50:FDP50:SUBDP50\t52:52:0,0:49,49:3,3:54.96:0.00:0.00\t131:131:34,34:77,77:19,19:130.89:1.24:0.00",
                "ENSR00001625040");
            Assert.Null(regulatoryRegion);
        }

        [Fact]
        public void VepMissingRegulatoryFeature()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00000554042_chr1_Ensembl84_reg"),
                "1	225615945	.	T	C	.	PASS	SOMATIC;QSS=78;TQSS=1;NT=ref;QSS_NT=78;TQSS_NT=1;SGT=TT->CT;DP=159;MQ=60.00;MQ0=0;ALTPOS=44;ALTMAP=26;ReadPosRankSum=-0.92;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=9.59	DP:FDP:SDP:SUBDP:AU:CU:GU:TU	48:0:0:0:0,0:1,1:1,1:46,46	111:6:0:0:0,0:29,33:1,2:75,76",
                "ENSR00000554042");
            Assert.NotNull(regulatoryRegion);
        }

        [Fact]
        public void OverlapFraction()
        {
            var interval = new AnnotationInterval(101, 110);

            var overlap = interval.OverlapFraction(99, 110);
            Assert.Equal(1.0, overlap);// full overlap

            overlap = interval.OverlapFraction(90, 105);
            Assert.Equal(0.50, overlap);

            overlap = interval.OverlapFraction(105, 111);
            Assert.Equal(0.60, overlap);

            overlap = interval.OverlapFraction(101, 101);
            Assert.Equal(0.10, overlap);
        }

        [Fact]
        public void CnvOverlspTest()
        {
            var interval = new AnnotationInterval(16764333, 17483981);

            var overlap = interval.OverlapFraction(16886175, 17054465);
            Assert.Equal(0.23385150260752116, overlap);// full overlap

            var variant = new AnnotationInterval(16886175, 17054465);
            var variantOverlap = variant.OverlapFraction(16764333, 17483981);

            Assert.Equal(1.0, variantOverlap);
        }

        [Fact]
        [Trait("jira", "NIR-2119")]
        public void RegulatoryAnnotation1()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001519570_chr1_Ensembl84_reg"),
                "chr1	42104000	.	A	<DEL>	.	.	SVTYPE=DEL;END=42105000",
                "ENSR00001519570");
            Assert.NotNull(regulatoryRegion);
        }

        [Fact]
        [Trait("jira", "NIR-2119")]
        public void RegulatoryAnnotation2()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001519570_chr1_Ensembl84_reg"),
                "chr1	42104000	.	A	<DEL>	.	.	SVTYPE=DEL;END=42154000",
                "ENSR00001519570");
            Assert.NotNull(regulatoryRegion);
        }

        [Fact]
        [Trait("jira", "NIR-2119")]
        public void DisableRegulatoryForlargeSV1()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001519570_chr1_Ensembl84_reg"),
                "chr1	42104000	.	A	<DEL>	.	.	SVTYPE=DEL;END=42154001",
                "ENSR00001519570");
            Assert.Null(regulatoryRegion);
        }

        [Fact]
        [Trait("jira", "NIR-2119")]
        public void DisableRegulatoryForlargeSV2()
        {
            var regulatoryRegion = DataUtilities.GetRegulatoryRegion(Resources.CacheGRCh37("ENSR00001519570_chr1_Ensembl84_reg"),
                "chr1	42104000	.	A	<DEL>	.	.	SVTYPE=DEL;END=42174000",
                "ENSR00001519570");
            Assert.Null(regulatoryRegion);
        }
    }
}
