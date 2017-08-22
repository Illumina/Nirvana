using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Vcf.Info;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
	public sealed class VariantFactoryTests
	{
		//chr1    69391    .    A    <DEL>    .    .    SVTYPE=DEL;END=138730    .    .
		[Fact]
		public void GetVariant_svDel()
		{
			var infoData = VcfInfoParser.Parse("SVTYPE=DEL;END=138730");

			var chromosome1 = new Chromosome("chr1", "1", 0);
			var variantFactory = new VariantFactory(new Dictionary<string, IChromosome> { { "1", chromosome1 } }, null,false);

			var variants = variantFactory.CreateVariants(chromosome1,null, 69391, 138730, "A", new[] { "<DEL>" }, infoData, null);
			Assert.NotNull(variants);
			Assert.Equal(2, variants[0].BreakEnds.Length);
		}

		//1	723707	Canvas:GAIN:1:723708:2581225	N	<CNV>	41	PASS	SVTYPE=CNV;END=2581225	RC:BC:CN:MCC	.	129:3123:3:2
		[Fact]
		public void GetVariant_canvas_cnv()
		{
			var infoData = new InfoData(2581225, null, VariantType.copy_number_gain, null, null, null, 3, null, false, null, null, false, false, null, null, null);
			var chromosome1 = new Chromosome("chr1", "1", 0);
			var variantFactory = new VariantFactory(new Dictionary<string, IChromosome> { { "1", chromosome1 } }, null, false);

			var variants = variantFactory.CreateVariants(chromosome1, "Canvas:GAIN:1:723708:2581225", 723707, 2581225, "N", new[] { "<CNV>" }, infoData, 3);
			Assert.NotNull(variants);
			Assert.Null(variants[0].BreakEnds);

			Assert.Equal("1:723708:2581225:3", variants[0].VariantId);
			Assert.Equal(VariantType.copy_number_gain, variants[0].Type);
		}
		
		
		//1       37820921        MantaDUP:TANDEM:5515:0:1:0:0:0  G       <DUP:TANDEM>    .       MGE10kb END=38404543;SVTYPE=DUP;SVLEN=583622;CIPOS=0,1;CIEND=0,1;HOMLEN=1;HOMSEQ=A;SOMATIC;SOMATICSCORE=63;ColocalizedCanvas    PR:SR   39,0:44,0       202,26:192,32
		[Fact]
		public void GetVariant_tandem_duplication()
		{
			var infoData = new InfoData(2581225, null, VariantType.duplication, null, null, null, 3, null, false, null, null, false, false, null, null, null);
			var chromosome1 = new Chromosome("chr1", "1", 0);
			var variantFactory = new VariantFactory(new Dictionary<string, IChromosome> { { "1", chromosome1 } }, null, false);

			var variants = variantFactory.CreateVariants(chromosome1, null, 723707, 2581225, "N", new[] { "<DUP:TANDEM>" }, infoData, null);
			Assert.NotNull(variants);

			Assert.Equal(VariantType.tandem_duplication, variants[0].Type);
		}
	}
}