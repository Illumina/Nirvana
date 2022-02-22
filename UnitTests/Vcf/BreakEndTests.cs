using System.Collections.Generic;
using Genome;
using UnitTests.SAUtils.InputFileParsers;
using UnitTests.TestUtilities;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
	public sealed class BreakEndTests
	{
		[Theory]
		[InlineData("T" , "[3:115024109[T", 28722335, "1:28722335:-:3:115024109:+")]
		[InlineData("C", "]6:42248252]C", 31410878, "1:31410878:-:6:42248252:-")]
		[InlineData("C", "CGATCTCAT[6:41297838[", 31561816, "1:31561816:+:6:41297838:+")]
		[InlineData("A", "A]8:100990100]", 84461562, "1:84461562:+:8:100990100:-")]
		public void ToString_translocation_breakend(string refAllele, string altAllele, int position, string expectedVid)
		{
            var seqProvider = ParserTestUtils.GetSequenceProvider(position, refAllele, 'A', new Dictionary<string, Chromosome> { { "1", ChromosomeUtilities.Chr1 } });
			var variantFactory = new VariantFactory(seqProvider);

			var breakEnds = variantFactory.GetTranslocationBreakends(ChromosomeUtilities.Chr1, refAllele, altAllele, position);
			Assert.NotNull(breakEnds);
			Assert.Single(breakEnds);
			Assert.Equal(expectedVid, breakEnds[0].ToString());
		}

		[Fact]
		public void ToString_deletion()
		{
            var seqProvider = ParserTestUtils.GetSequenceProvider(1594584, "T", 'A', new Dictionary<string, Chromosome> { { "1", ChromosomeUtilities.Chr1 } });
            var variantFactory = new VariantFactory(seqProvider);

			var breakEnds = variantFactory.GetSvBreakEnds("1", 1594584, VariantType.deletion, 1660503, false, false);
			Assert.NotNull(breakEnds);
			Assert.Equal(2, breakEnds.Length);
			Assert.Equal("1:1594584:+:1:1660504:+", breakEnds[0].ToString());
			Assert.Equal("1:1660504:-:1:1594584:-", breakEnds[1].ToString());
		}


		[Fact]
		public void ToString_duplication()
		{
            var seqProvider = ParserTestUtils.GetSequenceProvider(37820921, "T", 'A', new Dictionary<string, Chromosome> { { "1", ChromosomeUtilities.Chr1 } });
            var variantFactory = new VariantFactory(seqProvider);

            var breakEnds = variantFactory.GetSvBreakEnds("1", 37820921, VariantType.duplication, 38404543, false, false);
			Assert.NotNull(breakEnds);
			Assert.Equal(2, breakEnds.Length);
			Assert.Equal("1:38404543:+:1:37820921:+", breakEnds[0].ToString());
			Assert.Equal("1:37820921:-:1:38404543:-", breakEnds[1].ToString());
		}

		[Fact]
		public void ToString_inversion3()
		{
            var seqProvider = ParserTestUtils.GetSequenceProvider(63989116, "T", 'A', new Dictionary<string, Chromosome> { { "1", ChromosomeUtilities.Chr1 } });
            var variantFactory = new VariantFactory(seqProvider);

            var breakEnds = variantFactory.GetSvBreakEnds("1", 63989116, VariantType.inversion, 64291267, true, false);
			Assert.NotNull(breakEnds);
			Assert.Equal(2, breakEnds.Length);
			Assert.Equal("1:63989116:+:1:64291267:-", breakEnds[0].ToString());
			Assert.Equal("1:64291267:+:1:63989116:-", breakEnds[1].ToString());
		}

		[Fact]
		public void ToString_inversion5()
		{
            var seqProvider = ParserTestUtils.GetSequenceProvider(117582031, "T", 'A', new Dictionary<string, Chromosome> { { "1", ChromosomeUtilities.Chr1 } });
            var variantFactory = new VariantFactory(seqProvider);

            var breakEnds = variantFactory.GetSvBreakEnds("1", 117582031, VariantType.inversion, 117595110, false, true);
			Assert.NotNull(breakEnds);
			Assert.Equal(2, breakEnds.Length);
			Assert.Equal("1:117582032:-:1:117595111:+", breakEnds[0].ToString());
			Assert.Equal("1:117595111:-:1:117582032:+", breakEnds[1].ToString());
		}
	}
}