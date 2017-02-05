using System.Collections.Generic;
using CacheUtils.CombineAndUpdateGenes.Algorithms;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.CacheUtils
{
    public class GeneMergerTests
    {
        [Fact]
        // ReSharper disable once InconsistentNaming
        public void CDRT1()
        {
            var genesA = GetCdrt1EnsemblGenes();
            var genesB = GetCdrt1RefSeqGenes();
            var linkedEnsemblIds = new Dictionary<string, string> { ["ENSG00000241322"] = "374286" };

            var merger = new GeneMerger(genesA, genesB, linkedEnsemblIds);
            var mergedGenes = merger.Merge();
            Assert.Equal(3, mergedGenes.Count);

            var mergedGene = mergedGenes[0];
            Assert.Equal(16, mergedGene.ReferenceIndex);
            Assert.Equal(15468797, mergedGene.Start);
            Assert.Equal(15469590, mergedGene.End);
            Assert.Equal("CDRT1", mergedGene.Symbol);
            Assert.Equal(-1, mergedGene.HgncId);
            Assert.True(mergedGene.EntrezGeneId.IsEmpty);
            Assert.Equal("ENSG00000181464", mergedGene.EnsemblId.ToString());
            Assert.True(mergedGene.OnReverseStrand);

            var mergedGene2 = mergedGenes[1];
            Assert.Equal(16, mergedGene2.ReferenceIndex);
            Assert.Equal(15468798, mergedGene2.Start);
            Assert.Equal(15523018, mergedGene2.End);
            Assert.Equal("CDRT1", mergedGene2.Symbol);
            Assert.Equal(14379, mergedGene2.HgncId);
            Assert.Equal("374286", mergedGene2.EntrezGeneId.ToString());
            Assert.Equal("ENSG00000241322", mergedGene2.EnsemblId.ToString());
            Assert.True(mergedGene2.OnReverseStrand);

            var mergedGene3 = mergedGenes[2];
            Assert.Equal(16, mergedGene3.ReferenceIndex);
            Assert.Equal(15474805, mergedGene3.Start);
            Assert.Equal(15554967, mergedGene3.End);
            Assert.Equal("CDRT1", mergedGene3.Symbol);
            Assert.Equal(-1, mergedGene3.HgncId);
            Assert.True(mergedGene3.EntrezGeneId.IsEmpty);
            Assert.Equal("ENSG00000251537", mergedGene3.EnsemblId.ToString());
            Assert.True(mergedGene3.OnReverseStrand);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public void SH3RF3()
        {
            var genesA = GetSh3EnsemblGenes();
            var genesB = GetSh3RefSeqGenes();
            var linkedEnsemblIds = new Dictionary<string, string> { ["ENSG00000172985"] = "344558" };

            var merger = new GeneMerger(genesA, genesB, linkedEnsemblIds);
            var mergedGenes = merger.Merge();
            Assert.Equal(1, mergedGenes.Count);

            var mergedGene = mergedGenes[0];
            Assert.Equal(1, mergedGene.ReferenceIndex);
            Assert.Equal(109745804, mergedGene.Start);
            Assert.Equal(110262207, mergedGene.End);
            Assert.Equal("SH3RF3", mergedGene.Symbol);
            Assert.Equal(24699, mergedGene.HgncId);
            Assert.Equal("344558", mergedGene.EntrezGeneId.ToString());
            Assert.Equal("ENSG00000172985", mergedGene.EnsemblId.ToString());
            Assert.False(mergedGene.OnReverseStrand);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public void SRGAP2C()
        {
            var genesA = GetSrgap2CEnsemblGenes();
            var genesB = GetSrgap2CRefSeqGenes();
            var linkedEnsemblIds = new Dictionary<string, string> { ["ENSG00000171943"] = "653464" };

            var merger = new GeneMerger(genesA, genesB, linkedEnsemblIds);
            var mergedGenes = merger.Merge();
            Assert.Equal(2, mergedGenes.Count);

            var mergedGene = mergedGenes[0];
            Assert.Equal(0, mergedGene.ReferenceIndex);
            Assert.Equal(120835810, mergedGene.Start);
            Assert.Equal(120838261, mergedGene.End);
            Assert.Equal("SRGAP2C", mergedGene.Symbol);
            Assert.Equal(-1, mergedGene.HgncId);
            Assert.Equal("653464", mergedGene.EntrezGeneId.ToString());
            Assert.True(mergedGene.EnsemblId.IsEmpty);
            Assert.True(mergedGene.OnReverseStrand);

            var mergedGene2 = mergedGenes[1];
            Assert.Equal(0, mergedGene2.ReferenceIndex);
            Assert.Equal(121107124, mergedGene2.Start);
            Assert.Equal(121131061, mergedGene2.End);
            Assert.Equal("SRGAP2C", mergedGene2.Symbol);
            Assert.Equal(30584, mergedGene2.HgncId);
            Assert.Equal("653464", mergedGene2.EntrezGeneId.ToString());
            Assert.Equal("ENSG00000171943", mergedGene2.EnsemblId.ToString());
            Assert.False(mergedGene2.OnReverseStrand);
        }

        private List<MutableGene> GetCdrt1RefSeqGenes()
        {
            ushort refIndex = 16;
            var geneId = CompactId.Convert("374286");
            var symbol = "CDRT1";
            var hgncId = -1;
            var transcriptSource = TranscriptDataSource.RefSeq;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 15491977,
                    End                  = 15523018,
                    OnReverseStrand      = true,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EntrezGeneId         = geneId,
                    TranscriptDataSource = transcriptSource
                }
            };
        }

        private List<MutableGene> GetCdrt1EnsemblGenes()
        {
            ushort refIndex = 16;
            var symbol = "CDRT1";
            var transcriptSource = TranscriptDataSource.Ensembl;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 15468797,
                    End                  = 15469590,
                    OnReverseStrand      = true,
                    Symbol               = symbol,
                    HgncId               = -1,
                    EnsemblId            = CompactId.Convert("ENSG00000181464"),
                    TranscriptDataSource = transcriptSource
                },
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 15468798,
                    End                  = 15522826,
                    OnReverseStrand      = true,
                    Symbol               = symbol,
                    HgncId               = 14379,
                    EnsemblId            = CompactId.Convert("ENSG00000241322"),
                    TranscriptDataSource = transcriptSource
                },
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 15474805,
                    End                  = 15554967,
                    OnReverseStrand      = true,
                    Symbol               = symbol,
                    HgncId               = -1,
                    EnsemblId            = CompactId.Convert("ENSG00000251537"),
                    TranscriptDataSource = transcriptSource
                }
            };
        }

        private List<MutableGene> GetSrgap2CRefSeqGenes()
        {
            ushort refIndex = 0;
            var geneId = CompactId.Convert("653464");
            var symbol = "SRGAP2C";
            var hgncId = -1;
            var transcriptSource = TranscriptDataSource.RefSeq;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 120835810,
                    End                  = 120838261,
                    OnReverseStrand      = true,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EntrezGeneId         = geneId,
                    TranscriptDataSource = transcriptSource
                },
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 121107154,
                    End                  = 121131061,
                    OnReverseStrand      = false,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EntrezGeneId         = geneId,
                    TranscriptDataSource = transcriptSource
                }
            };
        }

        private List<MutableGene> GetSrgap2CEnsemblGenes()
        {
            ushort refIndex = 0;
            var geneId = CompactId.Convert("ENSG00000171943");
            var symbol = "SRGAP2C";
            var hgncId = 30584;
            var transcriptSource = TranscriptDataSource.Ensembl;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 121107124,
                    End                  = 121129949,
                    OnReverseStrand      = false,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EnsemblId            = geneId,
                    TranscriptDataSource = transcriptSource
                }
            };
        }

        private static List<MutableGene> GetSh3RefSeqGenes()
        {
            ushort refIndex = 1;
            var geneId = CompactId.Convert("344558");
            var symbol = "SH3RF3";
            var hgncId = -1;
            var transcriptSource = TranscriptDataSource.RefSeq;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 109745997,
                    End                  = 110107395,
                    OnReverseStrand      = false,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EntrezGeneId         = geneId,
                    TranscriptDataSource = transcriptSource
                },
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 110259067,
                    End                  = 110262207,
                    OnReverseStrand      = false,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EntrezGeneId         = geneId,
                    TranscriptDataSource = transcriptSource
                }
            };
        }

        private static List<MutableGene> GetSh3EnsemblGenes()
        {
            ushort refIndex = 1;
            var geneId = CompactId.Convert("ENSG00000172985");
            var symbol = "SH3RF3";
            var hgncId = 24699;
            var transcriptSource = TranscriptDataSource.Ensembl;

            return new List<MutableGene>
            {
                new MutableGene
                {
                    ReferenceIndex       = refIndex,
                    Start                = 109745804,
                    End                  = 110262207,
                    OnReverseStrand      = false,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EnsemblId            = geneId,
                    TranscriptDataSource = transcriptSource
                }
            };
        }
    }
}
