using System.Collections.Generic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
	public class CustomItemTest
	{
		[Fact]
		public void EqualityAndHash()
		{
			var customItem = new CustomItem("chr1",100, "A", "C","test", "cust101", true, null, null);

			var customHash = new HashSet<CustomItem>() { customItem };

			Assert.Equal(1, customHash.Count);
			Assert.True(customHash.Contains(customItem));
		}
	}
}
