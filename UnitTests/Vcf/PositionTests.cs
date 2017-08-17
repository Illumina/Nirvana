namespace UnitTests.Vcf
{
    public class PositionTests
    {
        //[Fact]
        //public void Position_snv()
        //{
        //    var vcfLine = "chr1	13133	.	T	C,G	36.00	q30;LowGQ	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
        //    var vcfField = vcfLine.Split('\t');
        //    var chrom = new Mock<IChromosome>();
        //    var refMinorProvider = new Mock<IRefMinorProvider>();
        //    var position = new Position(chrom.Object,vcfField,refMinorProvider.Object);
        //    Assert.Equal(13133,position.Start);
        //    Assert.Equal(13133,position.End);
        //    Assert.Equal("T",position.RefAllele);
        //    Assert.Equal(2,position.AltAlleles.Length);
        //    Assert.Equal("C",position.AltAlleles[0]);
        //    Assert.Equal("G", position.AltAlleles[1]);
        //    Assert.Equal(36.00,position.Quality);
        //    Assert.Equal(new[]{"q30","LowGQ"},position.Filters);
        //    Assert.Equal(2,position.Variants.Length);
        //    Assert.Equal(1,position.Samples.Length);
        //    Assert.NotNull(position.InfoData);
        //}

        //[Fact(Skip = "not implemented yet")]
        //public void Position_cnv()
        //{
        //    var vcfLine = "1	723707	Canvas:GAIN:1:723708:2581225	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2";
        //    var vcfField = vcfLine.Split('\t');
        //    var chrom = new Mock<IChromosome>();
        //    var refMinorProvider = new Mock<IRefMinorProvider>();
        //    var position = new Position(chrom.Object, vcfField, refMinorProvider.Object);
        //    Assert.Equal(723707, position.Start);
        //    Assert.Equal(2581225, position.End);

        //}
    }
}