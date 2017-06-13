using VariantAnnotation.DataStructures.Variants;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures
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
