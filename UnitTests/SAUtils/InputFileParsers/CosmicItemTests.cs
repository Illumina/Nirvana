using System.Collections.Generic;
using SAUtils.DataStructures;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CosmicItemTests
    {
        [Fact]
        public void Tumors_Equal()
        {
            var tumor01 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0");
            var tumor02 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0");
            var tumor03 = new CosmicItem.CosmicTumor("101", "primary histology 0", "primarySite 0", "tier 0");

            var tumor04 = new CosmicItem.CosmicTumor("100", null, null, null);
            var tumor05 = new CosmicItem.CosmicTumor("100", null, null, null);
            var tumor06 = new CosmicItem.CosmicTumor("101", null, null, null);

            var tumor07 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0");
            var tumor08 = new CosmicItem.CosmicTumor("100", "primary histology 1", "primarySite 0", "tier 0");
            var tumor09 = new CosmicItem.CosmicTumor("100", null,                  "primarySite 0", "tier 0");

            var tumor10 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0");
            var tumor11 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 1", "tier 0");
            var tumor12 = new CosmicItem.CosmicTumor("100", "primary histology 0", null,            "tier 0");

            var tumor13 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 0");
            var tumor14 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", "tier 1");
            var tumor15 = new CosmicItem.CosmicTumor("100", "primary histology 0", "primarySite 0", null    );


            Assert.True(tumor01.Equals(tumor02));
            Assert.True(tumor04.Equals(tumor05));

            Assert.False(tumor01.Equals(tumor03));
            Assert.False(tumor01.Equals(tumor04));
            Assert.False(tumor05.Equals(tumor06));
            Assert.False(tumor07.Equals(tumor08));
            Assert.False(tumor07.Equals(tumor09));
            Assert.False(tumor08.Equals(tumor09));
            Assert.False(tumor10.Equals(tumor11));
            Assert.False(tumor10.Equals(tumor12));
            Assert.False(tumor11.Equals(tumor12));
            Assert.False(tumor13.Equals(tumor14));
            Assert.False(tumor13.Equals(tumor15));
            Assert.False(tumor14.Equals(tumor15));
        }

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
                new CosmicItem.CosmicTumor("107", "primary histology 1", "primarySite 0", "tier 1"),
                new CosmicItem.CosmicTumor("108", "primary histology 2", "primarySite 2", "tier 1")
            }, 8);

            Assert.Equal("\"id\":\"rs101\",\"refAllele\":\"A\",\"altAllele\":\"C\",\"gene\":\"GENE0\",\"sampleCount\":8,\"cancerTypesAndCounts\":[{\"cancerType\":\"primary histology 0\",\"count\":3},{\"cancerType\":\"primary histology 1\",\"count\":4},{\"cancerType\":\"primary histology 2\",\"count\":1}],\"cancerSitesAndCounts\":[{\"cancerSite\":\"primarySite 0\",\"count\":4},{\"cancerSite\":\"primarySite 1\",\"count\":3},{\"cancerSite\":\"primarySite 2\",\"count\":1}],\"tiersAndCounts\":[{\"tier\":\"tier 0\",\"count\":4},{\"tier\":\"tier 1\",\"count\":4}]", cosmicItem.GetJsonString());
        }

    }
}
