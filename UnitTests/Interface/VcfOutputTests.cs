using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.Interface
{
    [Collection("Chromosome 1 collection")]

    public sealed class VcfOutputTests
    {
        #region members
        
        private readonly VcfUtilities _vcfUtilities = new VcfUtilities();
        
        #endregion

        [Fact]
        public void AlleleSpecificPhylop()
        {
            const string vcfLine = "1	4004037	.	CG	CA,TG	0	LowDP;LowGQ	DP=0	GT:GQ:AD:VF:NL:SB:GQX	./.:0:0:0.000:20:-100.0000:0";
            var annotatedVariant = DataUtilities.GetVariant(null as string, vcfLine);
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(0);
            DataUtilities.SetConservationScore(altAllele, "-0.344");

            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            DataUtilities.SetConservationScore(altAllele2, "0.135");

            var observedVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);

            Assert.Contains("phyloP=-0.344,0.135", observedVcfLine);
		}

        [Fact]
        public void MissingRefBlockEntry()
        {
            var expectedVcfLine = "1	10005	.	C	.	.	LowGQX	END=10034;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:3:1:0";
            var annotatedVariant = DataUtilities.GetVariant(null as string, expectedVcfLine);

            var observedVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(expectedVcfLine, annotatedVariant);
            Assert.Equal(expectedVcfLine, observedVcfLine);
        }

        [Fact]
        public void AlleleMissingPhylop()
        {
            const string vcfLine = "1	4004037	.	CG	CA,TG	0	LowDP;LowGQ	DP=0	GT:GQ:AD:VF:NL:SB:GQX	./.:0:0:0.000:20:-100.0000:0";
            var annotatedVariant = DataUtilities.GetVariant(null as string, vcfLine);
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            DataUtilities.SetConservationScore(altAllele, "-0.344");

            var observedVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);

            Assert.Contains("phyloP=-0.344,.", observedVcfLine);
        }

        [Fact]
        public void CosmicMultiDelete()
        {
            _vcfUtilities.FieldEquals(
                "17	21319650	.	CGAG	C	101	PASS	CIGAR=1M3D;RU=GAG;REFREP=2;IDREP=1	GT:GQ:GQX:DPI:AD	0/1:141:101:29:22,4",
                "chr17_21319650_21319651.nsa", "CIGAR=1M3D;RU=GAG;REFREP=2;IDREP=1;cosmic=1|COSM278475",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void RefAlleleTrimmingCosmic()
        {
            _vcfUtilities.FieldEquals(
                "17	38858134	.	CA	C	233	LowGQX	CIGAR=1M1D;RU=A;REFREP=1;IDREP=0	GT:GQ:GQX:DPI:AD	1/1:15:12:6:0,5",
                "chr17_38858134_38858135.nsa",
                "CIGAR=1M1D;RU=A;REFREP=1;IDREP=0;AA=A;AF1000G=0.464856;EVS=0.368669|79|6259;cosmic=1|COSM1684505",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void EvsWrongAltValue()
        {
            _vcfUtilities.FieldEquals(
                "17	641336	rs60947910	C	T	9	LowGQX	SNVSB=0.0;SNVHPOL=19;AA=C;GMAF=T|0.1835;AF1000G=0.183506;EVS=|22|6254;phyloP=-1.271	GT:GQ:GQX:DP:DPF:AD	0/1:17:9:3:2:1,2",
                "chr17_641334_641337.nsa",
                "SNVSB=0.0;SNVHPOL=19;AA=C;GMAF=T|0.1835;AF1000G=0.183506;cosmic=1|COSN6415581", VcfCommon.InfoIndex);
        }

        [Fact]
        public void MissingEvsDot()
        {
            _vcfUtilities.FieldEquals(
                "1	116609125	rs548438731;rs3833541	CTC	CC,CT	531	PASS	CIGAR=1M1D1M,2M1D;RU=T,C;REFREP=1,4;IDREP=0,3;AA=T,.;GMAF=C|0.03674,.;AF1000G=0.0367412,.;EVS=0.624324|44|5731;CSQT=1|SLC22A15|ENST00000369503|intron_variant&feature_truncation,2|SLC22A15|ENST00000369503|intron_variant&feature_truncation	GT:GQ:GQX:DPI:AD	1/2:574:9:58:0,13,26",
                "chr1_116609125_116609128.nsa",
                "CIGAR=1M1D1M,2M1D;RU=T,C;REFREP=1,4;IDREP=0,3;AA=T,.;AF1000G=0.036741,.;EVS=.,0.624324|44|5731",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void MissingEvsValue()
        {
            _vcfUtilities.FieldEquals(
                "1	226259211	rs375001380;rs397983063	TCA	TA,TC	32	LowGQXHetAltDel	CIGAR=1M1D1M,2M1D;RU=C,A;REFREP=1,17;IDREP=0,16;EVS=|6|5096;CSQT=1|H3F3A|ENST00000366813|3_prime_UTR_variant&feature_truncation,2|H3F3A|ENST00000366813|3_prime_UTR_variant&feature_truncation	GT:GQ:GQX:DPI:AD	1/2:162:2:22:4,8,1",
                "chr1_226259211_226259213.nsa", "CIGAR=1M1D1M,2M1D;RU=C,A;REFREP=1,17;IDREP=0,16", VcfCommon.InfoIndex);
        }

        [Fact]
        public void MultiDbSnpOutput()
        {
            _vcfUtilities.FieldEquals(
                "17	186913	rs34543275,rs11453667	A	AT	111	LowGQX	CIGAR=1M1I;RU=T;REFREP=11;IDREP=12;GMAF=AC|0.002995;AF1000G=0.748003;CSQT=1|RPH3AL|ENST00000331302|intron_variant&feature_elongation,1||ENST00000575743|downstream_gene_variant	GT:GQ:GQX:DPI:AD	1/1:21:18:9:0,7",
                "chr17_186913_186914.nsa", "rs11453667;rs34543275", VcfCommon.IdIndex);
        }

        [Fact]
        public void MissingRsid()
        {
            _vcfUtilities.FieldEquals(
                "chr1	129010	rs377161483	AATG	A	32	LowGQXHetAltDel	CIGAR=1M1D1M,2M1D;RU=C,A;REFREP=1,17;IDREP=0,16	GT:GQ:GQX:DPI:AD	1/2:162:2:22:4,8,1",
                "chr1_129010_129012.nsa", "rs377161483", VcfCommon.IdIndex);
        }

        [Fact]
        public void DbSnpIds()
        {
            _vcfUtilities.FieldEquals(
                "1	1594584	MantaDEL:164:0:1:1:0:0;rs123	C	<DEL>	.	MGE10kb END=1660503;SVTYPE=DEL;SVLEN=-65919;IMPRECISE;CIPOS=-285,285;CIEND=-205,205;SOMATIC;SOMATICSCORE=36;Colocaliz	edCanvas	PR	42,0	226,9",
                "chr1_129010_129012.nsa", "MantaDEL:164:0:1:1:0:0", VcfCommon.IdIndex);
        }

        [Fact]
        public void OneKfreqMultiAllele()
        {
            _vcfUtilities.FieldEquals(
                "chr1	825069	rs4475692	G	A,C	362.00	LowGQX;HighDPFRatio	SNVSB=-36.9;SNVHPOL=3	GT:GQ:GQX:DP:DPF:AD	1/2:4:0:52:38:8,11,33",
                "chr1_825069_825070.nsa",
                "SNVSB=-36.9;SNVHPOL=3;AA=.,g;GMAF=G|0.3227,G|0.3227;AF1000G=.,0.677316;cosmic=1|COSN16256566,2|COSN16256389",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void NoRefMinorForDeletion()
        {
            _vcfUtilities.FieldEquals(
                "17	77263	.	TG	T	428	PASS	CIGAR=1M1D;RU=G;REFREP=4;IDREP=3	GT:GQ:GQX:DPI:AD	1/1:33:30:12:0,11",
                "chr17_77263_77265.nsa", "CIGAR=1M1D;RU=G;REFREP=4;IDREP=3;AA=GGG;AF1000G=1", VcfCommon.InfoIndex);
        }

        [Fact]
        public void GatkGenomeVcf()
        {
            _vcfUtilities.FieldEquals(
                "1	30923	rs140337953	G	T,<NON_REF>	264.77	PASS	BaseQRankSum=0.259;DB;DP=26;MLEAC=1,0;MLEAF=0.500,0.00;MQ=43.87;MQ0=0;MQRankSum=-0.830;ReadPosRankSum=-0.156	GT:AD:GQ:PL:SB	0/1:15,11,20:99:293,0,330,337,363,700:8,7,3,8",
                null, "T,<NON_REF>", VcfCommon.AltIndex);
        }

        [Fact]
        public void EmptyInputInfo()
        {
            _vcfUtilities.FieldEquals("17	77263	.	TG	T	428	PASS	.	GT:GQ:GQX:DPI:AD	1/1:33:30:12:0,11", "chr17_77263_77265.nsa",
                "AA=GGG;AF1000G=1", VcfCommon.InfoIndex);
        }
    }
}