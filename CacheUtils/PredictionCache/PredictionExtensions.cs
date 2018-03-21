using System;
using System.IO;

namespace CacheUtils.PredictionCache
{
    public static class PredictionExtensions
    {
        public static RoundedEntryPrediction[] GetRoundedEntryPredictions(this string[] predictionStrings)
        {
            var predictions = new RoundedEntryPrediction[predictionStrings.Length];
            var currentIndex = 0;
            foreach (string s in predictionStrings) predictions[currentIndex++] = s.GetRoundedEntryPrediction();
            return predictions;
        }

        private static RoundedEntryPrediction GetRoundedEntryPrediction(this string predictionString)
        {
            // convert the base 64 string representation to our compressed prediction data
            var uncompressedDataWithHeader = Convert.FromBase64String(predictionString);
            const int headerLength = 3;

            // skip the 'VEP' header
            int newLength = uncompressedDataWithHeader.Length - headerLength;

            // sanity check: we should have an even number of bytes
            if ((newLength & 1) != 0)
            {
                throw new InvalidDataException($"Expected an even number of bytes when serializing the protein function prediction matrix: {newLength}");
            }

            var data = new ushort[newLength / 2];
            Buffer.BlockCopy(uncompressedDataWithHeader, headerLength, data, 0, newLength);

            var roundedEntries = new RoundedEntry[data.Length];
            for (var i = 0; i < data.Length; i++) roundedEntries[i] = new RoundedEntry(data[i]);
            return new RoundedEntryPrediction(roundedEntries);
        }
    }
}
