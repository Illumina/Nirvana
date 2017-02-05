using System.Collections.Generic;
using CacheUtils.CombineAndUpdateGenes.Algorithms;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.CacheUtils
{
    public class GeneFlattenerTests
    {
        [Fact]
        public void Ensembl()
        {
            var genes = GetCdrt1EnsemblGenes();
            var flattener = new GeneFlattener(genes, "test", false);
            var flatGenes = flattener.Flatten();
            Assert.Equal(1, flatGenes.Count);

            var flatGene = flatGenes[0];
            Assert.Equal(15468798, flatGene.Start);
            Assert.Equal(15522826, flatGene.End);
        }

        private static List<MutableGene> GetCdrt1EnsemblGenes()
        {
            ushort refIndex = 16;
            var geneId      = CompactId.Convert("ENSG00000241322");
            var symbol      = "CDRT1";
            var hgncId      = 14379;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex  = refIndex,
                    Start           = 15468798,
                    End             = 15496722,
                    OnReverseStrand = true,
                    Symbol          = symbol,
                    HgncId          = hgncId,
                    EnsemblId       = geneId
                },
                new MutableGene
                {
                    ReferenceIndex  = refIndex,
                    Start           = 15491702,
                    End             = 15502111,
                    OnReverseStrand = true,
                    Symbol          = symbol,
                    HgncId          = hgncId,
                    EnsemblId       = geneId
                },
                new MutableGene
                {
                    ReferenceIndex  = refIndex,
                    Start           = 15491977,
                    End             = 15522826,
                    OnReverseStrand = true,
                    Symbol          = symbol,
                    HgncId          = hgncId,
                    EnsemblId       = geneId
                },
                new MutableGene
                {
                    ReferenceIndex  = refIndex,
                    Start           = 15492216,
                    End             = 15501932,
                    OnReverseStrand = true,
                    Symbol          = symbol,
                    HgncId          = hgncId,
                    EnsemblId       = geneId
                },
                new MutableGene
                {
                    ReferenceIndex  = refIndex,
                    Start           = 15497867,
                    End             = 15499946,
                    OnReverseStrand = true,
                    Symbol          = symbol,
                    HgncId          = hgncId,
                    EnsemblId       = geneId
                },
                new MutableGene
                {
                    ReferenceIndex  = refIndex,
                    Start           = 15508538,
                    End             = 15519008,
                    OnReverseStrand = true,
                    Symbol          = symbol,
                    HgncId          = hgncId,
                    EnsemblId       = geneId
                }
            };
        }
    }
}
