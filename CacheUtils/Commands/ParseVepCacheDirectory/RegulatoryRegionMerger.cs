using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.TranscriptCache.Comparers;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public static class RegulatoryRegionMerger
    {
        public static IEnumerable<IRegulatoryRegion> Merge(IEnumerable<IRegulatoryRegion> regulatoryRegions)
        {
            var regulatoryDict = new Dictionary<string, IRegulatoryRegion>();
            var comparer       = new RegulatoryRegionComparer();

            foreach (var currentRegion in regulatoryRegions)
            {
                if (currentRegion.Id.IsEmpty()) throw new InvalidOperationException("Found a regulatory region without an ID.");

                string regulatoryKey = $"{currentRegion.Id}.{currentRegion.Start}.{currentRegion.End}";

                if (regulatoryDict.TryGetValue(regulatoryKey, out var previousRegion))
                {
                    MergeRegulatoryRegion(previousRegion, currentRegion, comparer);
                }
                else
                {
                    regulatoryDict[regulatoryKey] = currentRegion;
                }
            }

            return regulatoryDict.Values.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End)
                .ToList();
        }

        private static void MergeRegulatoryRegion(IRegulatoryRegion previous, IRegulatoryRegion current,
            RegulatoryRegionComparer comparer)
        {
            if (comparer.Equals(previous, current)) return;
            throw new InvalidDataException("Found different regulatory regions");
        }
    }
}
