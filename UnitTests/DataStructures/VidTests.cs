using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class VidTests
    {
        [Fact]
        public void Alu()
        {
            const string vcfLine = "1	645710	ALU_umary_ALU_2	A	<INS:ME:ALU>	100	PASS	AC=35;AF=0.00698882;AN=5008;CS=ALU_umary;MEINFO=AluYa4_5,1,223,-;NS=2504;SVLEN=222;SVTYPE=ALU;TSD=null;DP=12290;EAS_AF=0.0069;AMR_AF=0.0072;AFR_AF=0;EUR_AF=0.0189;SAS_AF=0.0041";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:645711:645932:MEI";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void CopyNumberVariation()
        {
            const string vcfLine = "1	9319324	Canvas:GAIN:1:9319325:9404899	N	<CNV>	36	PASS	SVTYPE=CNV;END=9404899	RC:BC:CN:MCC	.	144:104:6:4";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:9319325:9404899:6";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void BreakendVid1()
        {
            // NIR-941
            const string vcfLine = "chr1	797265	MantaBND:10:0:1:0:2:0:0	G	G]chr8:245687]	55	PASS	SVTYPE=BND;MATEID=MantaBND:10:0:1:0:2:0:1;CIPOS=0,31;HOMLEN=31;HOMSEQ=ATTGATAGATGATAGGTAGATAGTAGATAGA;BND_DEPTH=59;MATE_BND_DEPTH=41	   GT:GQ:PR:SR	0/1:55:39,6:20,3";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            // the naming convention for breakends is different
            Assert.Equal("1:797265:-:8:245687:-", variant.AlternateAlleles[0].VariantId);
        }

        //
        [Fact]
        public void BreakendVid2()
        {
            // NIR-941
            const string vcfLine = "chr1	9121449	MantaBND:542:0:2:0:0:0:0	C	[chr14:93712486[C	518	PASS	SVTYPE=BND;MATEID=MantaBND:542:0:2:0:0:0:1;CIPOS=0,4;HOMLEN=4;HOMSEQ=CCTG;BND_DEPTH=49;MATE_BND_DEPTH=48	GT:GQ:PR:SR	0/1:518:33,2:32,15";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            // the naming convention for breakends is different
            Assert.Equal("1:9121449:+:14:93712486:+", variant.AlternateAlleles[0].VariantId);
        }

        [Fact]
        public void SvVidChrName()
        {
            // NIR-941
            const string vcfLine = "chr1	814866	Canvas:GAIN:chr1:814867:824517	N	<CNV>	4	q10;CLT10kb	SVTYPE=CNV;END=824517;CSQ=CNV|upstream_gene_variant|MODIFIER|FAM41C|284593|Transcript|NR_027055.1|misc_RNA|||||||||||2685|-1|||YES|||rseq_mrna_match&rseq_ens_no_match|||||||	  RC:BC:CN\t214:7:4";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            // the naming convention for breakends is different
            Assert.Equal("1:814867:824517:4", variant.AlternateAlleles[0].VariantId);
        }

        [Fact]
        public void CopyNumberFromSomatic()
        {
            // NIR-811
            const string vcfLine = "1	816119	.	N	<CNV>	6	q10	SVTYPE=CNV;END=826343;CSQT=1|AL645608.2|ENST00000594233|	RC:BC:CN	186:11:4";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:816120:826343:4";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void MissingCopyNumber()
        {
            // NIR-927: this was caused because the header of the input file did not have TUMOR/NORMAL
            const string vcfLine = "1	72806722	Canvas:GAIN:1:72806723:84290708	N	<CNV>	49	PASS	SVTYPE=CNV;END=84290708	RC:BC:CN:MCC	 .	167:22451:4:2";

            // we create the variant from the vcf entry
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:72806723:84290708:4";
            var observedVid = variant.AlternateAlleles[0].VariantId;

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void CopyNumberVariation_1000G()
        {
            const string vcfLine = "1	713044	DUP_gs_CNV_1_713044_755966	C	<CN0>,<CN2>	100	PASS	AC=3,206;AF=0.000599042,0.0411342;AN=5008;CS=DUP_gs;END=755966;NS=2504;SVTYPE=CNV;DP=20698;EAS_AF=0.001,0.0615;AMR_AF=0.0014,0.0259;AFR_AF=0,0.0303;EUR_AF=0.001,0.0417;SAS_AF=0,0.045";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:713045:755966:0";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);

            const string expectedVid2 = "1:713045:755966:2";
            var observedVid2 = VID.Create(variant.ReferenceName, variant.AlternateAlleles[1]);

            Assert.Equal(expectedVid2, observedVid2);

            var annotatedVariant = DataUtilities.GetVariant("chr1_59758869_T_G_UF_RefSeq84_pos.ndb", vcfLine);
            Assert.NotNull(annotatedVariant);

            const string expectedJsonAltAllele = "\"altAllele\":\"CN0\"";
            const string expectedJsonAltAllele2 = "\"altAllele\":\"CN2\"";
            const string expectedJsonVid = "\"vid\":\"" + expectedVid + "\"";
            const string expectedJsonVid2 = "\"vid\":\"" + expectedVid2 + "\"";

            Assert.Contains(expectedJsonAltAllele, annotatedVariant.ToString());
            Assert.Contains(expectedJsonAltAllele2, annotatedVariant.ToString());
            Assert.Contains(expectedJsonVid, annotatedVariant.ToString());
            Assert.Contains(expectedJsonVid2, annotatedVariant.ToString());
        }

        [Fact]
        public void Deletion()
        {
            var altAllele = new VariantAlternateAllele(100, 104, "TAGGT", "")
            {
                NirvanaVariantType = VariantType.deletion
            };

            const string expectedVid = "4:100:104";
            var observedVid = VID.Create("4", altAllele);

            Assert.Equal(expectedVid, observedVid);
        }


        [Fact]
        public void Insertion()
        {
            var altAllele = new VariantAlternateAllele(100, 99, "", "CGA")
            {
                NirvanaVariantType = VariantType.insertion
            };

            const string expectedVid = "4:100:99:CGA";
            var observedVid = VID.Create("4", altAllele);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void InsertionDeletion()
        {
            var altAllele = new VariantAlternateAllele(100, 104, "TAGGT", "CCCCCC")
            {
                NirvanaVariantType = VariantType.indel
            };

            const string expectedVid = "4:100:104:CCCCCC";
            var observedVid = VID.Create("4", altAllele);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void Line1()
        {
            const string vcfLine = "1	812283	L1_umary_LINE1_1	G	<INS:ME:LINE1>	100	PASS	AC=58;AF=0.0115815;AN=5008;CS=L1_umary;MEINFO=LINE1,2926,3363,+;NS=2504;SVLEN=437;SVTYPE=LINE1;TSD=null;DP=19016;EAS_AF=0.0109;AMR_AF=0.0187;AFR_AF=0.0098;EUR_AF=0.0179;SAS_AF=0.0031";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:812284:812720:MEI";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void LongInsertion()
        {
            var altAllele = new VariantAlternateAllele(100, 99, "",
                "TTTCAGGGGGGGAGCCCTCATGGTCTCTTCTACTGATGACTCAACACGCTAGG")
            {
                NirvanaVariantType = VariantType.insertion
            };

            var md5 = SupplementaryAnnotation.GetMd5HashString(altAllele.AlternateAllele);

            var expectedVid = "4:100:99:" + md5;
            var observedVid = VID.Create("4", altAllele);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void LossOfHeterozygosity()
        {
            const string vcfLine = "1	753829	Canvas:REF:1:753830:2581602	N	<CNV>	38	PASS	SVTYPE=LOH;END=2581602	RC:BC:CN:MCC	.	86:1865:2:2";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:753830:2581602:2";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void MultipleNucleotideVariant()
        {
            var altAllele = new VariantAlternateAllele(100, 104, "TAGGT", "ACTTA")
            {
                NirvanaVariantType = VariantType.MNV
            };

            const string expectedVid = "4:100:104:ACTTA";
            var observedVid = VID.Create("4", altAllele);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void ReferenceNoCall()
        {
            const string vcfLine = "1\t10035\t.\tC\t.\t.\tLowGQX\tEND=10067;BLOCKAVG_min30p3a\tGT:GQX:DP:DPF\t0/0:15:6:0";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            var samples = variant.ExtractSampleInfo();
            Assert.Equal(1, samples.Count);

            const string expectedVid = "1:10035:10067";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SingleNucleotideVariant()
        {
            var altAllele = new VariantAlternateAllele(100, 100, "T", "A")
            {
                NirvanaVariantType = VariantType.SNV
            };

            const string expectedVid = "4:100:A";
            var observedVid = VID.Create("4", altAllele);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void Sva()
        {
            const string vcfLine =
                "1	1517860	SVA_umary_SVA_1	A	<INS:ME:SVA>	100	PASS	AC=3;AF=0.000599042;AN=5008;CS=SVA_umary;MEINFO=SVA,44,394,+;NS=2504;SVLEN=350;SVTYPE=SVA;TSD=null;DP=18602;EAS_AF=0.001;AMR_AF=0;AFR_AF=0.0015;EUR_AF=0;SAS_AF=0";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:1517861:1518210:MEI";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SvDeletion()
        {
            const string vcfLine =
                "1	207981229	MantaDEL:9144:0:1:0:0:0	A	<DEL>	.	MGE10kb	END=208014817;SVTYPE=DEL;SVLEN=-33588;CIPOS=0,4;CIEND=0,4;HOMLEN=4;HOMSEQ=GAGG;SOMATIC;SOMATICSCORE=44;ColocalizedCanvas	PR:SR	13,0:16,0	23,4:19,4";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:207981230:208014817";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SvDuplication()
        {
            const string vcfLine =
                "1	55024355	DUP_delly_DUP22372	C	<CN2>	100	PASS	AC=3;AF=0.000599042;AN=5008;CIEND=-150,150;CIPOS=-150,150;CS=DUP_delly;END=55050323;NS=2504;SVLEN=25968;SVTYPE=DUP;IMPRECISE;DP=19622;EAS_AF=0.001;AMR_AF=0;AFR_AF=0;EUR_AF=0;SAS_AF=0.002";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:55024356:55050323:DUP";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SvInsertion()
        {
            const string vcfLine =
                "12	129771777	MantaINS:104437:0:0:0:3:0	A	<INS>	.	PASS	END=129771779;SVTYPE=INS;LEFT_SVINSSEQ=TCTCACTCATAGGTGGGAATTGAACAATGAGATCACATGGACACAGGAAGGGGAATATCACACTCT;RIGHT_SVINSSEQ=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA;SOMATIC;SOMATICSCORE=34	PR:SR	7,0:8,0	5,0:14,9";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "12:129771778:129771779:INS";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SvInversion()
        {
            const string vcfLine =
                "2	70850318	MantaINV:15204:1:2:0:0:0	T	<INV>	.	MinSomaticScore	END=71389878;SVTYPE=INV;SVLEN=539560;IMPRECISE;CIPOS=-134,135;CIEND=-144,145;INV5;SOMATIC;SOMATICSCORE=11	PR	9,0	31,5";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "2:70850319:71389878:Inverse";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SvTandemDuplication()
        {
            const string vcfLine =
                "1	224646603	MantaDUP:TANDEM:9996:0:1:0:0:0	A	<DUP:TANDEM>	.	MGE10kb	END=224800119;SVTYPE=DUP;SVLEN=153516;SVINSLEN=37;SVINSSEQ=CAAAACTTACTATAGCAGTTCTGTGAGCTGCTCTAGC;SOMATIC;SOMATICSCORE=58;ColocalizedCanvas	PR:SR	26,0:20,0	51,10:60,12";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:224646604:224800119:TDUP";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void SvTranslocationBreakEnd()
        {
            const string vcfLine =
                "1	160359958	MantaBND:6887:0:1:0:0:0:1	G	G]3:19392235]	.	PASS	SVTYPE=BND;MATEID=MantaBND:6887:0:1:0:0:0:0;IMPRECISE;CIPOS=-137,137;SOMATIC;SOMATICSCORE=41;BND_DEPTH=22;MATE_BND_DEPTH=11	PR	21,0	41,7";

            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            const string expectedVid = "1:160359958:-:3:19392235:-";
            var observedVid = VID.Create(variant.ReferenceName, variant.AlternateAlleles[0]);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void Unknown()
        {
            var variant   = VcfUtilities.GetVariantFeature("4	100	.	TAGGT	CCCCC	.	.	.");
            var altAllele = variant.AlternateAlleles[0];
            altAllele.NirvanaVariantType = VariantType.unknown;

            const string expectedVid = "4:100:104";
            var observedVid = VID.Create(variant.ReferenceName, altAllele);

            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void MantaInsertion()
        {
            var annotatedVariant = DataUtilities.GetVariant(null as string,
                "chr1	79805469	MantaINS:169:0:0:0:0:0	C	CTCTAGAGACCTTCCTATTTCTTGCTTTTAGTTCATTGGCAAATGTCTTTCTTGATTGGAAATGTGATACTCATTTTCAAGACTACTCTATCTGTAATATCTCTGCAATCTTATTTTGATTATTCTACTAAAAATGTACTT	76	PASS	END=79805469;SVTYPE=INS;SVLEN=140;CIGAR=1M140I;CIPOS=0,2;HOMLEN=2;HOMSEQ=TC	GT:FT:GQ:PL:PR:SR	0/0:MinGQ:5:45,0,0:0,0:0,1	0/0:PASS:48:0,0,0:0,0:0,0	1/1:MinGQ:7:127,9,0:0,1:0,2");
            Assert.NotNull(annotatedVariant);
            Assert.Equal("1:79805470:79805469:2631c313bc68dd98c1ce1e5505a6f800", annotatedVariant.AnnotatedAlternateAlleles.First().VariantId);
        }
    }
}