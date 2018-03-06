using System.Collections.Generic;
using SAUtils.DataStructures;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CosmicTests
    {
        [Fact]
        public void EqualityAndHash()
        {
            var cosmicItem = new CosmicItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("100", "histology", "primarySite") }, 1);

            var customHash = new HashSet<CosmicItem> { cosmicItem };

            Assert.Single(customHash);
            Assert.Contains(cosmicItem, customHash);
        }

        [Fact]
        public void EqulityHashStudy()
        {
            var cosmicStudy = new CosmicItem.CosmicStudy("123", "histology1", "primarySite1");

            var studyHash = new HashSet<CosmicItem.CosmicStudy> { cosmicStudy };

            Assert.Single(studyHash);
            Assert.Contains(cosmicStudy, studyHash);
        }
    }
}
