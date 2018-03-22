using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genbank;
using CacheUtils.Genes.Utilities;
using VariantAnnotation.Interface;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public static class TranscriptMerger
    {
        /// <summary>
        /// separates the transcripts by ID and clusters the transcripts into overlapping
        /// islands. From there we can resolve differences and return a unique transcript 
        /// for each cluster.
        /// </summary>
        public static List<MutableTranscript> Merge(ILogger logger, IEnumerable<MutableTranscript> transcripts,
            Dictionary<string, GenbankEntry> idToGenbankEntry)
        {
            var idToTranscripts   = transcripts.GetMultiValueDict(x => x.Id + "|" + x.Start + "|" + x.End);
            var mergedTranscripts = idToTranscripts.Select(kvp => Merge(logger, kvp.Value, idToGenbankEntry)).ToList();
            return mergedTranscripts.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        private static MutableTranscript Merge(ILogger logger, IReadOnlyList<MutableTranscript> transcripts,
            Dictionary<string, GenbankEntry> idToGenbankEntry)
        {
            var transcriptId = transcripts[0].Id;

            if (transcripts.Count == 1)
            {
                transcripts.Unique().InvestigateInconsistentCdnaMaps(logger, transcriptId);
                return transcripts[0];
            }

            var filteredTranscripts = transcripts
                .Unique()
                .InvestigateInconsistentCdnaMaps(logger, transcriptId)
                .RemoveFailedTranscripts(logger)
                .ChooseEditedTranscripts(logger)
                .RemoveTranscriptsWithLowestVersion(logger)
                .FixCodingRegionCdnaStart(logger, idToGenbankEntry, transcriptId)
                .FixCodingRegionCdnaEnd(logger, idToGenbankEntry, transcriptId)
                .FixGeneSymbolSource(logger)
                .FixBioType(logger)
                .FixGeneId(logger, idToGenbankEntry, transcriptId)
                .FixCanonical(logger)
                .FixHgncId(logger)
                .FixGeneStart(logger)
                .FixGeneEnd(logger)
                .FixGeneSymbols(logger, idToGenbankEntry, transcriptId)
                .UnsupervisedFixGeneId(logger)
                .PickSpecificTranscript(logger, transcriptId);

            if (filteredTranscripts.Count == 1) return filteredTranscripts[0];
            throw new NotImplementedException($"Could not merge down to one transcript: {filteredTranscripts.Count} transcripts ({transcriptId})");
        }
    }
}
