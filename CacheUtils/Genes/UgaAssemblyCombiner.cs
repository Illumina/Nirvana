using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.Genes.Combiners;
using CacheUtils.Genes.DataStructures;
using CacheUtils.TranscriptCache.Comparers;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.Genes
{
    public static class UgaAssemblyCombiner
    {
        internal static readonly List<UgaGene> EmptyUgaGenes = new List<UgaGene>();

        public static UgaGene[] Combine(Dictionary<ushort, List<UgaGene>> genesByRef37,
            Dictionary<ushort, List<UgaGene>> genesByRef38)
        {
            var referenceIndices = GetReferenceIndices(genesByRef37.Keys, genesByRef38.Keys);
            var combinedGenes    = new List<UgaGene>();

            var combiners = GetCombiners();

            foreach (var refIndex in referenceIndices.OrderBy(x => x))
            {
                var ugaGenesByRef = CombineByReference(GetUgaGenesByRef(genesByRef37, refIndex),
                    GetUgaGenesByRef(genesByRef38, refIndex), combiners);
                combinedGenes.AddRange(ugaGenesByRef);
            }

            return combinedGenes.OrderBy(x => x.Chromosome.Index).ThenBy(x => MinCoordinate(x, y => y.Start))
                .ThenBy(x => MinCoordinate(x, y => y.End)).ToArray();
        }

        private static List<ICombiner> GetCombiners() =>
            new List<ICombiner> {new HgncIdCombiner(), new PartitionCombiner()};

        private static IEnumerable<ushort> GetReferenceIndices(IEnumerable<ushort> keysA, IEnumerable<ushort> keysB)
        {
            var referenceIndices = new HashSet<ushort>();
            foreach (var key in keysA) referenceIndices.Add(key);
            foreach (var key in keysB) referenceIndices.Add(key);
            return referenceIndices.OrderBy(x => x);
        }

        private static IEnumerable<UgaGene> CombineByReference(IEnumerable<UgaGene> uga37, IEnumerable<UgaGene> uga38,
            IEnumerable<ICombiner> combiners)
        {
            var combinedGenes = new List<UgaGene>();

            var remainingUga37 = GetRemainingGenes(uga37);
            var remainingUga38 = GetRemainingGenes(uga38);

            foreach (var combiner in combiners) combiner.Combine(combinedGenes, remainingUga37, remainingUga38);

            if (remainingUga37.Count > 0 || remainingUga38.Count > 0)
                throw new InvalidDataException($"Expected the combiners to handle all genes, but some still remain. GRCh37: {remainingUga37.Count}, GRCh38: {remainingUga38.Count}");

            return combinedGenes;
        }

        private static HashSet<UgaGene> GetRemainingGenes(IEnumerable<UgaGene> genes)
        {
            var comparer = new UgaGeneComparer();
            var geneSet = new HashSet<UgaGene>(comparer);
            foreach (var gene in genes) geneSet.Add(gene);
            return geneSet;
        }

        private static IEnumerable<UgaGene> GetUgaGenesByRef(IReadOnlyDictionary<ushort, List<UgaGene>> refIndexToUgaGenes,
            ushort refIndex) => refIndexToUgaGenes.TryGetValue(refIndex, out var genes) ? genes : EmptyUgaGenes;

        private static int MinCoordinate(UgaGene gene, Func<IInterval, int> coordFunc) => coordFunc(gene.GRCh37 ?? gene.GRCh38);
    }
}
