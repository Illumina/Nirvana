using System.Collections.Generic;
using System.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CosmicItemTests
    {
        [Fact]
        public void GetTissueCount_same_study()
        {
            var cosmicItem = new CosmicItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy>
            {
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}),
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"})
            }, 1);

            var tissueCounts = cosmicItem.GetTissueCounts();
            Assert.Equal(2, tissueCounts.Count());
            Assert.Equal(1, tissueCounts["primarySite 0"]);
            Assert.Equal(1, tissueCounts["site subtype 1"]);
        }

        [Fact]
        public void GetTissueCount_different_studies()
        {
            var cosmicItem = new CosmicItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy>
            {
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 2"}),
                new CosmicItem.CosmicStudy("110", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"})
            }, 1);

            var tissueCounts = cosmicItem.GetTissueCounts();
            Assert.Equal(3, tissueCounts.Count());
            Assert.Equal(2, tissueCounts["primarySite 0"]);
            Assert.Equal(1, tissueCounts["site subtype 1"]);
            Assert.Equal(1, tissueCounts["site subtype 2"]);
        }

        [Fact]
        public void GetCancerTypeCount_same_study()
        {
            var cosmicItem = new CosmicItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy>
            {
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}),
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"})
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(2, cancerTypeCounts.Count());
            Assert.Equal(1, cancerTypeCounts["primary histology 0"]);
            Assert.Equal(1, cancerTypeCounts["histology subtype 1"]);
        }


        [Fact]
        public void GetCancerTypeCount_different_studies()
        {
            var cosmicItem = new CosmicItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy>
            {
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}),
                new CosmicItem.CosmicStudy("101", new []{"primary histology 0", "histology subtype 2"}, new []{"primarySite 0", "site subtype 1"})
            }, 1);

            var cancerTypeCounts = cosmicItem.GetCancerTypeCounts();
            Assert.Equal(3, cancerTypeCounts.Count());
            Assert.Equal(2, cancerTypeCounts["primary histology 0"]);
            Assert.Equal(1, cancerTypeCounts["histology subtype 1"]);
            Assert.Equal(1, cancerTypeCounts["histology subtype 2"]);
        }

        [Fact]
        public void GetJsonString()
        {
            var cosmicItem = new CosmicItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "GENE0", new HashSet<CosmicItem.CosmicStudy>
            {
                new CosmicItem.CosmicStudy("100", new []{"primary histology 0", "histology subtype 1"}, new []{"primarySite 0", "site subtype 1"}),
                new CosmicItem.CosmicStudy("101", new []{"primary histology 0", "histology subtype 2"}, new []{"primarySite 0", "site subtype 1"})
            }, 1);

            Assert.Equal("\"id\":\"rs101\",\"refAllele\":\"A\",\"altAllele\":\"C\",\"gene\":\"GENE0\",\"sampleCount\":1,\"cancerTypes\":[{\"primary histology 0\":2},{\"histology subtype 1\":1},{\"histology subtype 2\":1}],\"tissues\":[{\"primarySite 0\":2},{\"site subtype 1\":2}]", cosmicItem.GetJsonString());
        }

    }
}
