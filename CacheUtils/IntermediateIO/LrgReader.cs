using System.Collections.Generic;
using System.IO;
using IO;
using OptimizedCore;
using VariantAnnotation.Utilities;

namespace CacheUtils.IntermediateIO
{
    public static class LrgReader
    {
        private const int RefSeqTranscriptIndex  = 4;
        private const int EnsemblTranscriptIndex = 5;
        private const int CccdsIndex             = 6;

        public static HashSet<string> GetTranscriptIds(string lrgPath, Dictionary<string, List<string>> ccdsIdToEnsemblId)
        {
            var transcriptIds = new HashSet<string>();

            using (var reader = FileUtilities.GetStreamReader(FileUtilities.GetReadStream(lrgPath)))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    if (line.OptimizedStartsWith('#')) continue;

                    var cols = line.OptimizedSplit('\t');
                    if (cols.Length != 7) throw new InvalidDataException($"Expected 7 columns, but found {cols.Length}: [{line}]");

                    var refSeqTranscript    = FormatUtilities.SplitVersion(Sanitize(cols[RefSeqTranscriptIndex]));
                    var ccds                = FormatUtilities.SplitVersion(Sanitize(cols[CccdsIndex]));
                    var ensemblTranscriptIds = GetEnsemblTranscriptIds(ccds.Id, ccdsIdToEnsemblId, Sanitize(cols[EnsemblTranscriptIndex]));

                    if (refSeqTranscript.Id  != null) transcriptIds.Add(refSeqTranscript.Id);
                    // ReSharper disable once InvertIf
                    if (ensemblTranscriptIds != null) foreach (string id in ensemblTranscriptIds) transcriptIds.Add(id);
                }
            }

            return transcriptIds;
        }

        private static List<string> GetEnsemblTranscriptIds(string ccdsId,
            IReadOnlyDictionary<string, List<string>> ccdsIdToEnsemblId, string ensemblId)
        {
            if (!string.IsNullOrEmpty(ensemblId)) return new List<string> { ensemblId };
            if (string.IsNullOrEmpty(ccdsId)) return null;
            return !ccdsIdToEnsemblId.TryGetValue(ccdsId, out var ensemblList) ? null : ensemblList;
        }

        private static string Sanitize(string s) => s == "-" ? null : s;
    }
}
