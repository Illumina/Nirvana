using System.Collections.Generic;
using VariantAnnotation.Caches.DataStructures;

namespace CacheUtils.PredictionCache
{
    public sealed class RoundedEntryPrediction
    {
        public readonly RoundedEntry[] Entries;
        public RoundedEntryPrediction(RoundedEntry[] entries) => Entries = entries;

        public Prediction Convert(Dictionary<RoundedEntry, byte> lutDict, Prediction.Entry[] lut)
        {
            int numEntries = Entries.Length;
            var lutIndices = new byte[numEntries];

            var index = 0;
            foreach (var entry in Entries) lutIndices[index++] = entry.Score > 1000 ? (byte) 255 : lutDict[entry];
            return new Prediction(lutIndices, lut);
        }
    }
}
