using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.Compression;

namespace GffComparison
{
    sealed class GffComparer
    {
        private readonly Dictionary<string, TranscriptInfo> _transcriptsA;
        private readonly Dictionary<string, TranscriptInfo> _transcriptsB;

        /// <summary>
        /// constructor
        /// </summary>
        public GffComparer(string inputPath, string inputPath2)
        {
            _transcriptsA = LoadTranscripts(inputPath, "A");
            _transcriptsB = LoadTranscripts(inputPath2, "B");
        }

        private static Dictionary<string, TranscriptInfo> LoadTranscripts(string inputPath, string description)
        {
            Console.Write($"- loading {description}... ");

            var transcriptDict = new Dictionary<string, TranscriptInfo>();

            using (var reader = GZipUtilities.GetAppropriateStreamReader(inputPath))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    var cols = line.Split('\t');
                    if (cols.Length != 9) throw new GeneralException($"Expected 9 columns in the GFF file, but found only {cols.Length} in {Path.GetFileName(inputPath)}. Line: [{line}]");

                    var chromosome = cols[0];
                    var entryType = cols[2];
                    if (entryType != "transcript") continue;

                    var infoCols = GetGffKeyValuePairs(cols[8]);

                    var transcriptId = RemoveVersion(infoCols["transcript_id"]);
                    if (transcriptId == null) throw new GeneralException("Could not find the ID for this transcript");

                    // skip LOC and NC_ transcripts
                    if (transcriptId.StartsWith("LOC")) continue;
                    if (transcriptId.StartsWith("NC_")) continue;

                    var geneId = infoCols["gene_id"];
                    if (geneId == null) throw new GeneralException("Could not find a gene ID for this transcript");

                    var geneSymbol = infoCols["gene_name"];
                    if (geneSymbol == null) throw new GeneralException("Could not find a gene symbol for this transcript");

                    var isCanonical = line.Contains("tag \"canonical\";");

                    TranscriptInfo oldInfo;
                    if (transcriptDict.TryGetValue(transcriptId, out oldInfo))
                    {
                        var sameIdAndSymbol = oldInfo.GeneId == geneId && oldInfo.GeneSymbol == geneSymbol;
                        if (!sameIdAndSymbol) throw new GeneralException($"The transcript dictionary already contains an entry for {transcriptId} with a different gene ID or symbol.");

                        var possibleParRegionTranscript = oldInfo.Chromosome == "chrX" && chromosome == "chrY";
                        if (oldInfo.Chromosome != chromosome && !possibleParRegionTranscript) throw new GeneralException($"The transcript dictionary already contains an entry for {transcriptId} on different chromosomes.");
                    }

                    transcriptDict[transcriptId] = new TranscriptInfo
                    {
                        Chromosome  = chromosome,
                        GeneId      = geneId,
                        GeneSymbol  = geneSymbol,
                        IsCanonical = isCanonical
                    };
                }
            }

            Console.WriteLine($"{transcriptDict.Count} transcripts loaded.");

            return transcriptDict;
        }

        private static Dictionary<string, string> GetGffKeyValuePairs(string infoColumn)
        {
            var keyValuePairs = new Dictionary<string, string>();

            foreach (var pairString in Regex.Split(infoColumn, "; "))
            {
                if (string.IsNullOrEmpty(pairString)) continue;

                var cols = pairString.Split(' ');
                if (cols.Length != 2) throw new GeneralException($"Expected 2 values in GFF the key/value pair, but found {cols.Length}. string: [{pairString}]");

                var key = cols[0];
                var value = cols[1].Trim('"');
                keyValuePairs[key] = value;
            }

            return keyValuePairs;
        }

        public void Compare(string outputTsvPath)
        {
            //using (var writer = new StreamWriter(outputTsvPath))
            //{
            //    OutputTranscriptsA(writer);
            //}

            CheckTranscriptIds();
            CheckGeneSymbols();
            CheckGeneIds();
            CheckCanonical();
        }

        private void OutputTranscriptsA(StreamWriter writer)
        {
            foreach (var kvp in _transcriptsA.OrderBy(x => x.Key))
            {
                writer.WriteLine($"{kvp.Key}\t{kvp.Value.GeneId}\t{kvp.Value.GeneSymbol}");
            }
        }

        private void CheckGeneSymbols()
        {
            Console.WriteLine();
            Console.WriteLine("Transcripts with changed gene symbols:");
            Console.WriteLine("======================================");

            int numChanges = 0;
            foreach (var id in GetAllTranscriptIds())
            {
                var geneSymbolA = GetGeneSymbol(id, _transcriptsA);
                var geneSymbolB = GetGeneSymbol(id, _transcriptsB);

                if (geneSymbolA != geneSymbolB)
                {
                    Console.WriteLine($"{id}: [{geneSymbolA}] --> [{geneSymbolB}]");
                    numChanges++;
                }
            }

            Console.WriteLine(numChanges == 0 ? "None." : $"*** {numChanges} changes found. ***");
        }

        private void CheckCanonical()
        {
            Console.WriteLine();
            Console.WriteLine("Transcripts with changed canonical flags:");
            Console.WriteLine("=========================================");

            int numChanges = 0;
            foreach (var id in GetAllTranscriptIds())
            {
                var canonicalA = GetCanonical(id, _transcriptsA);
                var canonicalB = GetCanonical(id, _transcriptsB);

                if (canonicalA != canonicalB)
                {
                    Console.WriteLine($"{id}: [{canonicalA}] --> [{canonicalB}]");
                    numChanges++;
                }
            }

            Console.WriteLine(numChanges == 0 ? "None." : $"*** {numChanges} changes found. ***");
        }

        private void CheckGeneIds()
        {
            Console.WriteLine();
            Console.WriteLine("Transcripts with changed gene IDs:");
            Console.WriteLine("==================================");

            int numChanges = 0;
            foreach (var id in GetAllTranscriptIds())
            {
                var geneIdA = GetGeneId(id, _transcriptsA);
                var geneIdB = GetGeneId(id, _transcriptsB);

                if (geneIdA != geneIdB)
                {
                    Console.WriteLine($"{id}: [{geneIdA}] --> [{geneIdB}]");
                    numChanges++;
                }
            }

            Console.WriteLine(numChanges == 0 ? "None." : $"*** {numChanges} changes found. ***");
        }

        private static string GetCanonical(string id, Dictionary<string, TranscriptInfo> transcripts)
        {
            TranscriptInfo info;
            if (!transcripts.TryGetValue(id, out info)) return "missing";
            return info.IsCanonical ? "true" : "false";
        }

        private static string GetGeneSymbol(string id, Dictionary<string, TranscriptInfo> transcripts)
        {
            TranscriptInfo info;
            return transcripts.TryGetValue(id, out info) ? info.GeneSymbol : string.Empty;
        }

        private static string GetGeneId(string id, Dictionary<string, TranscriptInfo> transcripts)
        {
            TranscriptInfo info;
            return transcripts.TryGetValue(id, out info) ? info.GeneId : string.Empty;
        }

        private void CheckTranscriptIds()
        {
            var idsA = GetTranscriptIds(_transcriptsA);
            var idsB = GetTranscriptIds(_transcriptsB);

            Console.WriteLine();
            Console.WriteLine("Transcripts in A but not B:");
            Console.WriteLine("===========================");

            var aNotB = new HashSet<string>(idsA);
            aNotB.ExceptWith(idsB);
            foreach (var id in aNotB) Console.WriteLine(id);

            if (aNotB.Count == 0) Console.WriteLine("None.");

            Console.WriteLine();
            Console.WriteLine("Transcripts in B but not A:");
            Console.WriteLine("===========================");

            var bNotA = new HashSet<string>(idsB);
            bNotA.ExceptWith(idsA);
            foreach (var id in bNotA) Console.WriteLine(id);

            if (bNotA.Count == 0) Console.WriteLine("None.");
        }

        private static HashSet<string> GetTranscriptIds(Dictionary<string, TranscriptInfo> transcripts)
        {
            var ids = new HashSet<string>();
            foreach (var id in transcripts.Keys) ids.Add(id);
            return ids;
        }

        private HashSet<string> GetAllTranscriptIds()
        {
            var ids = new HashSet<string>();
            foreach (var id in _transcriptsA.Keys) ids.Add(id);
            foreach (var id in _transcriptsB.Keys) ids.Add(id);
            return ids;
        }

        private static string RemoveVersion(string id)
        {
            if (id == null) return null;
            if (id.StartsWith("NC_")) return id;

            int lastPeriod = id.LastIndexOf('.');
            if (lastPeriod == -1) return id;

            return id.Substring(0, lastPeriod);
        }
    }
}
