using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.IntermediateIO;
using CacheUtils.Utilities;
using Genome;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.PredictionCache
{
    public sealed class PredictionCacheBuilder
    {
        private readonly ILogger _logger;
        private readonly GenomeAssembly _genomeAssembly;

        public PredictionCacheBuilder(ILogger logger, GenomeAssembly genomeAssembly)
        {
            _logger         = logger;
            _genomeAssembly = genomeAssembly;
        }

        public (PredictionCacheStaging Sift, PredictionCacheStaging PolyPhen) CreatePredictionCaches(
            Dictionary<ushort, List<MutableTranscript>> transcriptsByRefIndex, PredictionReader siftReader,
            PredictionReader polyphenReader, int numRefSeqs)
        {
            _logger.Write("- converting prediction strings... ");

            var siftRoundedPredictionsPerRef     = new RoundedEntryPrediction[numRefSeqs][];
            var polyPhenRoundedPredictionsPerRef = new RoundedEntryPrediction[numRefSeqs][];

            for (ushort refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                var sift     = siftReader.GetPredictionData();
                var polyphen = polyphenReader.GetPredictionData();

                if (sift.Chromosome.Index != refIndex || polyphen.Chromosome.Index != refIndex)
                    throw new InvalidDataException(
                        $"Found mismatch between transcript chromosome index ({refIndex}) and prediction chromosome indices (SIFT: {sift.Chromosome.Index}, PolyPhen: {polyphen.Chromosome.Index}.");

                if (!transcriptsByRefIndex.TryGetValue(refIndex, out var refTranscripts)) continue;

                var (siftPredictions, polyPhenPredictions) = ProcessReference(refTranscripts,
                    sift.TranscriptToPredictionIndex, polyphen.TranscriptToPredictionIndex, sift.PredictionData,
                    polyphen.PredictionData);

                siftRoundedPredictionsPerRef[refIndex]     = siftPredictions;
                polyPhenRoundedPredictionsPerRef[refIndex] = polyPhenPredictions;
            }

            _logger.WriteLine("finished.");

            var siftStaging     = BuildCacheStaging("SIFT", siftRoundedPredictionsPerRef, numRefSeqs);
            var polyPhenStaging = BuildCacheStaging("PolyPhen", polyPhenRoundedPredictionsPerRef, numRefSeqs);

            return (siftStaging, polyPhenStaging);
        }

        private PredictionCacheStaging BuildCacheStaging(string description,
            IReadOnlyList<RoundedEntryPrediction[]> roundedPredictionsPerRef, int numReferenceSeqs)
        {
            _logger.Write($"- calculating {description} LUT... ");
            var (lut, roundedEntryToLutIndex) = CreateLut(roundedPredictionsPerRef);
            _logger.WriteLine($"{lut.Length} entries.");

            _logger.Write($"- converting {description} rounded entries... ");
            var predictionsPerRef = ConvertPredictions(roundedPredictionsPerRef, roundedEntryToLutIndex, lut);
            _logger.WriteLine("finished.");

            var header = CreateHeader(numReferenceSeqs, lut);
            return new PredictionCacheStaging(header, predictionsPerRef);
        }

        private PredictionHeader CreateHeader(int numReferenceSeqs, Prediction.Entry[] lut)
        {
            var customHeader = new PredictionCacheCustomHeader(new IndexEntry[numReferenceSeqs]);
            return new PredictionHeader(HeaderUtilities.GetHeader(Source.None, _genomeAssembly), customHeader, lut);
        }

        private static Prediction[][] ConvertPredictions(IReadOnlyList<RoundedEntryPrediction[]> roundedPredictionsPerRef,
            Dictionary<RoundedEntry, byte> roundedEntryToLutIndex, Prediction.Entry[] lut)
        {
            int numReferenceSeqs  = roundedPredictionsPerRef.Count;
            var predictionsPerRef = new Prediction[numReferenceSeqs][];

            for (var i = 0; i < numReferenceSeqs; i++)
            {
                predictionsPerRef[i] = ConvertReferencePredictions(roundedPredictionsPerRef[i], roundedEntryToLutIndex, lut);
            }

            return predictionsPerRef;
        }

        private static Prediction[] ConvertReferencePredictions(IReadOnlyList<RoundedEntryPrediction> roundedEntryPredictions,
            Dictionary<RoundedEntry, byte> roundedEntryToLutIndex, Prediction.Entry[] lut)
        {
            if (roundedEntryPredictions == null) return null;

            int numPredictions = roundedEntryPredictions.Count;
            var predictions    = new Prediction[numPredictions];

            for (var i = 0; i < numPredictions; i++)
                predictions[i] = roundedEntryPredictions[i].Convert(roundedEntryToLutIndex, lut);

            return predictions;
        }

        private static (Prediction.Entry[] Lut, Dictionary<RoundedEntry, byte> RoundedEntryToLutIndex) CreateLut(
            IEnumerable<RoundedEntryPrediction[]> roundedPredictionsPerRef)
        {
            var scores = new HashSet<RoundedEntry>();

            foreach (var roundedPredictions in roundedPredictionsPerRef)
            {
                if (roundedPredictions == null) continue;

                foreach (var roundedPrediction in roundedPredictions)
                {
                    foreach (var roundedEntry in roundedPrediction.Entries)
                    {
                        if (roundedEntry.Score > 1000) continue;
                        scores.Add(roundedEntry);
                    }
                }
            }

            if (scores.Count > 255) throw new InvalidDataException($"Unable to create lookup table, too many LUT entries: {scores.Count} (max 255).");

            var lut                    = new Prediction.Entry[scores.Count];
            var roundedEntryToLutIndex = new Dictionary<RoundedEntry, byte>();

            var currentIndex = 0;
            foreach (var entry in scores.OrderBy(x => x.EnumIndex).ThenBy(x => x.Score))
            {
                roundedEntryToLutIndex[entry] = (byte)currentIndex;
                lut[currentIndex++] = new Prediction.Entry(entry.Score / 1000.0, entry.EnumIndex);
            }

            return (lut, roundedEntryToLutIndex);
        }

        private static (RoundedEntryPrediction[] Sift, RoundedEntryPrediction[] PolyPhen) ProcessReference(
            IReadOnlyList<MutableTranscript> transcripts, Dictionary<int, int> siftTranscriptToPredictionIndex,
            Dictionary<int, int> polyphenTranscriptToPredictionIndex, string[] siftPredictionData,
            string[] polyphenPredictionData)
        {
            AssignPredictionIndices(transcripts, siftTranscriptToPredictionIndex, polyphenTranscriptToPredictionIndex);

            var siftPredictions     = siftPredictionData.GetRoundedEntryPredictions();
            var polyPhenPredictions = polyphenPredictionData.GetRoundedEntryPredictions();

            return (siftPredictions, polyPhenPredictions);
        }

        private static void AssignPredictionIndices(IReadOnlyList<MutableTranscript> transcripts,
            Dictionary<int, int> siftTranscriptToPredictionIndex,
            Dictionary<int, int> polyphenTranscriptToPredictionIndex)
        {
            foreach (var kvp in siftTranscriptToPredictionIndex)     transcripts[kvp.Key].SiftIndex     = kvp.Value;
            foreach (var kvp in polyphenTranscriptToPredictionIndex) transcripts[kvp.Key].PolyPhenIndex = kvp.Value;
        }
    }
}
