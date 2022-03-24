using System.IO;
using CacheUtils.TranscriptCache;
using ErrorHandling.Exceptions;
using Genome;
using Moq;
using OptimizedCore;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;
using Vcf;
using Vcf.Info;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class VariantFactoryTests
    {
        private static readonly ISequence Sequence = new NSequence();

        private readonly ISequence _chr12Seq = new SimpleSequence(
            "TCCCCATGCTGCTCTTTTTTGCAAACACCAACACAATTTGGGCTCCATTTATAAGGCATCTGCTGCACCAACCCTCTTTCTTGGTGCTTACTGGACCTGCTCAGGGTTAATTTCTAACTCAAAGAACCTAACTTGGAGTAACTCCGTACCACCAGCAAAGCGACTGGCTTTGGGGAATGACATTTACAATGTATCCACTGTTATTTGGTCACCCAGCAAACTGTCATTTTTCAGAAACCAGGGCTGTCTCACAAACTGGCTTTCAATAAGGTGGGTTGCTTAGCAACTGCCAAGGAATTAAGAAGACAGAATAAGGTATCCGCCAGAGATATTTTATGACCAAAATGAGCTGCACTCATGTGTCTGGTTGTGTTCAAGGTAACCAAGTAAGAGATAACACCCGACTATTTTTGCATCATGAGGAAAAATACTTGGCTTCTGCCCAGAAGGGCAATTATCTCAAAGTCTTGGCAGGCCCCATGGTATGAGAAATGGTAACTGATATGGGGGTTAAAAAAAA",
            106499648);

        private readonly VariantId         _vidCreator       = new();
        private readonly LegacyVariantId   _legacyVidCreator = new(null);
        private readonly Mock<ISequence>   _sequenceMock     = new();
        private readonly VariantFactory    _variantFactory;
        private readonly ISequenceProvider _sequenceProvider;

        public VariantFactoryTests()
        {
            // GRCh38
            _sequenceMock.Setup(x => x.Substring(1037629,   1)).Returns("G");
            _sequenceMock.Setup(x => x.Substring(787922,    1)).Returns("A");
            _sequenceMock.Setup(x => x.Substring(110541588, 1)).Returns("T");
            _sequenceMock.Setup(x => x.Substring(100955983, 1)).Returns("C");
            _sequenceMock.Setup(x => x.Substring(11071438,  1)).Returns("G");
            _sequenceMock.Setup(x => x.Substring(934063,    1)).Returns("A");
            _sequenceMock.Setup(x => x.Substring(36690135,  1)).Returns("C");
            _sequenceMock.Setup(x => x.Substring(20093,     1)).Returns("T");
            _sequenceMock.Setup(x => x.Substring(15902,     1)).Returns("G");

            // GRCh37 (for multi-allelic deletion with left alignment)
            _sequenceMock.Setup(x => x.Substring(106500157, 1)).Returns("G");
            _sequenceMock.Setup(x => x.Substring(106500158, 1)).Returns("T");
            _sequenceMock.Setup(x => x.Substring(106500159, 1)).Returns("T");
            _sequenceMock.Setup(x => x.Substring(106500159, 2)).Returns("TA");
            _sequenceMock.Setup(x => x.Substring(106499659, 500)).Returns(
                "CTCTTTTTTGCAAACACCAACACAATTTGGGCTCCATTTATAAGGCATCTGCTGCACCAACCCTCTTTCTTGGTGCTTACTGGACCTGCTCAGGGTTAATTTCTAACTCAAAGAACCTAACTTGGAGTAACTCCGTACCACCAGCAAAGCGACTGGCTTTGGGGAATGACATTTACAATGTATCCACTGTTATTTGGTCACCCAGCAAACTGTCATTTTTCAGAAACCAGGGCTGTCTCACAAACTGGCTTTCAATAAGGTGGGTTGCTTAGCAACTGCCAAGGAATTAAGAAGACAGAATAAGGTATCCGCCAGAGATATTTTATGACCAAAATGAGCTGCACTCATGTGTCTGGTTGTGTTCAAGGTAACCAAGTAAGAGATAACACCCGACTATTTTTGCATCATGAGGAAAAATACTTGGCTTCTGCCCAGAAGGGCAATTATCTCAAAGTCTTGGCAGGCCCCATGGTATGAGAAATGGTAACTGATATGGGGGT");

            _sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh38, _sequenceMock.Object, ChromosomeUtilities.RefNameToChromosome);
            _variantFactory   = new VariantFactory(_sequenceMock.Object, _vidCreator);
        }

        private IPosition ParseVcfLine(string vcfLine)
        {
            string[]    vcfFields  = vcfLine.OptimizedSplit('\t');
            Chromosome chromosome = ReferenceNameUtilities.GetChromosome(ChromosomeUtilities.RefNameToChromosome, vcfFields[VcfCommon.ChromIndex]);

            (int start, bool foundError) = vcfFields[VcfCommon.PosIndex].OptimizedParseInt32();
            if (foundError) throw new InvalidDataException($"Unable to convert the VCF position to an integer: {vcfFields[VcfCommon.PosIndex]}");

            var simplePosition = SimplePosition.GetSimplePosition(chromosome, start, vcfFields, new NullVcfFilter());

            return Position.ToPosition(simplePosition, null, _sequenceProvider, null, _variantFactory);
        }

        // chr1    69391    .    A    <DEL>    .    .    SVTYPE=DEL;END=138730    .    .
        [Fact]
        public void CreateVariants_svDel()
        {
            var      builder        = new InfoDataBuilder {SvType = "DEL", End = 138730};
            InfoData infoData       = builder.Create();
            var      variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 69391, 138730, "A", new[] {"<DEL>"}, infoData,
                new[] {false}, false, null, null);
            Assert.NotNull(variants);
        }

        // 1	723707	Canvas:GAIN:1:723708:2581225	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2
        [Fact]
        public void CreateVariants_canvas_cnv()
        {
            var      builder  = new InfoDataBuilder {SvType = "CNV", End = 2581225};
            InfoData infoData = builder.Create();

            var variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 723707, 2581225, "N", new[] {"<CNV>"}, infoData,
                new[] {false}, false, null, null);
            Assert.NotNull(variants);

            Assert.Equal("1-723707-2581225-N-<CNV>-CNV",    variants[0].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[0].Type);
        }

        // chr1    854895  Canvas:COMPLEXCNV:chr1:854896-861879    N       <CN0>,<CN3>     .       PASS    SVTYPE=CNV;END=861879;CNVLEN=6984;CIPOS=-291,291;CIEND=-291,291 GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/1:59.45:12:1:1:.:25.34:PASS:. 0/1:59.45:12:1:1:.:25.34:PASS:. 1/2:165.40:12:3:3:16.80:16.71:PASS:.
        [Fact]
        public void CreateVariants_canvas_cnx()
        {
            var      builder        = new InfoDataBuilder {SvType = "CNV", End = 861879, CiPos = new[] {-291, 291}, CiEnd = new[] {-291, 291}};
            InfoData infoData       = builder.Create();
            var      variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 854895, 861879, "N", new[] {"<CN0>", "<CN3>"}, infoData,
                new[] {false, false}, false, null, null);
            Assert.NotNull(variants);
            Assert.Equal(2, variants.Length);

            Assert.Equal("1-854895-861879-N-<CN0>-CNV",     variants[0].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[0].Type);

            Assert.Equal("1-854895-861879-N-<CN3>-CNV",     variants[1].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[1].Type);
        }

        // chr1    1463185 Canvas:COMPLEXCNV:chr1:1463186-1476229  N       <CN0>,<DUP>     .       PASS    SVTYPE=CNV;END=1476229;CNVLEN=13044;CIPOS=-415,415;CIEND=-291,291       GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/0:109.56:15:2:.:.:20.04:PASS:.        1/1:0.00:15:0:.:.:64.59:PASS:.  ./2:167.45:15:3:.:.:17.87:PASS:.
        [Fact]
        public void CreateVariants_canvas_cnv_dup()
        {
            var      builder        = new InfoDataBuilder {SvType = "CNV", End = 1476229, CiPos = new[] {-415, 415}, CiEnd = new[] {-291, 291}};
            InfoData infoData       = builder.Create();
            var      variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 1463185, 1476229, "N", new[] {"<CN0>", "<DUP>"}, infoData,
                new[] {false, false}, false, null, null);
            Assert.NotNull(variants);
            Assert.Equal(2, variants.Length);

            Assert.Equal("1-1463185-1476229-N-<CN0>-CNV",   variants[0].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[0].Type);

            Assert.Equal("1-1463185-1476229-N-<DUP>-CNV", variants[1].VariantId);
            Assert.Equal(VariantType.copy_number_gain,    variants[1].Type); // <DUP>s are copy number gains
        }

        // chr1    1463185 .  N       <DUP>     .       PASS    SVTYPE=DUP;END=1476229;SVLEN=13044;CIPOS=-415,415;CIEND=-291,291       GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/0:109.56:15:2:.:.:20.04:PASS:.        1/1:0.00:15:0:.:.:64.59:PASS:.  ./1:167.45:15:3:.:.:17.87:PASS:.
        [Fact]
        public void CreateVariants_dup()
        {
            var      builder        = new InfoDataBuilder {SvType = "DUP", End = 1476229, CiPos = new[] {-415, 415}, CiEnd = new[] {-291, 291}};
            InfoData infoData       = builder.Create();
            var      variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 1463185, 1476229, "N", new[] {"<DUP>"}, infoData,
                new[] {false}, false, null, null);
            Assert.NotNull(variants);
            Assert.Single(variants);

            Assert.Equal("1-1463185-1476229-N-<DUP>-DUP", variants[0].VariantId);
            Assert.Equal(VariantType.duplication,         variants[0].Type);
        }

        // 1       37820921        MantaDUP:TANDEM:5515:0:1:0:0:0  G       <DUP:TANDEM>    .       MGE10kb END=38404543;SVTYPE=DUP;SVLEN=583622;CIPOS=0,1;CIEND=0,1;HOMLEN=1;HOMSEQ=A;SOMATIC;SOMATICSCORE=63;ColocalizedCanvas    PR:SR   39,0:44,0       202,26:192,32
        [Fact]
        public void CreateVariants_tandem_duplication()
        {
            var      builder = new InfoDataBuilder {SvType = "DUP", End = 38404543, SvLength = 583622, CiPos = new[] {0, 1}, CiEnd = new[] {0, 1}};
            InfoData infoData = builder.Create();
            var      variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 723707, 2581225, "N", new[] {"<DUP:TANDEM>"}, infoData,
                new[] {false}, false, null, null);
            Assert.NotNull(variants);

            Assert.Equal(VariantType.tandem_duplication, variants[0].Type);
        }

        // 1   4000000 .   N   <ROH> .   ROHLC   SVTYPE=ROH;END=4001000  GT  .   .   1
        [Fact]
        public void CreateVariants_ROH()
        {
            var      builder        = new InfoDataBuilder {SvType = "ROH", End = 4001000};
            InfoData infoData       = builder.Create();
            var      variantFactory = new VariantFactory(Sequence, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr1, 400_0000, 400_1000, "N", new[] {"<ROH>"}, infoData,
                new[] {false}, false, null, null);

            Assert.Equal(AnnotationBehavior.RunsOfHomozygosity, variants[0].Behavior);
            Assert.Equal(VariantType.run_of_homozygosity,       variants[0].Type);
        }

        // chr12	106500158	.	GTTA	GTA,GT	.	.	.
        [Fact]
        public void CreateVariants_LegacyVid_DisableLeftAlignment_MultiAllelic_Deletions()
        {
            InfoData infoData       = new InfoDataBuilder().Create();
            var      variantFactory = new VariantFactory(_chr12Seq, _legacyVidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr12, 106500158, 106500161, "GTTA",
                new[] {"GTA", "GT"}, infoData, new[] {false, false}, false, null, null);

            Assert.Equal(2,                        variants.Length);
            Assert.Equal("12:106500160:106500160", variants[0].VariantId);
            Assert.Equal("12:106500160:106500161", variants[1].VariantId);
        }

        // chr12	106500158	.	GTTA	GTA,GT	.	.	.
        [Fact]
        public void CreateVariants_NormalVid_EnableLeftAlignment_MultiAllelic_Deletions()
        {
            InfoData infoData       = new InfoDataBuilder().Create();
            var      variantFactory = new VariantFactory(_chr12Seq, _vidCreator);

            IVariant[] variants = variantFactory.CreateVariants(ChromosomeUtilities.Chr12, 106500158, 106500161, "GTTA",
                new[] {"GTA", "GT"}, infoData, new[] {false, false}, false, null, null);

            Assert.Equal(2,                    variants.Length);
            Assert.Equal("12-106500158-GT-G",  variants[0].VariantId);
            Assert.Equal("12-106500159-TTA-T", variants[1].VariantId);
        }

        [Fact]
        public void ToPosition_SNV()
        {
            IPosition  position = ParseVcfLine("chr1	15274	SNV	A	T	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-15274-A-T",   variant.VariantId);
            Assert.Equal(VariantType.SNV, variant.Type);
            Assert.Equal(15274,           variant.Start);
            Assert.Equal(15274,           variant.End);
        }

        [Fact]
        public void ToPosition_insertion()
        {
            IPosition  position = ParseVcfLine("chr1	15903	INS	G	GC	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-15903-G-GC",        variant.VariantId);
            Assert.Equal(VariantType.insertion, variant.Type);
            Assert.Equal(15904,                 variant.Start);
            Assert.Equal(15903,                 variant.End);
        }

        [Fact]
        public void ToPosition_deletion()
        {
            IPosition  position = ParseVcfLine("chr1	20094	DEL	TAA	T	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-20094-TAA-T",      variant.VariantId);
            Assert.Equal(VariantType.deletion, variant.Type);
            Assert.Equal(20095,                variant.Start);
            Assert.Equal(20096,                variant.End);
        }

        [Fact]
        public void ToPosition_CANVAS_LOH()
        {
            IPosition  position = ParseVcfLine("chr1	787923	CNV_CANVAS_LOH	N	<CNV>	40	.	SVTYPE=LOH;END=887923	RC:BC:CN:MCC	106.52:12642:2:2");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-787923-887923-A-<CNV>-LOH",     variant.VariantId);
            Assert.Equal(VariantType.copy_number_variation, variant.Type);
            Assert.Equal(787924,                            variant.Start);
            Assert.Equal(887923,                            variant.End);
        }

        [Fact]
        public void ToPosition_Manta_SmallDeletion()
        {
            IPosition position = ParseVcfLine(
                "chr1	934064	SV_SNV	AGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTCCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTCCGTTACAGGTGGGCAGGGGAGGCGGCTCCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTCCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCTGCTCCGTTACAGGTGGGCGGGGGAGGCTGCTCCGTTACAGGTGGGCGGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTCCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTCCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCG	A	.	.	END=934904;SVTYPE=DEL	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal(
                "1-934064-AGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTCCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTCCGTTACAGGTGGGCAGGGGAGGCGGCTCCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTCCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCTGCTCCGTTACAGGTGGGCGGGGGAGGCTGCTCCGTTACAGGTGGGCGGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGGGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTCCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCAGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTCCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCGGGGGAGGCGGCTGCGTTACAGGTGGGCG-A",
                variant.VariantId);
            Assert.Equal(VariantType.deletion, variant.Type);
            Assert.Equal(934065,               variant.Start);
            Assert.Equal(934904,               variant.End);
        }

        [Fact]
        public void ToPosition_CANVAS_CNnum()
        {
            IPosition position = ParseVcfLine(
                "chr1	1037630	CNV_CN#	N	<CN0>	.	.	SVTYPE=CNV;END=1045024	GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ	0/1:60.76:8:1:.:.:22.51:PASS:.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-1037630-1045024-G-<CN0>-CNV",   variant.VariantId);
            Assert.Equal(VariantType.copy_number_variation, variant.Type);
            Assert.Equal(1037631,                           variant.Start);
            Assert.Equal(1045024,                           variant.End);
        }

        [Fact]
        public void ToPosition_SV_DUP()
        {
            IPosition  position = ParseVcfLine("chr1	1477854	SV_DUP	C	<DUP:TANDEM>	.	.	END=1477984;SVTYPE=DUP	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-1477854-1477984-C-<DUP:TANDEM>-DUP", variant.VariantId);
            Assert.Equal(VariantType.tandem_duplication,         variant.Type);
            Assert.Equal(1477855,                                variant.Start);
            Assert.Equal(1477984,                                variant.End);
        }

        [Fact]
        public void ToPosition_SV_INS()
        {
            IPosition  position = ParseVcfLine("chr1	1565683	SV_INS	G	<INS>	.	.	END=1565684;SVTYPE=INS	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-1565683-1565684-G-<INS>-INS", variant.VariantId);
            Assert.Equal(VariantType.insertion,           variant.Type);
            Assert.Equal(1565684,                         variant.Start);
            Assert.Equal(1565684,                         variant.End);
        }

        [Fact]
        public void ToPosition_SV_INV()
        {
            IPosition  position = ParseVcfLine("chr1	6558910	SV_INV	G	<INV>	.	.	END=6559723;SVTYPE=INV	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-6558910-6559723-G-<INV>-INV", variant.VariantId);
            Assert.Equal(VariantType.inversion,           variant.Type);
            Assert.Equal(6558911,                         variant.Start);
            Assert.Equal(6559723,                         variant.End);
        }

        [Fact]
        public void ToPosition_SV_Translocation()
        {
            IPosition  position = ParseVcfLine("chr1	9061384	SV_BND	C	C]chr14:93246833]	.	.	SVTYPE=BND	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-9061384-C-C]chr14:93246833]",    variant.VariantId);
            Assert.Equal(VariantType.translocation_breakend, variant.Type);
            Assert.Equal(9061384,                            variant.Start);
            Assert.Equal(9061384,                            variant.End);
        }

        [Fact]
        public void ToPosition_DRAGEN_LOH()
        {
            IPosition position = ParseVcfLine(
                "chr1	11071439	CNV_DRAGEN_LOH	N	<DEL>,<DUP>	.	.	SVTYPE=CNV;END=12859473;REFLEN=1788034	GT:CN:MCN:CNQ:MCNQ:CNF:MCNF:SD:MAF:BC:AS	1/2:2:0:1000:1000:2.03102:0.000203:248.8:0.0001:1493:1137");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1-11071439-12859473-G-<DEL>-CNV", variant.VariantId);
            Assert.Equal(VariantType.copy_number_loss,      variant.Type);
            Assert.Equal(11071440,                          variant.Start);
            Assert.Equal(12859473,                          variant.End);

            variant = variants[1];
            Assert.Equal("1-11071439-12859473-G-<DUP>-CNV", variant.VariantId);
            Assert.Equal(VariantType.copy_number_gain,      variant.Type);
            Assert.Equal(11071440,                          variant.Start);
            Assert.Equal(12859473,                          variant.End);
        }

        [Fact]
        public void ToPosition_STR()
        {
            IPosition position = ParseVcfLine(
                "chr3	63912684	STR	G	<STR12>	.	PASS	END=63912714;REF=10;RL=30;RU=GCA;VARID=ATXN7;REPID=ATXN7	GT:SO:REPCN:REPCI:ADSP:ADFL:ADIR:LC	0/1:SPANNING/SPANNING:10/12:10-10/12-12:9/3:8/11:0/0:26.270270");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("3-63912684-63912714-G-<STR12>-STR",       variant.VariantId);
            Assert.Equal(VariantType.short_tandem_repeat_variation, variant.Type);
            Assert.Equal(63912685,                                  variant.Start);
            Assert.Equal(63912714,                                  variant.End);
        }
        
        [Fact]
        public void STR_without_num_throws_user_error()
        {
            var vcfLine =
                "chr3	63912684	STR	G	<STR>	.	PASS	END=63912714;REF=10;RL=30;RU=GCA;VARID=ATXN7;REPID=ATXN7	GT:SO:REPCN:REPCI:ADSP:ADFL:ADIR:LC	0/1:SPANNING/SPANNING:10/12:10-10/12-12:9/3:8/11:0/0:26.270270"; 
            
            Assert.Throws<UserErrorException>(()=>ParseVcfLine(vcfLine));
            
        }

        [Fact]
        public void ToPosition_indel()
        {
            IPosition  position = ParseVcfLine("chr4	46758265	INDEL	GAGGTATAGAG	GTT	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("4-46758266-AGGTATAGAG-TT", variant.VariantId);
            Assert.Equal(VariantType.indel,          variant.Type);
            Assert.Equal(46758266,                   variant.Start);
            Assert.Equal(46758275,                   variant.End);
        }

        [Fact]
        public void ToPosition_MNV()
        {
            IPosition  position = ParseVcfLine("chr4	67754304	MNV	TGA	TTT	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("4-67754305-GA-TT", variant.VariantId);
            Assert.Equal(VariantType.MNV,    variant.Type);
            Assert.Equal(67754305,           variant.Start);
            Assert.Equal(67754306,           variant.End);
        }

        [Fact]
        public void ToPosition_CNV_DUP()
        {
            IPosition position = ParseVcfLine(
                "chr7	100955984	CNV_DUP	N	<DUP>	37	PASS	SVTYPE=CNV;END=100969873;REFLEN=13889	GT:SM:CN:BC:PE	./1:1.6625:3:12:48,81");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("7-100955984-100969873-C-<DUP>-CNV", variant.VariantId);
            Assert.Equal(VariantType.copy_number_gain,        variant.Type);
            Assert.Equal(100955985,                           variant.Start);
            Assert.Equal(100969873,                           variant.End);
        }

        [Fact]
        public void ToPosition_CNV_DEL()
        {
            IPosition position = ParseVcfLine(
                "chr7	110541589	CNV_DEL	N	<DEL>	27	cnvLength	SVTYPE=CNV;END=110548681;REFLEN=7092	GT:SM:CN:BC:PE	0/1:0.443182:1:7:19,17");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("7-110541589-110548681-T-<DEL>-CNV", variant.VariantId);
            Assert.Equal(VariantType.copy_number_loss,        variant.Type);
            Assert.Equal(110541590,                           variant.Start);
            Assert.Equal(110548681,                           variant.End);
        }

        [Fact]
        public void ToPosition_ROH()
        {
            IPosition  position = ParseVcfLine("chr22	36690136	ROH	N	<ROH>	.	.	END=36788158;SVTYPE=ROH	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("22-36690136-36788158-C-<ROH>-ROH", variant.VariantId);
            Assert.Equal(VariantType.run_of_homozygosity,    variant.Type);
            Assert.Equal(36690137,                           variant.Start);
            Assert.Equal(36788158,                           variant.End);
        }

        // this is actually on GRCh37
        [Fact]
        public void ToPosition_MultiAllelic_Deletions()
        {
            IPosition  position = ParseVcfLine("chr12	106500158	.	GTTA	GTA,GT	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("12-106500158-GT-G",  variant.VariantId);
            Assert.Equal(VariantType.deletion, variant.Type);
            Assert.Equal(106500159,            variant.Start);
            Assert.Equal(106500159,            variant.End);

            variant = variants[1];
            Assert.Equal("12-106500159-TTA-T", variant.VariantId);
            Assert.Equal(VariantType.deletion, variant.Type);
            Assert.Equal(106500160,            variant.Start);
            Assert.Equal(106500161,            variant.End);
        }
    }
}