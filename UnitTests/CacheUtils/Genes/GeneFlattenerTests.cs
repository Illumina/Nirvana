using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes;
using Xunit;

namespace UnitTests.CacheUtils.Genes
{
    public sealed class GeneFlattenerTests
    {
        [Fact]
        public void Flatten_AllGenesShouldBeCombined()
        {
            var genes = new List<MutableGene>
            {
                new MutableGene(null, 100, 120, false, null, GeneSymbolSource.Unknown, "test", -1),
                new MutableGene(null, 110, 115, false, null, GeneSymbolSource.Unknown, "test", -1),
                new MutableGene(null, 120, 130, false, null, GeneSymbolSource.Unknown, "test", -1)
            };

            var flatGenes = GeneFlattener.FlattenWithSameId(genes);

            Assert.Single(flatGenes);

            var flatGene = flatGenes[0];
            Assert.Equal(100, flatGene.Start);
            Assert.Equal(130, flatGene.End);
        }

        [Fact]
        public void Flatten_ReturnSameGene_WhenListHasOneEntry()
        {
            var genes = new List<MutableGene>
            {
                new MutableGene(null, 100, 120, false, null, GeneSymbolSource.Unknown, "test", -1)
            };

            var flatGenes = GeneFlattener.FlattenWithSameId(genes);

            Assert.Single(flatGenes);
            Assert.Equal(genes[0].Start, flatGenes[0].Start);
            Assert.Equal(genes[0].End, flatGenes[0].End);
        }

        [Fact]
        public void Flatten_ReturnNull_WhenInputNull()
        {
            var flatGenes = GeneFlattener.FlattenWithSameId(null as List<MutableGene>);
            Assert.Null(flatGenes);
        }

        [Fact]
        public void Flatten_NoGenesShouldBeCombined()
        {
            var genes = new List<MutableGene>
            {
                new MutableGene(null, 100, 120, false, null, GeneSymbolSource.Unknown, "test", -1),
                new MutableGene(null, 130, 140, false, null, GeneSymbolSource.Unknown, "test", -1),
                new MutableGene(null, 150, 160, false, null, GeneSymbolSource.Unknown, "test", -1)
            };

            var flatGenes = GeneFlattener.FlattenWithSameId(genes);

            Assert.Equal(3, flatGenes.Count);
            for (int i = 0; i < flatGenes.Count; i++)
            {
                Assert.Equal(genes[i].Start, flatGenes[i].Start);
                Assert.Equal(genes[i].End, flatGenes[i].End);
            }
        }
    }
}
