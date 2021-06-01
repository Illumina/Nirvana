using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.IO;
using SAUtils.CosmicGeneFusions.Utilities;

namespace SAUtils.CosmicGeneFusions.Conversion
{
    public static class Histology
    {
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static CosmicCount[] GetCounts(HashSet<RawCosmicGeneFusion> fusionEntries, int numSamples)
        {
            var histologyCountDict = new Dictionary<string, int>();
            var totalCount         = 0;

            foreach (RawCosmicGeneFusion fusionEntry in fusionEntries)
            {
                string histology = GetMostSpecificValue(fusionEntry.PrimaryHistology, fusionEntry.HistologySubtype1);
                if (histology == CosmicGeneFusionParser.MissingValue) continue;

                if (histologyCountDict.TryGetValue(histology, out int count)) histologyCountDict[histology] = count + 1;
                else histologyCountDict[histology]                                                          = 1;
                totalCount++;
            }

            if (totalCount != numSamples)
            {
                throw new InvalidDataException($"Found different histology count total ({totalCount}) than samples ({numSamples}).");
            }
            
            return histologyCountDict.GetCosmicCounts();
        }

        private static string GetMostSpecificValue(string primary, string subtype1) =>
            subtype1 != CosmicGeneFusionParser.MissingValue ? subtype1 : primary;
    }
}