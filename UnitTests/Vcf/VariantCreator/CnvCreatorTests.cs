using Genome;
using Variants;
using Vcf.Info;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class CnvCreatorTests
    {
        private static readonly IChromosome Chr1 = new Chromosome("chr1","1", 0);

        [Fact]
        public void Create_Dragen_3_3_DEL()
        {
            // chr1    907965  DRAGEN:LOSS:907966-909406       N       <DEL>   .       SampleFT        SVTYPE=CNV;END=909406;REFLEN=1441       GT:SM:CN:BC:QS:FT:DN    0/1:0.516574:1:1:24:cnvLength:.     0/1:0.409726:1:1:26:cnvLength:. 0/1:0.496663:1:1:23:cnvLength:Inherited
            var infoData = new InfoData(null, null, 909406, null, null, null, null, null,
                VariantType.copy_number_variation);

            var observedResults = CnvCreator.Create(Chr1, 907965, "N", "<DEL>", infoData);

            Assert.Equal(Chr1,                         observedResults.Chromosome);
            Assert.Equal(907966,                       observedResults.Start);
            Assert.Equal(909406,                       observedResults.End);
            Assert.Equal("N",                          observedResults.RefAllele);
            Assert.Equal("<DEL>",                      observedResults.AltAllele);
            Assert.Equal(VariantType.copy_number_loss, observedResults.Type);
            Assert.Equal("1:907966:909406:CDEL",        observedResults.VariantId);
        }

        [Fact]
        public void Create_Dragen_3_3_DUP()
        {
            // chr1    1715898 DRAGEN:GAIN:1715899-1750149     N       <DUP>   .       PASS    SVTYPE=CNV;END=1750149;REFLEN=34251     GT:SM:CN:BC:QS:FT:DN    ./.:1.07189:2:6:33:PASS:.   ./1:1.53631:3:6:49:PASS:.       ./.:1.012:2:6:38:PASS:Inherited
            var infoData = new InfoData(null, null, 1750149, null, null, null, null, null,
                VariantType.copy_number_variation);

            var observedResults = CnvCreator.Create(Chr1, 1715898, "N", "<DUP>", infoData);

            Assert.Equal(Chr1, observedResults.Chromosome);
            Assert.Equal(1715899, observedResults.Start);
            Assert.Equal(1750149, observedResults.End);
            Assert.Equal("N", observedResults.RefAllele);
            Assert.Equal("<DUP>", observedResults.AltAllele);
            Assert.Equal(VariantType.copy_number_gain, observedResults.Type);
            Assert.Equal("1:1715899:1750149:CDUP", observedResults.VariantId);
        }

        [Fact]
        public void Create_Canvas_TotalCopyNumber()
        {
            // 1	723707	Canvas:GAIN:1:723708:2581225	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2
            var infoData = new InfoData(null, null, 2581225, null, null, null, null, null,
                VariantType.copy_number_variation);

            var observedResults = CnvCreator.Create(Chr1, 723707, "N", "<CNV>", infoData);

            Assert.Equal(Chr1, observedResults.Chromosome);
            Assert.Equal(723708, observedResults.Start);
            Assert.Equal(2581225, observedResults.End);
            Assert.Equal("N", observedResults.RefAllele);
            Assert.Equal("<CNV>", observedResults.AltAllele);
            Assert.Equal(VariantType.copy_number_variation, observedResults.Type);
            Assert.Equal("1:723708:2581225:CNV", observedResults.VariantId);
        }

        [Fact]
        public void Create_Canvas_AlleleSpecificCopyNumber()
        {
            //chr1    854895  Canvas:COMPLEXCNV:chr1:854896-861879    N       <CN0>,<CN3>     .       PASS    SVTYPE=CNV;END=861879;CNVLEN=6984;CIPOS=-291,291;CIEND=-291,291 GT:RC:BC:CN:MCC:MCCQ:QS:FT:DQ   0/1:59.45:12:1:1:.:25.34:PASS:. 0/1:59.45:12:1:1:.:25.34:PASS:. 1/2:165.40:12:3:3:16.80:16.71:PASS:.
            var infoData = new InfoData(new[] {-291, -291}, new[] {-291, -291}, 861879, null, null, null, null, null,
                VariantType.copy_number_variation);

            var observedResults = CnvCreator.Create(Chr1, 854895, "N", "<CN0>", infoData);

            Assert.Equal(Chr1, observedResults.Chromosome);
            Assert.Equal(854896, observedResults.Start);
            Assert.Equal(861879, observedResults.End);
            Assert.Equal("N", observedResults.RefAllele);
            Assert.Equal("<CN0>", observedResults.AltAllele);
            Assert.Equal(VariantType.copy_number_variation, observedResults.Type);
            Assert.Equal("1:854896:861879:CN0", observedResults.VariantId);
        }
    }
}
