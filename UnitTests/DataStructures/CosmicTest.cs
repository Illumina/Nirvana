using System.Collections.Generic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
	public class CosmicTest
	{
		[Fact]
		public void EqualityAndHash()
		{
			var cosmicItem = new CosmicItem("chr1", 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy>() {new CosmicItem.CosmicStudy("100", "histology", "primarySite")});

			var customHash = new HashSet<CosmicItem>() { cosmicItem };

			Assert.Equal(1, customHash.Count);
			Assert.True(customHash.Contains(cosmicItem));
		}

		[Fact]
		public void EqulityHashStudy()
		{
			var cosmicStudy = new CosmicItem.CosmicStudy("123", "histology1", "primarySite1");

			var studyHash = new HashSet<CosmicItem.CosmicStudy>() {cosmicStudy};

			Assert.Equal(1, studyHash.Count);
			Assert.True(studyHash.Contains(cosmicStudy));

		}
	}

}
