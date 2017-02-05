using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.VEP;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Utilities
{
    public sealed class RegulatoryRegionMerger
    {
        public void Merge(ImportDataStore originalDataStore, ImportDataStore mergedDataStore, FeatureStatistics statistics)
        {
            var regulatoryDict = GetMergedRegulatoryRegions(originalDataStore);
            mergedDataStore.RegulatoryFeatures.AddRange(regulatoryDict.Values.ToList());
            statistics.Increment(mergedDataStore.RegulatoryFeatures.Count, originalDataStore.RegulatoryFeatures.Count);
        }

        private static Dictionary<string, RegulatoryFeature> GetMergedRegulatoryRegions(ImportDataStore other)
        {
            var regulatoryDict = new Dictionary<string, RegulatoryFeature>();

            foreach (var regulatoryFeature in other.RegulatoryFeatures)
            {
                // skip regulatory elements without identifiers
                if (string.IsNullOrEmpty(regulatoryFeature.Id)) continue;

                // merge regulatory regions
                var regulatoryKey = $"{regulatoryFeature.Id}.{regulatoryFeature.Start}.{regulatoryFeature.End}";
                RegulatoryFeature prevRegulatoryFeature;

                if (regulatoryDict.TryGetValue(regulatoryKey, out prevRegulatoryFeature))
                {
                    MergeRegulatoryRegion(prevRegulatoryFeature, regulatoryFeature);
                }
                else
                {
                    regulatoryDict[regulatoryKey] = regulatoryFeature;
                }
            }

            return regulatoryDict;
        }

        private static void MergeRegulatoryRegion(RegulatoryFeature prev, RegulatoryFeature curr)
        {
            if (RegulatoryRegionEquals(prev, curr)) return;

            RegulatoryRegionDump(prev);
            RegulatoryRegionDump(curr);
            throw new GeneralException("Found different regulatory regions");
        }

        private static void RegulatoryRegionDump(RegulatoryFeature r)
        {
            Console.WriteLine("==================================");
            Console.WriteLine($"ReferenceIndex:     {r.ReferenceIndex}");
            Console.WriteLine($"Start:              {r.Start}");
            Console.WriteLine($"End:                {r.End}");
            Console.WriteLine($"Id:                 {r.Id}");
            Console.WriteLine($"FeatureType:        {r.FeatureType}");
            Console.WriteLine("==================================");
        }

        private static bool RegulatoryRegionEquals(RegulatoryFeature prev, RegulatoryFeature curr)
        {
            return prev.ReferenceIndex == curr.ReferenceIndex &&
                   prev.Start          == curr.Start          &&
                   prev.End            == curr.End            &&
                   prev.Id             == curr.Id             &&
                   prev.FeatureType    == curr.FeatureType;
        }
    }
}
