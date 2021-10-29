using System.Collections.Generic;
using SAUtils.DataStructures;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CosmicItemTests
    {
        [Fact]
        public void GetTissueCount_same_tumor()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0")
            }, 1);

            var counts = cosmicItem.GetTissueCounts();
            Assert.Equal(1, counts.Count);
            Assert.Equal(1, counts["primarySite 0"]);
        }

        [Fact]
        public void GetTissueCount_different_tumors_same_sites()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 0", "tier 0")
            }, 1);

            var counts = cosmicItem.GetTissueCounts();
            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts["primarySite 0"]);
        }

        [Fact]
        public void GetTissueCount_different_tumors_different_sites()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 1", "tier 0")
            }, 1);

            var counts = cosmicItem.GetTissueCounts();
            Assert.Equal(2, counts.Count);
            Assert.Equal(1, counts["primarySite 0"]);
            Assert.Equal(1, counts["primarySite 1"]);
        }

        [Fact]
        public void GetCancerTypeCount_same_tumor()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0")
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(1, cancerTypeCounts.Count);
            Assert.Equal(1, cancerTypeCounts["primary histology 0"]);
        }

        [Fact]
        public void GetCancerTypeCount_different_tumors_same_histologies()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 0", "tier 0")
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(2, cancerTypeCounts.Count);
            Assert.Equal(2, cancerTypeCounts["primary histology 0"]);
        }
  
        [Fact]
        public void GetCancerTypeCount_different_tumors_different_histologies()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("101", "primary histology 1", "primarySite 0", "tier 0")
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(2, cancerTypeCounts.Count);
            Assert.Equal(1, cancerTypeCounts["primary histology 0"]);
            Assert.Equal(1, cancerTypeCounts["primary histology 1"]);
        }
        
        [Fact]
                public void GetTierCount_same_tumor()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0")
            }, 1);

            var tierCounts = cosmicItem.GetTierCounts();
            Assert.Equal(1, tierCounts.Count);
            Assert.Equal(1, tierCounts["tier 0"]);
        }

        [Fact]
        public void GetTierCount_different_tumors_same_tiers()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 0", "tier 0")
            }, 1);

            var tierCounts = cosmicItem.GetTierCounts();
            Assert.Equal(2, tierCounts.Count);
            Assert.Equal(2, tierCounts["tier 0"]);
        }

        [Fact]
        public void GetTierCount_different_tumors_different_tiers()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 0", "tier 1")
            }, 1);

            var tierCounts = cosmicItem.GetTierCounts();
            Assert.Equal(2, tierCounts.Count);
            Assert.Equal(1, tierCounts["tier 0"]);
            Assert.Equal(1, tierCounts["tier 1"]);
        }


        [Fact]
        public void GetJsonString()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("102", "primary histology 0", "primarySite 1", "tier 0"),
                new CosmicItem.CosmicTumor("103", "primary histology 0", "primarySite 0", "tier 0"),
                new CosmicItem.CosmicTumor("104", "primary histology 1", "primarySite 1", "tier 1"),
                new CosmicItem.CosmicTumor("105", "primary histology 1", "primarySite 0", "tier 1"),
                new CosmicItem.CosmicTumor("106", "primary histology 1", "primarySite 1", "tier 1"),
                new CosmicItem.CosmicTumor("107", "primary histology 1", "primarySite 0", "tier 1")
                new CosmicItem.CosmicTumor("108", "primary histology 2", "primarySite 2", "tier 1")
            }, 8);

            Assert.Equal("\"id\":\"rs101\",\"refAllele\":\"A\",\"altAllele\":\"C\",\"gene\":\"GENE0\",\"sampleCount\":8,\"cancerTypesAndCounts\":[{\"cancerType\":\"primary histology 0\",\"count\":3},{\"cancerType\":\"primary histology 1\",\"count\":4},{\"cancerType\":\"primary histology 2\",\"count\":1}],\"cancerSitesAndCounts\":[{\"cancerSite\":\"primarySite 0\",\"count\":4},{\"cancerSite\":\"primarySite 1\",\"count\":3},{\"cancerSite\":\"primarySite 2\",\"count\":1}],\"tiersAndCounts\":[{\"tier\":\"tier 0\",\"count\":4},{\"tier\":\"tier 1\",\"count\":4}]", cosmicItem.GetJsonString());
        }

    }
}
