using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.Utilities;
using CacheUtils.TranscriptCache;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.PredictionCache
{
    public static class PredictionUtilities
    {
        internal static IntervalArray<ITranscript>[] UpdateTranscripts(IEnumerable<ITranscript> transcripts,
            Prediction[] oldSiftPredictions, IEnumerable<Prediction> siftPredictions,
            Prediction[] oldPolyPhenPredictions, IEnumerable<Prediction> polyPhenPredictions, int numRefSeqs)
        {
            var siftDict       = siftPredictions.CreateIndex();
            var polyphenDict   = polyPhenPredictions.CreateIndex();
            var newTranscripts = new List<ITranscript>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var transcript in transcripts)
            {
                int siftIndex     = GetNewIndex(oldSiftPredictions, transcript.SiftIndex, siftDict);
                int polyphenIndex = GetNewIndex(oldPolyPhenPredictions, transcript.PolyPhenIndex, polyphenDict);
                newTranscripts.Add(transcript.UpdatePredictions(siftIndex, polyphenIndex));
            }

            return newTranscripts.ToIntervalArrays(numRefSeqs);
        }

        internal static ITranscript UpdatePredictions(this ITranscript t, int siftIndex, int polyphenIndex)
        {
            return new Transcript(t.Chromosome, t.Start, t.End, t.Id, t.Translation, t.BioType, t.Gene,
                t.TotalExonLength, t.StartExonPhase, t.IsCanonical, t.TranscriptRegions, t.NumExons,
                t.MicroRnas, siftIndex, polyphenIndex, t.Source, t.CdsStartNotFound, t.CdsEndNotFound,
                t.Selenocysteines, t.RnaEdits);
        }

        private static int GetNewIndex(IReadOnlyList<Prediction> oldPredictions, int index,
            IReadOnlyDictionary<Prediction, int> dict)
        {
            if (index == -1) return -1;
            var prediction = oldPredictions[index];
            if (!dict.TryGetValue(prediction, out int newIndex)) throw new InvalidDataException("Unable to find the prediction in the dictionary.");
            return newIndex;
        }
    }
}
