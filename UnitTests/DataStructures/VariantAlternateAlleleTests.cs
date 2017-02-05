using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
	public class VariantAlternateAlleleTests
	{
		[Fact]
		public void ToStringTest()
		{
			var vaa = new VariantAlternateAllele(100,101,"AT", "A");

			Assert.NotNull(vaa.ToString());
		}
	}
}
