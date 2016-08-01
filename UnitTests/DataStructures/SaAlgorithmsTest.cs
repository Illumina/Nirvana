using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
	public sealed class SaAlgorithmsTest
	{
		[Theory]
		[InlineData(".",".")]
		[InlineData("iAG", "AG")]
		[InlineData("<TG", "TG")]
		[InlineData("2", "-")]
		[InlineData("C","C")]
		[InlineData("NTG","NTG")]
		public void ReverseSaReducedAlleleTests(string saAltAllele,string expectedAltAllele)
		{
			string observedAltAllele = SupplementaryAnnotation.ReverseSaReducedAllele(saAltAllele);
			Assert.Equal(expectedAltAllele, observedAltAllele);
		}
	}
}
