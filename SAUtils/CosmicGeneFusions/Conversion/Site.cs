using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.IO;
using SAUtils.CosmicGeneFusions.Utilities;

namespace SAUtils.CosmicGeneFusions.Conversion
{
    public static class Site
    {
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static CosmicCount[] GetCounts(HashSet<RawCosmicGeneFusion> fusionEntries, int numSamples)
        {
            var siteCountDict = new Dictionary<string, int>();
            var totalCount    = 0;

            foreach (RawCosmicGeneFusion fusionEntry in fusionEntries)
            {
                string site = CombineLevels(fusionEntry.PrimarySite, fusionEntry.SiteSubtype1);
                if (site == CosmicGeneFusionParser.MissingValue) continue;

                if (siteCountDict.TryGetValue(site, out int count)) siteCountDict[site] = count + 1;
                else siteCountDict[site]                                                = 1;
                totalCount++;
            }

            // this can be less if we had missing values
            if (totalCount > numSamples) throw new InvalidDataException($"Found more total sites ({totalCount}) than samples ({numSamples}).");

            return siteCountDict.GetCosmicCounts();
        }

        private static string CombineLevels(string primary, string subtype1) =>
            subtype1 != CosmicGeneFusionParser.MissingValue ? $"{primary} ({subtype1})" : primary;
    }
}