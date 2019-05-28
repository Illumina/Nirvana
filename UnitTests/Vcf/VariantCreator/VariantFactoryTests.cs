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

			var variants = variantFactory.CreateVariants(_chromosome1, 69391, 138730, "A", new[] { "<DEL>" }, infoData, new[] { false }, false, null);
			Assert.NotNull(variants);
			Assert.Equal(2, variants[0].BreakEnds.Length);
		}

        //1       37820921        MantaDUP:TANDEM:5515:0:1:0:0:0  G       <DUP:TANDEM>    .       MGE10kb END=38404543;SVTYPE=DUP;SVLEN=583622;CIPOS=0,1;CIEND=0,1;HOMLEN=1;HOMSEQ=A;SOMATIC;SOMATICSCORE=63;ColocalizedCanvas    PR:SR   39,0:44,0       202,26:192,32
        [Fact]
		public void GetVariant_tandem_duplication()
        {
            var infoData = new InfoData(new[] { 0, 1 }, new[] { 0, 1 }, 38404543, null, null, null, null, null,
                VariantType.duplication);

			var chromosome1 = new Chromosome("chr1", "1", 0);
			var variantFactory = new VariantFactory(_sequenceProvider);

			var variants = variantFactory.CreateVariants(chromosome1, 723707, 2581225, "N", new[] { "<DUP:TANDEM>" }, infoData, new[] { false }, false, null);
			Assert.NotNull(variants);

			Assert.Equal(VariantType.tandem_duplication, variants[0].Type);
		}
    }
}