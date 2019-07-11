using System.Collections.Generic;
using Genome;
using UnitTests.TestDataStructures;
using Variants;
using Vcf.Info;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class VariantFactoryTests
    {
        private readonly Chromosome _chromosome1 = new Chromosome("chr1", "1", 0);

        private readonly SimpleSequenceProvider _sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh37, null, new Dictionary<string, IChromosome> { { "1", new Chromosome("chr1", "1", 0) } });
        //chr1    69391    .    A    <DEL>    .    .    SVTYPE=DEL;END=138730    .    .
        [Fact]
        public void GetVariant_svDel()
        {
            var infoData = VcfInfoParser.Parse("SVTYPE=DEL;END=138730");

            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(_chromosome1, 69391, 138730, "A", new[] { "<DEL>" }, infoData, new[] { false }, false, null, null);
            Assert.NotNull(variants);
            Assert.Equal(2, variants[0].BreakEnds.Length);
        }

        //1	723707	Canvas:GAIN:1:723708:2581225	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2
        [Fact]
        public void GetVariant_canvas_cnv()
        {
            var infoData = new InfoData(null, null, 2581225, null, null,null,null, 2581225- 723707 +1,VariantType.copy_number_variation);

            var chromosome1 = new Chromosome("chr1", "1", 0);
            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(chromosome1, 723707, 2581225, "N", new[] { "<CNV>" }, infoData, new[] { false }, false, null, null);
            Assert.NotNull(variants);
            Assert.Null(variants[0].BreakEnds);

            Assert.Equal("1:723708:2581225:CNV", variants[0].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[0].Type);
        }

        //chr1    854895  Canvas:COMPLEXCNV:chr1:854896-861879    N       <CN0>,<CN3>     .       PASS    SVTYPE=CNV;END=861879;CNVLEN=6984;CIPOS=-291,291;CIEND=-291,291 GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/1:59.45:12:1:1:.:25.34:PASS:. 0/1:59.45:12:1:1:.:25.34:PASS:. 1/2:165.40:12:3:3:16.80:16.71:PASS:.
        [Fact]
        public void GetVariant_canvas_cnx()
        {
            var infoData = new InfoData(new []{ -291, 291 }, new []{ -291, 291 }, 861879,null, null,null, null, 6984, VariantType.copy_number_variation);
            var chromosome1 = new Chromosome("chr1", "1", 0);
            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(chromosome1, 854895, 861879, "N", new[] { "<CN0>", "<CN3>" }, infoData, new[] { false, false }, false, null, null);
            Assert.NotNull(variants);
            Assert.Equal(2, variants.Length);

            Assert.Equal("1:854896:861879:CN0", variants[0].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[0].Type);

            Assert.Equal("1:854896:861879:CN3", variants[1].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[1].Type);
        }

        //chr1    1463185 Canvas:COMPLEXCNV:chr1:1463186-1476229  N       <CN0>,<DUP>     .       PASS    SVTYPE=CNV;END=1476229;CNVLEN=13044;CIPOS=-415,415;CIEND=-291,291       GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/0:109.56:15:2:.:.:20.04:PASS:.        1/1:0.00:15:0:.:.:64.59:PASS:.  ./2:167.45:15:3:.:.:17.87:PASS:.
        [Fact]
        public void GetVariant_canvas_cnv_dup()
        {
            var infoData = new InfoData(new []{ -291, 291 }, new []{ -415, 415 }, 1476229, null, null, null,null, 13044, VariantType.copy_number_variation);
            var chromosome1 = new Chromosome("chr1", "1", 0);
            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(chromosome1, 1463185, 1476229, "N", new[] { "<CN0>", "<DUP>" }, infoData, new[] { false, false }, false, null, null);
            Assert.NotNull(variants);
            Assert.Equal(2, variants.Length);

            Assert.Equal("1:1463186:1476229:CN0", variants[0].VariantId);
            Assert.Equal(VariantType.copy_number_variation, variants[0].Type);

            Assert.Equal("1:1463186:1476229:CDUP", variants[1].VariantId);
            Assert.Equal(VariantType.copy_number_gain, variants[1].Type);// <DUP>s are copy number gains
        }

        //chr1    1463185 .  N       <DUP>     .       PASS    SVTYPE=DUP;END=1476229;SVLEN=13044;CIPOS=-415,415;CIEND=-291,291       GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/0:109.56:15:2:.:.:20.04:PASS:.        1/1:0.00:15:0:.:.:64.59:PASS:.  ./1:167.45:15:3:.:.:17.87:PASS:.
        [Fact]
        public void GetVariant_dup()
        {
            //new InfoData(1476229, 13044, VariantType.duplication, null, null, null, 1, null, false, null, null, false, false, null, null, null);
            var infoData = new InfoData(new []{ -291, 291 }, new []{ -415, 415 }, 1476229, null, null, null, null, 13044, VariantType.duplication);
            var chromosome1 = new Chromosome("chr1", "1", 0);
            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(chromosome1, 1463185, 1476229, "N", new[] { "<DUP>" }, infoData, new[] { false }, false, null, null);
            Assert.NotNull(variants);
            Assert.Single(variants);

            Assert.Equal("1:1463186:1476229:DUP", variants[0].VariantId);
            Assert.Equal(VariantType.duplication, variants[0].Type);

        }

        //1       37820921        MantaDUP:TANDEM:5515:0:1:0:0:0  G       <DUP:TANDEM>    .       MGE10kb END=38404543;SVTYPE=DUP;SVLEN=583622;CIPOS=0,1;CIEND=0,1;HOMLEN=1;HOMSEQ=A;SOMATIC;SOMATICSCORE=63;ColocalizedCanvas    PR:SR   39,0:44,0       202,26:192,32
        [Fact]
        public void GetVariant_tandem_duplication()
        {
            var infoData = new InfoData(new []{ 0, 1 }, new[] { 0, 1 }, 38404543, null, null, null, null, 583622, VariantType.duplication);
            var chromosome1 = new Chromosome("chr1", "1", 0);
            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(chromosome1, 723707, 2581225, "N", new[] { "<DUP:TANDEM>" }, infoData, new[] { false }, false, null, null);
            Assert.NotNull(variants);

            Assert.Equal(VariantType.tandem_duplication, variants[0].Type);
        }
        //1   4000000 .   N   <ROH> .   ROHLC   SVTYPE=ROH;END=4001000  GT  .   .   1
        [Fact]
        public void GetVariant_ROH()
        {
            var infoData = new InfoData(null, null, 4001000, null, null, null, null, 1000, VariantType.run_of_homozygosity);
            var chromosome1 = new Chromosome("chr1", "1", 0);
            var variantFactory = new VariantFactory(_sequenceProvider);

            var variants = variantFactory.CreateVariants(chromosome1, 400_0000, 400_1000, "N", new []{"<ROH>"}, infoData, new []{false}, false, null, null);

            Assert.True(variants[0].Behavior.CanonicalTranscriptOnly);
            Assert.False(variants[0].Behavior.NeedFlankingTranscript);
            Assert.Equal(VariantType.run_of_homozygosity, variants[0].Type);
        }
    }
}