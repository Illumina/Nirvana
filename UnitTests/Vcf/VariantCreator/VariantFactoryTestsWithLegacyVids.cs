using System.IO;
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
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class VariantFactoryTestsWithLegacyVids
    {
        private readonly Mock<ISequence>   _sequenceMock = new();
        private readonly ISequenceProvider _sequenceProvider;
        private readonly VariantFactory    _variantFactory;

        public VariantFactoryTestsWithLegacyVids()
        {
            _sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh38, _sequenceMock.Object, ChromosomeUtilities.RefNameToChromosome);
            var vidCreator = new LegacyVariantId(ChromosomeUtilities.RefNameToChromosome);
            _variantFactory = new VariantFactory(_sequenceMock.Object, vidCreator);
        }

        private IPosition ParseVcfLine(string vcfLine)
        {
            string[] vcfFields = vcfLine.OptimizedSplit('\t');
            Chromosome chromosome =
                ReferenceNameUtilities.GetChromosome(ChromosomeUtilities.RefNameToChromosome, vcfFields[VcfCommon.ChromIndex]);

            (int start, bool foundError) = vcfFields[VcfCommon.PosIndex].OptimizedParseInt32();
            if (foundError) throw new InvalidDataException($"Unable to convert the VCF position to an integer: {vcfFields[VcfCommon.PosIndex]}");

            var simplePosition = SimplePosition.GetSimplePosition(chromosome, start, vcfFields, new NullVcfFilter());

            return Position.ToPosition(simplePosition, null, _sequenceProvider, null, _variantFactory);
        }

        [Fact]
        public void ToPosition_SNV()
        {
            IPosition  position = ParseVcfLine("chr1	15274	SNV	A	T	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1:15274:T",     variant.VariantId);
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
            Assert.Equal("1:15904:15903:C",     variant.VariantId);
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
            Assert.Equal("1:20095:20096",      variant.VariantId);
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
            Assert.Equal("1:787924:887923:CNV",             variant.VariantId);
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
                "1:934065:934904",
                variant.VariantId);
            Assert.Equal(VariantType.deletion, variant.Type);
            Assert.Equal(934065,               variant.Start);
            Assert.Equal(934904,               variant.End);
        }

        [Fact]
        public void ToPosition_CANVAS_CNnum()
        {
            IPosition position =
                ParseVcfLine("chr1	1037630	CNV_CN#	N	<CN0>	.	.	SVTYPE=CNV;END=1045024	GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ	0/1:60.76:8:1:.:.:22.51:PASS:.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1:1037631:1045024:CN0",           variant.VariantId);
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
            Assert.Equal("1:1477855:1477984:TDUP",       variant.VariantId);
            Assert.Equal(VariantType.tandem_duplication, variant.Type);
            Assert.Equal(1477855,                        variant.Start);
            Assert.Equal(1477984,                        variant.End);
        }

        [Fact]
        public void ToPosition_SV_INS()
        {
            IPosition  position = ParseVcfLine("chr1	1565683	SV_INS	G	<INS>	.	.	END=1565684;SVTYPE=INS	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1:1565684:1565684:INS", variant.VariantId);
            Assert.Equal(VariantType.insertion,   variant.Type);
            Assert.Equal(1565684,                 variant.Start);
            Assert.Equal(1565684,                 variant.End);
        }

        [Fact]
        public void ToPosition_SV_INV()
        {
            IPosition  position = ParseVcfLine("chr1	6558910	SV_INV	G	<INV>	.	.	END=6559723;SVTYPE=INV	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1:6558911:6559723:Inverse", variant.VariantId);
            Assert.Equal(VariantType.inversion,       variant.Type);
            Assert.Equal(6558911,                     variant.Start);
            Assert.Equal(6559723,                     variant.End);
        }

        [Fact]
        public void ToPosition_SV_Translocation()
        {
            IPosition  position = ParseVcfLine("chr1	9061384	SV_BND	C	C]chr14:93246833]	.	.	SVTYPE=BND	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1:9061384:+:14:93246833:-",        variant.VariantId);
            Assert.Equal(VariantType.translocation_breakend, variant.Type);
            Assert.Equal(9061384,                            variant.Start);
            Assert.Equal(9061384,                            variant.End);
        }

        [Fact]
        public void ToPosition_DRAGEN_LOH()
        {
            IPosition position =
                ParseVcfLine(
                    "chr1	11071439	CNV_DRAGEN_LOH	N	<DEL>,<DUP>	.	.	SVTYPE=CNV;END=12859473;REFLEN=1788034	GT:CN:MCN:CNQ:MCNQ:CNF:MCNF:SD:MAF:BC:AS	1/2:2:0:1000:1000:2.03102:0.000203:248.8:0.0001:1493:1137");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("1:11071440:12859473:CDEL",   variant.VariantId);
            Assert.Equal(VariantType.copy_number_loss, variant.Type);
            Assert.Equal(11071440,                     variant.Start);
            Assert.Equal(12859473,                     variant.End);

            variant = variants[1];
            Assert.Equal("1:11071440:12859473:CDUP",   variant.VariantId);
            Assert.Equal(VariantType.copy_number_gain, variant.Type);
            Assert.Equal(11071440,                     variant.Start);
            Assert.Equal(12859473,                     variant.End);
        }

        [Fact]
        public void ToPosition_STR()
        {
            IPosition position =
                ParseVcfLine(
                    "chr3	63912684	STR	G	<STR12>	.	PASS	END=63912714;REF=10;RL=30;RU=GCA;VARID=ATXN7;REPID=ATXN7	GT:SO:REPCN:REPCI:ADSP:ADFL:ADIR:LC	0/1:SPANNING/SPANNING:10/12:10-10/12-12:9/3:8/11:0/0:26.270270");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("3:63912685:63912714:GCA:12",              variant.VariantId);
            Assert.Equal(VariantType.short_tandem_repeat_variation, variant.Type);
            Assert.Equal(63912685,                                  variant.Start);
            Assert.Equal(63912714,                                  variant.End);
        }

        [Fact]
        public void ToPosition_indel()
        {
            IPosition  position = ParseVcfLine("chr4	46758265	INDEL	GAGGTATAGAG	GTT	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("4:46758266:46758275:TT", variant.VariantId);
            Assert.Equal(VariantType.indel,        variant.Type);
            Assert.Equal(46758266,                 variant.Start);
            Assert.Equal(46758275,                 variant.End);
        }

        [Fact]
        public void ToPosition_MNV()
        {
            IPosition  position = ParseVcfLine("chr4	67754304	MNV	TGA	TTT	.	.	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("4:67754305:67754306:TT", variant.VariantId);
            Assert.Equal(VariantType.MNV,          variant.Type);
            Assert.Equal(67754305,                 variant.Start);
            Assert.Equal(67754306,                 variant.End);
        }

        [Fact]
        public void ToPosition_CNV_DUP()
        {
            IPosition position =
                ParseVcfLine("chr7	100955984	CNV_DUP	N	<DUP>	37	PASS	SVTYPE=CNV;END=100969873;REFLEN=13889	GT:SM:CN:BC:PE	./1:1.6625:3:12:48,81");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("7:100955985:100969873:CDUP", variant.VariantId);
            Assert.Equal(VariantType.copy_number_gain, variant.Type);
            Assert.Equal(100955985,                    variant.Start);
            Assert.Equal(100969873,                    variant.End);
        }

        [Fact]
        public void ToPosition_CNV_DEL()
        {
            IPosition position =
                ParseVcfLine(
                    "chr7	110541589	CNV_DEL	N	<DEL>	27	cnvLength	SVTYPE=CNV;END=110548681;REFLEN=7092	GT:SM:CN:BC:PE	0/1:0.443182:1:7:19,17");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("7:110541590:110548681:CDEL", variant.VariantId);
            Assert.Equal(VariantType.copy_number_loss, variant.Type);
            Assert.Equal(110541590,                    variant.Start);
            Assert.Equal(110548681,                    variant.End);
        }

        [Fact]
        public void ToPosition_ROH()
        {
            IPosition  position = ParseVcfLine("chr22	36690136	ROH	N	<ROH>	.	.	END=36788158;SVTYPE=ROH	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("22:36690137:36788158:ROH",      variant.VariantId);
            Assert.Equal(VariantType.run_of_homozygosity, variant.Type);
            Assert.Equal(36690137,                        variant.Start);
            Assert.Equal(36788158,                        variant.End);
        }

        // this is actually on GRCh37
        [Fact]
        public void ToPosition_MultiAllelic_Deletions()
        {
            IPosition  position = ParseVcfLine("chr12	106500158	.	GTTA	GTA,GT	.	.	.");
            IVariant[] variants = position.Variants;
            Assert.NotNull(variants);

            IVariant variant = variants[0];
            Assert.Equal("12:106500160:106500160", variant.VariantId);
            Assert.Equal(VariantType.deletion,     variant.Type);
            Assert.Equal(106500160,                variant.Start);
            Assert.Equal(106500160,                variant.End);

            variant = variants[1];
            Assert.Equal("12:106500160:106500161", variant.VariantId);
            Assert.Equal(VariantType.deletion,     variant.Type);
            Assert.Equal(106500160,                variant.Start);
            Assert.Equal(106500161,                variant.End);
        }
    }
}