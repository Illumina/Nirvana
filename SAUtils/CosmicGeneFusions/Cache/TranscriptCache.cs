using System.Collections.Generic;
using System.IO;
using Genome;
using Intervals;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Utilities;

namespace SAUtils.CosmicGeneFusions.Cache
{
    public sealed class TranscriptCache
    {
        private readonly Dictionary<string, ITranscript> _idToTranscript;

        public TranscriptCache(Dictionary<string, ITranscript> idToTranscript) => _idToTranscript = idToTranscript;

        public static TranscriptCache Create(Stream stream, IDictionary<ushort, Chromosome> refIndexToChromosome)
        {
            using var           reader    = new TranscriptCacheReader(stream);
            TranscriptCacheData cacheData = reader.Read(refIndexToChromosome);
            return new TranscriptCache(GetTranscriptIdToTranscript(cacheData.TranscriptIntervalArrays));
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        internal static Dictionary<string, ITranscript> GetTranscriptIdToTranscript(IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            var transcriptIdToTranscript = new Dictionary<string, ITranscript>();

            foreach (IntervalArray<ITranscript> refTranscriptIntervals in transcriptIntervalArrays)
            {
                if (refTranscriptIntervals == null) continue;
                
                foreach (Interval<ITranscript> transcriptInterval in refTranscriptIntervals.Array)
                {
                    ITranscript transcript = transcriptInterval.Value;
                    if (transcript.Source != Source.Ensembl) continue;

                    if (!transcriptIdToTranscript.ContainsKey(transcript.Id.WithVersion))
                        transcriptIdToTranscript[transcript.Id.WithVersion] = transcript;

                    if (!transcriptIdToTranscript.ContainsKey(transcript.Id.WithoutVersion))
                        transcriptIdToTranscript[transcript.Id.WithoutVersion] = transcript;
                }
            }

            return transcriptIdToTranscript;
        }

        public (string GeneId, string GeneSymbol) GetGene(string transcriptId)
        {
            string shortTranscriptId = FormatUtilities.SplitVersion(transcriptId).Id;

            return _idToTranscript.TryGetValue(shortTranscriptId, out ITranscript transcript)
                ? (transcript.Gene.EnsemblId.WithoutVersion, transcript.Gene.Symbol)
                : HandleMissingTranscripts(transcriptId);
        }

        // In GRCh38, we're missing some of the transcripts specified by COSMIC. However, it's fine to substitute
        // these transcripts with others belonging to the same gene. These are generally from transcripts that are
        // no longer used.
        internal static (string GeneId, string GeneSymbol) HandleMissingTranscripts(string transcriptId) =>
            transcriptId switch
            {
                "ENST00000646891.1" => ("ENSG00000157764", "BRAF"),
                "ENST00000242365.4" => ("ENSG00000122778", "KIAA1549"),
                "ENST00000311979.3" => ("ENSG00000172660", "TAF15"),
                "ENST00000529193.1" => ("ENSG00000157613", "CREB3L1"),
                "ENST00000312675.4" => ("ENSG00000145012", "LPP"),
                "ENST00000556625.1" => ("ENSG00000258389", "DUX4"),
                _                   => throw new InvalidDataException($"Found an unhandled transcript ID in HandleMissingTranscripts: {transcriptId}")
            };
    }
}