using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Genes.Utilities;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace CacheUtils.Genes.DataStores
{
    public sealed class Hgnc
    {
        public readonly HgncGene[] HgncGenes;
        public readonly Dictionary<int, string> HgncIdToSymbol;

        private Hgnc(HgncGene[] hgncGenes, Dictionary<int, string> hgncIdToSymbol)
        {
            HgncGenes      = hgncGenes;
            HgncIdToSymbol = hgncIdToSymbol;
        }

        public static Hgnc Create(string filePath, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var hgncGenes      = LoadHgncGenes(FileUtilities.GetReadStream(filePath), refNameToChromosome);
            var hgncIdToSymbol = hgncGenes.GetKeyValueDict(x => x.HgncId, x => x.Symbol);
            return new Hgnc(hgncGenes, hgncIdToSymbol);
        }

        private static HgncGene[] LoadHgncGenes(Stream stream, IDictionary<string, IChromosome> refNameToChromosome)
        {
            HgncGene[] genes;
            using (var reader = new HgncReader(stream, refNameToChromosome)) genes = reader.GetGenes();
            return genes;
        }

        public int AddCoordinates(EnsemblGtf ensemblGtf, RefSeqGff refSeqGff)
        {
            foreach (var hgncGene in HgncGenes)
            {
                var (refSeqGenes, ensemblGene, numMatches) = GetGenes(hgncGene.EntrezGeneId,
                    refSeqGff.EntrezGeneIdToGene, hgncGene.EnsemblId, ensemblGtf.EnsemblIdToGene);

                switch (numMatches)
                {
                    case 0:
                        break;

                    case 1:
                        if (ensemblGene == null) AddCoordinatesFromGene(hgncGene, refSeqGenes[0]);
                        else AddCoordinatesFromGene(hgncGene, ensemblGene);
                        break;

                    default:
                        AddCoordinatesFromMultipleGenes(hgncGene, ensemblGene, refSeqGenes);
                        break;
                }
            }

            return HgncGenes.Count(hgncGene => hgncGene.Start != 1 && hgncGene.End != -1);
        }

        private static void AddCoordinatesFromMultipleGenes(HgncGene hgncGene, EnsemblGene ensemblGene, IEnumerable<RefSeqGene> refSeqGenes)
        {
            if (ensemblGene == null) return;

            AddCoordinatesFromGene(hgncGene, ensemblGene);

            foreach (var refSeqGene in refSeqGenes)
            {
                if (!IntervalUtilities.Overlaps(hgncGene.Start, hgncGene.End, refSeqGene.Start, refSeqGene.End)) continue;
                AddCoordinatesFromGene(hgncGene, refSeqGene);
            }
        }

        private static void AddCoordinatesFromGene<T>(HgncGene hgncGene, IFlatGene<T> flatGene) where T : IFlatGene<T>
        {
            hgncGene.Start = hgncGene.Start == -1 ? flatGene.Start : Math.Min(hgncGene.Start, flatGene.Start);
            hgncGene.End   = hgncGene.End   == -1 ? flatGene.End   : Math.Max(hgncGene.End, flatGene.End);
        }

        private static (List<RefSeqGene> RefSeqGenes, EnsemblGene EnsemblGene, int NumMatches) GetGenes(
            string entrezGeneId, IReadOnlyDictionary<string, List<RefSeqGene>> entrezGeneIdToGene, string ensemblId,
            IReadOnlyDictionary<string, EnsemblGene> ensemblIdToGene)
        {
            var refSeqGenes = GetRefSeqGenes(entrezGeneId, entrezGeneIdToGene);
            var ensemblGene = GetEnsemblGene(ensemblId, ensemblIdToGene);
            int numMatches  = (ensemblGene != null ? 1 : 0) + refSeqGenes.Count;
            return (refSeqGenes, ensemblGene, numMatches);
        }

        public Hgnc Clone()
        {
            var newGenes = new HgncGene[HgncGenes.Length];
            for (var i = 0; i < HgncGenes.Length; i++) newGenes[i] = HgncGenes[i].Clone();
            return new Hgnc(newGenes, HgncIdToSymbol);
        }

        private static EnsemblGene GetEnsemblGene(string ensemblId, IReadOnlyDictionary<string, EnsemblGene> ensemblIdToGene)
        {
            if (string.IsNullOrEmpty(ensemblId)) return null;
            return ensemblIdToGene.TryGetValue(ensemblId, out var ensemblGene) ? ensemblGene : null;
        }

        private static readonly List<RefSeqGene> EmptyList = new List<RefSeqGene>();

        private static List<RefSeqGene> GetRefSeqGenes(string entrezGeneId, IReadOnlyDictionary<string, List<RefSeqGene>> entrezGeneIdToGene)
        {
            if (string.IsNullOrEmpty(entrezGeneId)) return EmptyList;
            return entrezGeneIdToGene.TryGetValue(entrezGeneId, out var geneList) ? geneList : EmptyList;
        }

        public (int NumEntrezGeneIdsRemoved, int NumEnsemblIdsRemoved) RemoveDuplicateEntries()
        {
            int numEntrezGeneIdsRemoved = RemoveDuplicatesByTranscriptSource(HgncGenes, x => x.EntrezGeneId, x => x.EntrezGeneId = null);
            int numEnsemblIdsRemoved    = RemoveDuplicatesByTranscriptSource(HgncGenes, x => x.EnsemblId, x => x.EnsemblId = null);
            return (numEntrezGeneIdsRemoved, numEnsemblIdsRemoved);
        }

        private static int RemoveDuplicatesByTranscriptSource(IEnumerable<HgncGene> newHgncGenes,
            Func<HgncGene, string> idFunc, Action<HgncGene> nullAction)
        {
            var hgncByGeneId = newHgncGenes.GetMultiValueDict(idFunc);
            var numGeneIdsRemoved = 0;

            foreach (var kvp in hgncByGeneId)
            {
                if (kvp.Value.Count == 1) continue;
                foreach (var hgncGene in kvp.Value) nullAction(hgncGene);
                numGeneIdsRemoved++;
            }

            return numGeneIdsRemoved;
        }
    }
}
