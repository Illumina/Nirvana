using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.DataDumperImport.Utilities;
using CacheUtils.Genbank;
using CacheUtils.Genes.Utilities;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public static class TranscriptFilter
    {
        private static readonly MutableTranscriptComparer Comparer = new MutableTranscriptComparer();

        private static void Log(this ILogger logger, string transcriptId, string description) =>
            logger.WriteLine($"{transcriptId}\t{description}");

        public static List<MutableTranscript> PickSpecificTranscript(
            this List<MutableTranscript> transcripts, ILogger logger, string transcriptId)
        {
            if (transcripts.Count == 1) return transcripts;

            List<MutableTranscript> filteredTranscripts;
            string logMessage;

            switch (transcriptId)
            {
                case "NM_001005786":
                    filteredTranscripts = transcripts.Where(transcript => transcript.CdnaMaps[9].Start == 25419007).ToList();
                    logMessage = $"Filtered on exon 9 start: {transcriptId}";
                    break;
                case "NM_001278597":
                case "NM_001278596":
                    filteredTranscripts = transcripts.Where(transcript => transcript.CdnaMaps.Length == 26).ToList();
                    logMessage = $"Filtered on exon count (26): {transcriptId}";
                    break;
                case "NM_016152":
                    filteredTranscripts = transcripts.Where(transcript => transcript.Exons[0].Phase == 0).ToList();
                    logMessage = $"Filtered on exon phase (0): {transcriptId}";
                    break;
                default:
                    return transcripts;
            }

            if (filteredTranscripts.Count == 0) return transcripts;
            logger.Log(transcriptId, logMessage);

            return filteredTranscripts.Unique();
        }

        public static List<MutableTranscript> InvestigateInconsistentCdnaMaps(this List<MutableTranscript> transcripts,
            ILogger logger, string transcriptId)
        {
            var index = 0;
            foreach (var transcript in transcripts)
            {
                string onReverseStrand = transcript.Gene.OnReverseStrand ? "R" : "F";

                if (transcript.Exons.Length != transcript.CdnaMaps.Length)
                {
                    logger.Log(transcriptId, $"Found different exon & cDNA maps counts ({transcript.Exons.Length} vs {transcript.CdnaMaps.Length}) (index: {index}, {onReverseStrand})");
                }

                if (transcript.Exons.Length == transcript.CdnaMaps.Length &&
                    DiffExonsAndCdnaMaps(transcript.Exons, transcript.CdnaMaps))
                {
                    logger.Log(transcriptId, $"Found different start/end coordinates between exons & cDNA maps. (index: {index}, {onReverseStrand})");
                }

                index++;
            }

            return transcripts;
        }

        private static bool DiffExonsAndCdnaMaps(IReadOnlyList<MutableExon> exons,
            IReadOnlyList<MutableTranscriptRegion> cdnaMaps)
        {
            int numExons = exons.Count;

            for (var i = 0; i < numExons; i++)
            {
                var exon    = exons[i];
                var cdnaMap = cdnaMaps[i];
                if (exon.Start != cdnaMap.Start || exon.End != cdnaMap.End) return false;
            }

            return false;
        }

        public static List<MutableTranscript> ChooseEditedTranscripts(
            this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var filteredTranscripts = transcripts.Where(transcript => transcript.RnaEdits != null || transcript.BamEditStatus == "ok").ToList();
            if (filteredTranscripts.Count == 0) return transcripts;

            logger.Log(transcripts[0].Id, "Filtered transcripts without RNA edits or BAM edit status");
            return filteredTranscripts.Unique();
        }

        public static List<MutableTranscript> RemoveFailedTranscripts(
            this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var filteredTranscripts = transcripts.Where(transcript => transcript.BamEditStatus != "failed").ToList();
            if (filteredTranscripts.Count == 0) return transcripts;

            logger.Log(transcripts[0].Id, "Filtered transcripts with failed BAM status.");
            return filteredTranscripts.Unique();
        }

        public static List<MutableTranscript> RemoveTranscriptsWithLowestVersion(
            this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var versionToTranscript = transcripts.GetMultiValueDict(x => x.Version);
            if (versionToTranscript.Count == 1) return transcripts;

            byte maxVersion = versionToTranscript.Keys.Max();
            transcripts.RemoveAll(x => x.Version != maxVersion);

            logger.Log(transcripts[0].Id, "Filtered transcripts with lower versions");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> Unique(this IEnumerable<MutableTranscript> transcripts)
        {
            var set = new HashSet<MutableTranscript>(Comparer);
            foreach (var transcript in transcripts) set.Add(transcript);
            return set.ToList();
        }

        public static List<MutableTranscript> FixCodingRegionCdnaStart(this List<MutableTranscript> transcripts,
            ILogger logger, IReadOnlyDictionary<string, GenbankEntry> idToGenbankEntry, string transcriptId)
        {
            if (transcripts.Count == 1 || idToGenbankEntry == null || !idToGenbankEntry.TryGetValue(transcriptId, out var genbankEntry)) return transcripts;

            var cdnaStartToTranscript = transcripts.GetMultiValueDict(x => x.CodingRegion.CdnaStart);
            if (cdnaStartToTranscript.Count == 1) return transcripts;

            if (!cdnaStartToTranscript.TryGetValue(genbankEntry.CodingRegion.Start, out var filteredTranscripts))
                return transcripts;

            logger.Log(transcripts[0].Id, "Filtered transcripts by coding region cDNA start");
            return filteredTranscripts.Unique();
        }

        public static List<MutableTranscript> FixCodingRegionCdnaEnd(this List<MutableTranscript> transcripts,
            ILogger logger, IReadOnlyDictionary<string, GenbankEntry> idToGenbankEntry, string transcriptId)
        {
            if (transcripts.Count == 1 || idToGenbankEntry == null || !idToGenbankEntry.TryGetValue(transcriptId, out var genbankEntry)) return transcripts;

            var cdnaEndToTranscript = transcripts.GetMultiValueDict(x => x.CodingRegion.CdnaEnd);
            if (cdnaEndToTranscript.Count == 1) return transcripts;

            if (!cdnaEndToTranscript.TryGetValue(genbankEntry.CodingRegion.End, out var filteredTranscripts))
                return transcripts;

            logger.Log(transcripts[0].Id, "Filtered transcripts by coding region cDNA end");
            return filteredTranscripts.Unique();
        }

        public static List<MutableTranscript> FixGeneSymbolSource(this List<MutableTranscript> transcripts,
            ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var symbolSources = transcripts.GetSet(x => x.Gene.SymbolSource);
            if (symbolSources.Count == 1) return transcripts;

            if (symbolSources.Contains(GeneSymbolSource.Unknown)) symbolSources.Remove(GeneSymbolSource.Unknown);
            if (symbolSources.Count != 1) throw new NotImplementedException("Cannot handle multiple gene symbol sources at this time");

            var targetSymbolSource = symbolSources.First();
            foreach (var transcript in transcripts) transcript.Gene.SymbolSource = targetSymbolSource;
            logger.Log(transcripts[0].Id, "Normalized gene symbol source");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> FixBioType(this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var biotypes = transcripts.GetSet(x => x.BioType);
            if (biotypes.Count != 2) return transcripts;

            var biotype = GetDesiredBioType(biotypes);
            if (biotype == BioType.other) return transcripts;

            foreach (var transcript in transcripts) transcript.BioType = biotype;
            logger.Log(transcripts[0].Id, "Normalized biotype");
            return transcripts.Unique();
        }

        private static readonly BioType[] MiscRnaBioTypes =
        {
            BioType.antisense_RNA,
            BioType.miRNA,
            BioType.pseudogene,
            BioType.lncRNA,
            BioType.protein_coding,
            BioType.rRNA,
            BioType.SRP_RNA,
            BioType.vaultRNA,
            BioType.Y_RNA
        };

        private static readonly BioType[] LncRnaBioTypes =
        {
            BioType.antisense_RNA,
            BioType.pseudogene
        };

        private static BioType GetDesiredBioType(ICollection<BioType> biotypes)
        {
            if (biotypes.Contains(BioType.misc_RNA))
            {
                foreach (var biotype in MiscRnaBioTypes)
                    if (biotypes.Contains(biotype))
                        return biotype;
            }

            if (biotypes.Contains(BioType.lncRNA) && LncRnaBioTypes.Any(biotypes.Contains)) return BioType.lncRNA;

            if (biotypes.Contains(BioType.mRNA) && biotypes.Contains(BioType.protein_coding))
                return BioType.protein_coding;

            return BioType.other;
        }

        public static List<MutableTranscript> FixGeneId(this List<MutableTranscript> transcripts, ILogger logger,
            Dictionary<string, GenbankEntry> idToGenbankEntry, string transcriptId)
        {
            if (transcripts.Count == 1 || idToGenbankEntry == null || !idToGenbankEntry.TryGetValue(transcriptId, out var genbankEntry)) return transcripts;

            var geneIds = transcripts.GetSet(x => x.Gene.GeneId);
            if (geneIds.Count == 1) return transcripts;

            if (!geneIds.Contains(genbankEntry.GeneId)) throw new InvalidDataException($"Could not find the Genbank gene ID ({genbankEntry.GeneId}) within the transcripts.");

            foreach (var transcript in transcripts) transcript.Gene.GeneId = genbankEntry.GeneId;
            logger.Log(transcripts[0].Id, "Normalized gene ID");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> UnsupervisedFixGeneId(this List<MutableTranscript> transcripts,
            ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var geneIds = transcripts.GetSet(x => x.Gene.GeneId).ToList();
            if (geneIds.Count == 1) return transcripts;

            string geneId = geneIds[0];
            foreach (var transcript in transcripts) transcript.Gene.GeneId = geneId;
            logger.Log(transcripts[0].Id, "Normalized gene ID (unsupervised)");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> FixGeneSymbols(this List<MutableTranscript> transcripts, ILogger logger,
            Dictionary<string, GenbankEntry> idToGenbankEntry, string transcriptId)
        {
            if (transcripts.Count == 1) return transcripts;

            var symbols = transcripts.GetSet(x => x.Gene.Symbol);
            if (symbols.Count == 1) return transcripts;
            if (symbols.Contains(null)) symbols.Remove(null);

            if (idToGenbankEntry == null || !idToGenbankEntry.TryGetValue(transcriptId, out var genbankEntry))
                return transcripts.UnsupervisedFixGeneSymbols(logger, symbols.ToList());

            if (!symbols.Contains(genbankEntry.Symbol)) return transcripts.UnsupervisedFixGeneSymbols(logger, symbols.ToList());

            foreach (var transcript in transcripts) transcript.Gene.Symbol = genbankEntry.Symbol;
            logger.Log(transcripts[0].Id, "Normalized gene symbol");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> FixCanonical(this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var canonicals = transcripts.GetSet(x => x.IsCanonical);
            if (canonicals.Count == 1) return transcripts;

            foreach (var transcript in transcripts) transcript.IsCanonical = false;
            logger.Log(transcripts[0].Id, "Normalized canonical flag");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> FixHgncId(this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var hgncIds = transcripts.GetSet(x => x.Gene.HgncId);
            if (hgncIds.Count == 1) return transcripts;

            if (hgncIds.Contains(-1)) hgncIds.Remove(-1);
            int hgncId = hgncIds.First();

            foreach (var transcript in transcripts) transcript.Gene.HgncId = hgncId;
            logger.Log(transcripts[0].Id, "Normalized HGNC ID");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> FixGeneStart(this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var geneStarts = transcripts.GetSet(x => x.Gene.Start);
            if (geneStarts.Count == 1) return transcripts;

            var transcriptStarts = transcripts.GetSet(x => x.Start).ToArray();
            if (transcriptStarts.Length > 1) return transcripts;

            int closestStart = GetClosest(geneStarts, transcriptStarts[0]);
            foreach (var transcript in transcripts) transcript.Gene.Start = closestStart;
            logger.Log(transcripts[0].Id, "Normalized gene start");
            return transcripts.Unique();
        }

        public static List<MutableTranscript> FixGeneEnd(this List<MutableTranscript> transcripts, ILogger logger)
        {
            if (transcripts.Count == 1) return transcripts;

            var geneEnds = transcripts.GetSet(x => x.Gene.End);
            if (geneEnds.Count == 1) return transcripts;

            var transcriptEnds = transcripts.GetSet(x => x.End).ToArray();
            if (transcriptEnds.Length > 1) return transcripts;

            int closestEnd = GetClosest(geneEnds, transcriptEnds[0]);
            foreach (var transcript in transcripts) transcript.Gene.End = closestEnd;
            logger.Log(transcripts[0].Id, "Normalized gene end");
            return transcripts.Unique();
        }

        private static List<MutableTranscript> UnsupervisedFixGeneSymbols(this IReadOnlyList<MutableTranscript> transcripts,
            ILogger logger, List<string> symbols)
        {
            var nonLocGeneSymbols = symbols.FindAll(x => !string.IsNullOrEmpty(x) && !x.StartsWith("LOC"));
            string symbol = nonLocGeneSymbols.Count > 0 ? nonLocGeneSymbols[0] : symbols[0];

            foreach (var transcript in transcripts) transcript.Gene.Symbol = symbol;
            logger.Log(transcripts[0].Id, "Normalized gene symbol (unsupervised)");
            return transcripts.Unique();
        }

        private static int GetClosest(IEnumerable<int> values, int targetValue)
        {
            int bestDelta = int.MaxValue;
            int bestValue = -1;

            foreach (int value in values)
            {
                int delta = Math.Abs(value - targetValue);
                if (delta >= bestDelta) continue;

                bestDelta = delta;
                bestValue = value;
            }

            return bestValue;
        }
    }
}
