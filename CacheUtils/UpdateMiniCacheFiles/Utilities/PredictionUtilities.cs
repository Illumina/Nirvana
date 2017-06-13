using System.Collections.Generic;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.PredictionCache;

namespace CacheUtils.UpdateMiniCacheFiles.Utilities
{
    public static class PredictionUtilities
    {
        public static PredictionCacheStaging GetStaging(DataBundle bundle, bool useSift, List<TranscriptPacket> packets,
            int numRefSeqs)
        {
            var cache = useSift ? bundle.SiftCache : bundle.PolyPhenCache;
            if (cache == null || packets.Count == 0) return null;
            return GetStagingWithPredictionIndices(cache, useSift, packets, numRefSeqs);
        }

        private static PredictionCacheStaging GetStagingWithPredictionIndices(PredictionCache cache, bool useSift,
            List<TranscriptPacket> packets, int numRefSeqs)
        {
            var predictionsPerRef = new Prediction[numRefSeqs][];

            for (ushort refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                predictionsPerRef[refIndex] = GetPredictions(packets, useSift, refIndex);
            }

            var header = PredictionCacheHeader.GetHeader(cache.Header.CreationTimeTicks, cache.Header.GenomeAssembly,
                numRefSeqs);

            return new PredictionCacheStaging(header, cache.LookupTable, predictionsPerRef);
        }

        private static Prediction[] GetPredictions(List<TranscriptPacket> packets, bool useSift, ushort refIndex)
        {
            var predictions = new List<Prediction>();

            int newIndex = 0;
            foreach (var packet in packets)
            {
                if (packet.ReferenceIndex != refIndex) continue;

                var prediction = useSift ? packet.SiftPrediction : packet.PolyPhenPrediction;
                if (prediction == null) continue;

                predictions.Add(prediction);

                if (useSift) packet.NewSiftIndex = newIndex;
                else packet.NewPolyPhenIndex = newIndex;

                newIndex++;
            }

            return predictions.ToArray();
        }

        public static void FixIndices(List<TranscriptPacket> packets)
        {
            foreach (var packet in packets)
            {
                var t = packet.Transcript;
                packet.Transcript = new Transcript(t.ReferenceIndex, t.Start, t.End, t.Id, t.Version, t.Translation,
                    t.BioType, t.Gene, t.TotalExonLength, t.StartExonPhase, t.IsCanonical, t.Introns, t.MicroRnas,
                    t.CdnaMaps, packet.NewSiftIndex, packet.NewPolyPhenIndex, t.TranscriptSource);
            }
        }
    }
}
