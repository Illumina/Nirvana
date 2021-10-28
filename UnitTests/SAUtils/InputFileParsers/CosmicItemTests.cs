using System.Collections.Generic;
using SAUtils.DataStructures;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CosmicItemTests
    {
        [Fact]
        public void GetCancerSiteCount_same_tumor()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"})
            }, 1);

            var counts = cosmicItem.GetTissueCounts();
            Assert.Equal(2, counts.Count);
            Assert.Equal(1, counts["primarySite 0"]);
            Assert.Equal(1, counts["site subtype 1"]);
        }

        [Fact]
        public void GetTissueCount_different_tumors()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("110", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 2"}, new[]{"tier 0", "tier 1"})
            }, 1);

            var counts = cosmicItem.GetTissueCounts();
            Assert.Equal(3, counts.Count);
            Assert.Equal(2, counts["primarySite 0"]);
            Assert.Equal(1, counts["site subtype 1"]);
            Assert.Equal(1, counts["site subtype 2"]);
        }

        [Fact]
        public void GetCancerTypeCount_same_tumor()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"})
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(2, cancerTypeCounts.Count);
            Assert.Equal(1, cancerTypeCounts["primary histology 0"]);
            Assert.Equal(1, cancerTypeCounts["histology subtype 1"]);
        }


        [Fact]
        public void GetCancerTypeCount_different_tumors()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("101", new []{"primary histology 0", "histology subtype 2"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"})
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(3, cancerTypeCounts.Count);
            Assert.Equal(2, cancerTypeCounts["primary histology 0"]);
            Assert.Equal(1, cancerTypeCounts["histology subtype 1"]);
            Assert.Equal(1, cancerTypeCounts["histology subtype 2"]);
        }

        [Fact]
                public void GetTierCount_same_tumor()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"})
            }, 1);

            var tierCounts = cosmicItem.GetTierCounts();
            Assert.Equal(2, tierCounts.Count);
            Assert.Equal(1, tierCounts["tier 0"]);
            Assert.Equal(1, tierCounts["tier 1"]);
        }


        [Fact]
        public void GetTierCount_different_tumors()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("101", new []{"primary histology 0", "histology subtype 2"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 2"})
            }, 1);

            var tierCounts = cosmicItem.GetTierCounts();
            Assert.Equal(3, tierCounts.Count);
            Assert.Equal(2, tierCounts["tier 0"]);
            Assert.Equal(1, tierCounts["tier 1"]);
            Assert.Equal(1, tierCounts["tier 2"]);
        }

        [Fact]
        public void GetJsonString()
        {
            var cosmicItem = new CosmicItem(ChromosomeUtilities.Chr1, 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicTumor>
            {
                new CosmicItem.CosmicTumor("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"}),
                new CosmicItem.CosmicTumor("101", new []{"primary histology 0", "histology subtype 2"}, new []{"primarySite 0", "site subtype 1"}, new[]{"tier 0", "tier 1"})
            }, 1);

            Assert.Equal("\"id\":\"rs101\",\"refAllele\":\"A\",\"altAllele\":\"C\",\"gene\":\"GENE0\",\"sampleCount\":1,\"cancerTypesAndCounts\":[{\"cancerType\":\"primary histology 0\",\"count\":2},{\"cancerType\":\"histology subtype 1\",\"count\":1},{\"cancerType\":\"histology subtype 2\",\"count\":1}],\"cancerSitesAndCounts\":[{\"cancerSite\":\"primarySite 0\",\"count\":2},{\"cancerSite\":\"site subtype 1\",\"count\":2}],\"tiersAndCounts\":[{\"tier\":\"tier 0\",\"count\":2},{\"tier\":\"tier 1\",\"count\":2}]", cosmicItem.GetJsonString());
        }

    }
}
