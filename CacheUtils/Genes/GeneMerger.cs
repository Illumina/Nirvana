using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.DataStores;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.Utilities;
using Intervals;

namespace CacheUtils.Genes
{
    public static class GeneMerger
    {
        public static Dictionary<ushort, List<UgaGene>> MergeByHgnc(this IUpdateHgncData data, bool isGrch37)
        {
            data.Logger.Write("- merging RefSeq & Ensembl genes... ");

            var genesByRef       = new Dictionary<ushort, List<MutableGene>>();
            var mergedGenesByRef = new Dictionary<ushort, List<UgaGene>>();

            AddGenes(data.EnsemblGenesByRef, genesByRef);
            AddGenes(data.RefSeqGenesByRef, genesByRef);

            var totalOrphanEntries = 0;
            var totalMergedEntries = 0;

            foreach (var kvp in genesByRef)
            {
                var hgncIdToGenes = kvp.Value.GetMultiValueDict(x => x.HgncId.ToString() + '|' + (x.OnReverseStrand ? 'R' : 'F'));
                (var mergedGenes, int numOrphanEntries, int numMergedEntries) = GetMergedGenes(hgncIdToGenes, isGrch37);

                mergedGenesByRef[kvp.Key] = mergedGenes;

                totalOrphanEntries += numOrphanEntries;
                totalMergedEntries += numMergedEntries;
            }

            data.Logger.WriteLine($"orphans: {totalOrphanEntries}, merged: {totalMergedEntries}");

            return mergedGenesByRef;
        }

        private static void AddGenes(Dictionary<ushort, List<MutableGene>> source,
            IDictionary<ushort, List<MutableGene>> target)
        {
            foreach (var kvp in source)
            {
                if (target.TryGetValue(kvp.Key, out var targetGeneList))
                {
                    targetGeneList.AddRange(kvp.Value);
                }
                else
                {
                    var geneList = new List<MutableGene>();
                    geneList.AddRange(kvp.Value);
                    target[kvp.Key] = geneList;
                }
            }
        }

        private static (List<UgaGene> MergedGenes, int NumOrphanEntries, int NumMergedEntries) GetMergedGenes(
            Dictionary<string, List<MutableGene>> hgncIdToGenes, bool isGrch37)
        {
            var mergedGenes      = new List<UgaGene>();
            var numOrphanEntries = 0;
            var numMergedEntries = 0;

            foreach (var kvp in hgncIdToGenes)
            {
                if (kvp.Key.StartsWith("-1|") || kvp.Value.Count == 1)
                {
                    var convertedGenes = ConvertToUgaGenes(kvp.Value, isGrch37);
                    mergedGenes.AddRange(convertedGenes);
                    numOrphanEntries += convertedGenes.Count;
                    continue;
                }

                if (kvp.Value.Count > 2) throw new InvalidDataException("Found more than two genes when merging Ensembl and RefSeq genes.");
                mergedGenes.Add(GetMergedGene(kvp.Value[0], kvp.Value[1], isGrch37));
                numMergedEntries++;
            }

            return (mergedGenes, numOrphanEntries, numMergedEntries);
        }

        private static List<UgaGene> ConvertToUgaGenes(IEnumerable<MutableGene> genes, bool isGrch37)
        {
            var ugaGenes = new List<UgaGene>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var gene in genes)
            {
                if (gene.GeneId == null) continue;
                ugaGenes.Add(gene.ToUgaGene(isGrch37));
            }

            return ugaGenes;
        }

        private static UgaGene GetMergedGene(MutableGene geneA, MutableGene geneB, bool isGrch37)
        {
            (MutableGene ensemblGene, MutableGene refSeqGene) = geneA.GeneId.StartsWith("ENSG") ? (geneA, geneB) : (geneB, geneA);

            if (ensemblGene.Chromosome.Index != refSeqGene.Chromosome.Index) throw new InvalidDataException($"The two genes are on different chromosomes: {geneA.GeneId} & {geneB.GeneId}");
            if (ensemblGene.OnReverseStrand  != refSeqGene.OnReverseStrand)  throw new InvalidDataException($"Both genes do not have the same orientation: {geneA.GeneId} & {geneB.GeneId}");

            IInterval interval = GetMergedInterval(ensemblGene, refSeqGene);
            (IInterval grch37, IInterval grch38) = isGrch37 ? (interval, null as IInterval) : (null as IInterval, interval);

            return new UgaGene(ensemblGene.Chromosome, grch37, grch38, ensemblGene.OnReverseStrand, refSeqGene.GeneId,
                ensemblGene.GeneId, ensemblGene.Symbol, ensemblGene.HgncId);
        }

        private static IInterval GetMergedInterval(MutableGene geneA, MutableGene geneB) =>
            new Interval(Math.Min(geneA.Start, geneB.Start), Math.Max(geneA.End, geneB.End));
    }
}
