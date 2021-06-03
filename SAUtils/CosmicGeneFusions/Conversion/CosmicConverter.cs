using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.CosmicGeneFusions.Cache;

namespace SAUtils.CosmicGeneFusions.Conversion
{
    public static class CosmicConverter
    {
        public static Dictionary<ulong, string[]> Convert(Dictionary<int, HashSet<RawCosmicGeneFusion>> fusionIdToEntries,
            TranscriptCache transcriptCache)
        {
            var fusionKeyToJsonList = new Dictionary<ulong, List<string>>();

            foreach ((int fusionId, HashSet<RawCosmicGeneFusion> fusionEntries) in fusionIdToEntries)
            {
                (ulong fusionKey, string json) = GetCosmicGeneFusion(fusionId, fusionEntries, transcriptCache);
                if (json == null) continue;

                if (!fusionKeyToJsonList.TryGetValue(fusionKey, out List<string> jsonEntries))
                {
                    jsonEntries                    = new List<string>();
                    fusionKeyToJsonList[fusionKey] = jsonEntries;
                }

                jsonEntries.Add(json);
            }

            return fusionKeyToJsonList.ToJsonArray();
        }

        internal static Dictionary<ulong, string[]> ToJsonArray(this Dictionary<ulong, List<string>> geneKeyToJsonList)
        {
            var geneKeyToJson = new Dictionary<ulong, string[]>();

            foreach ((ulong geneKey, List<string> jsonList) in geneKeyToJsonList) geneKeyToJson[geneKey] = jsonList.ToArray();
            return geneKeyToJson;
        }

        internal static (ulong FusionKey, string Json) GetCosmicGeneFusion(int fusionId, HashSet<RawCosmicGeneFusion> fusionEntries,
            TranscriptCache transcriptCache)
        {
            (int[] pubMedIds, int numSamples, string hgvsNotation) = AggregateRawCosmicGeneFusions(fusionEntries);
            if (hgvsNotation == null) return (0, null);

            var           id          = $"COSF{fusionId}";
            CosmicCount[] histologies = Histology.GetCounts(fusionEntries, numSamples);
            CosmicCount[] sites       = Site.GetCounts(fusionEntries, numSamples);

            (string[] geneSymbols, ulong fusionKey) = HgvsRnaParser.GetTranscripts(hgvsNotation, transcriptCache);

            var geneFusion = new CosmicGeneFusion(id, numSamples, geneSymbols, hgvsNotation, histologies, sites, pubMedIds);
            var json       = geneFusion.ToString();

            return (fusionKey, json);
        }

        internal static (int[] PubMedIds, int NumSamples, string HgvsNotation) AggregateRawCosmicGeneFusions(
            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            HashSet<RawCosmicGeneFusion> fusionEntries)
        {
            var sampleIds   = new HashSet<int>();
            var pubMedIds   = new HashSet<int>();
            var hgvsEntries = new HashSet<string>();

            foreach (RawCosmicGeneFusion fusionEntry in fusionEntries)
            {
                pubMedIds.Add(fusionEntry.PubMedId);
                sampleIds.Add(fusionEntry.SampleId);
                hgvsEntries.Add(fusionEntry.HgvsNotation);
            }

            if (hgvsEntries.Count != 1)
                throw new InvalidDataException($"Expected one HGVS entry for the gene fusion, but found {hgvsEntries.Count}");

            string hgvsr = HgvsRnaFixer.Fix(hgvsEntries.First());
            return (pubMedIds.OrderBy(x => x).ToArray(), sampleIds.Count, hgvsr);
        }
    }
}