using System.Collections.Generic;
using SAUtils.CosmicGeneFusions.Conversion;

namespace SAUtils.CosmicGeneFusions.Utilities
{
    public static class CosmicCountUtilities
    {
        public static CosmicCount[] GetCosmicCounts(this Dictionary<string, int> countDict)
        {
            var counts = new List<CosmicCount>(countDict.Count);
            foreach ((string histology, int count) in countDict) counts.Add(new CosmicCount(histology, count));
            return counts.ToArray();
        }
    }
}