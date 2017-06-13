using System;
using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures.ProteinFunction;
using ErrorHandling.Exceptions;

namespace CacheUtils.ParseVepCacheDirectory.PredictionConversion
{
    public sealed class TempPrediction
    {
        private readonly ushort _refIndex;
        private readonly RoundedEntry[] _entries;
        private const double MaxValue = 1000.0;

        /// <summary>
        /// constructor
        /// </summary>
        public TempPrediction(ushort refIndex, ushort[] data)
        {
            _refIndex = refIndex;
            _entries = new RoundedEntry[data.Length];
            for (int i = 0; i < data.Length; i++) _entries[i] = new RoundedEntry(data[i]);
        }

        public sealed class RoundedEntry : IEquatable<RoundedEntry>
        {
            public readonly ushort Score;
            public readonly byte EnumIndex;

            public RoundedEntry(ushort data)
            {
                Score = Round((ushort)(data & 0x3ff));
                EnumIndex = (byte)((data & 0xc000) >> 14);
            }

            private static ushort Round(ushort us)
            {
                return (ushort)((ushort)Math.Round(us / 5.0) * 5);
            }

            public bool Equals(RoundedEntry value)
            {
                if (this == null) throw new NullReferenceException();
                if (value == null) return false;
                if (this == value) return true;
                return Score == value.Score && EnumIndex == value.EnumIndex;
            }

            public override int GetHashCode()
            {
                return Score.GetHashCode() ^ EnumIndex.GetHashCode();
            }
        }

        public static RoundedEntry[] CreateLookupTable(List<TempPrediction> predictions)
        {
            var scores = new HashSet<RoundedEntry>();

            foreach (var prediction in predictions)
            {
                foreach (var roundedEntry in prediction._entries)
                {
                    if (roundedEntry.Score > 1000) continue;
                    scores.Add(roundedEntry);
                }
            }

            if (scores.Count > 255) throw new GeneralException($"Unable to create lookup table, too many LUT entries: {scores.Count} (max 255).");

            return scores.OrderBy(x => x.EnumIndex).ThenBy(x => x.Score).ToArray();
        }

        public static Prediction.Entry[] ConvertLookupTable(RoundedEntry[] lut)
        {
            var newLut = new Prediction.Entry[lut.Length];

            for (int i = 0; i < lut.Length; i++)
            {
                var entry = lut[i];
                newLut[i] = new Prediction.Entry(entry.Score / MaxValue, entry.EnumIndex);
            }

            return newLut;
        }

        public static Prediction[][] ConvertMatrices(List<TempPrediction> tempPredictions, RoundedEntry[] tempLut,
            Prediction.Entry[] lut, ushort numReferenceSeqs)
        {
            // create a lookup table from the original rounded entries to our enum index
            var lutDict = new Dictionary<RoundedEntry, byte>();

            for (int i = 0; i < tempLut.Length; i++)
            {
                var entry = tempLut[i];
                lutDict[entry] = (byte)i;
            }

            // build the list
            var predictionList = new List<Prediction>[numReferenceSeqs];
            for (int i = 0; i < numReferenceSeqs; i++) predictionList[i] = new List<Prediction>();

            foreach (var prediction in tempPredictions)
            {
                predictionList[prediction._refIndex].Add(prediction.Convert(lutDict, lut));
            }

            // convert list to an array
            var ret = new Prediction[numReferenceSeqs][];
            for (int i = 0; i < numReferenceSeqs; i++) ret[i] = predictionList[i].ToArray();

            return ret;
        }

        private Prediction Convert(Dictionary<RoundedEntry, byte> lutDict, Prediction.Entry[] newLut)
        {
            var numEntries = _entries.Length;
            var lutIndices = new byte[numEntries];

            // convert the entries into LUT indices
            for (int i = 0; i < numEntries; i++)
            {
                var entry = _entries[i];
                lutIndices[i] = entry.Score > 1000 ? (byte)255 : lutDict[entry];
            }

            return new Prediction(lutIndices, newLut);
        }
    }
}
