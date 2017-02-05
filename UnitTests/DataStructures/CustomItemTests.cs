using System.Collections.Generic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
	public class CustomItemTests
	{
		[Fact]
		public void EqualityAndHash()
		{
			var customItem = new CustomItem("chr1",100, "A", "C","test", "cust101", true, null, null);

			var customHash = new HashSet<CustomItem> { customItem };

			Assert.Equal(1, customHash.Count);
			Assert.True(customHash.Contains(customItem));
		}

		[Fact]
		[Trait("Jira","NIR-2101")]
		public void CheckSaAltAllele()
		{
			var customItem = new CustomItem("chr1", 100, "ATATA", "TTT", "test", "cust101", true, null, null);

			var spc = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(100));
			customItem.SetSupplementaryAnnotations(spc);

			Assert.Equal("5TTT", spc.SaPosition.CustomItems[0].SaAltAllele);
		}
	}
}
