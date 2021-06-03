using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.Conversion;

namespace SAUtils.CosmicGeneFusions.IO
{
    public static class CosmicGeneFusionParser
    {
        public const string MissingValue = "NS";

        public static Dictionary<int, HashSet<RawCosmicGeneFusion>> Parse(StreamReader reader)
        {
            var fusionEntries = new List<RawCosmicGeneFusion>();

            // skip the first line
            reader.ReadLine();

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                string[] cols = line.Split('\t');
                if (cols.Length != 32)
                    throw new InvalidDataException($"Expected 32 columns in the COSMIC gene fusions file, but found {cols.Length}");

                string fusionIdString = cols[10];

                // skip entries that are missing the fusion ID
                if (string.IsNullOrEmpty(fusionIdString)) continue;

                int    sampleId          = int.Parse(cols[0]);
                string primarySite       = RemoveUnderlines(cols[2]);
                string siteSubtype1      = RemoveUnderlines(cols[3]);
                string primaryHistology  = RemoveUnderlines(cols[6]);
                string histologySubtype1 = RemoveUnderlines(cols[7]);
                int    fusionId          = int.Parse(fusionIdString);
                string hgvsNotation      = cols[11];
                int    pubMedId          = int.Parse(cols[31]);

                fusionEntries.Add(new RawCosmicGeneFusion(sampleId, fusionId, primarySite, siteSubtype1, primaryHistology, histologySubtype1,
                    hgvsNotation, pubMedId));
            }

            return fusionEntries.GroupByFusionId();
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private static Dictionary<int, HashSet<RawCosmicGeneFusion>> GroupByFusionId(this List<RawCosmicGeneFusion> fusionEntries)
        {
            var fusionIdToEntries = new Dictionary<int, HashSet<RawCosmicGeneFusion>>();

            foreach (RawCosmicGeneFusion fusionEntry in fusionEntries)
            {
                if (!fusionIdToEntries.TryGetValue(fusionEntry.FusionId, out HashSet<RawCosmicGeneFusion> fusionEntrySet))
                {
                    fusionEntrySet                          = new HashSet<RawCosmicGeneFusion>();
                    fusionIdToEntries[fusionEntry.FusionId] = fusionEntrySet;
                }

                fusionEntrySet.Add(fusionEntry);
            }

            return fusionIdToEntries;
        }

        internal static string RemoveUnderlines(string s) => s.Replace('_', ' ');
    }
}