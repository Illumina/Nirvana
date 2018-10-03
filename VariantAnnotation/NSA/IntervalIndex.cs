using System.Collections.Generic;
using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.NSA
{
    public sealed class IntervalIndex
    {
        private readonly Dictionary<ushort, IntervalChromIndex> _chromIndexDictionary;
        public readonly string JsonKey;
        public readonly ReportFor ReportFor;

        public IntervalIndex(string jsonKey, ReportFor reportFor)
        {
            JsonKey = jsonKey;
            ReportFor = reportFor;
            _chromIndexDictionary = new Dictionary<ushort, IntervalChromIndex>();
        }

        public IntervalIndex(ExtendedBinaryReader reader)
        {
            JsonKey   = reader.ReadAsciiString();
            ReportFor = (ReportFor)reader.ReadByte();
            int count = reader.ReadOptInt32();

            _chromIndexDictionary = new Dictionary<ushort, IntervalChromIndex>(count);

            for (int i = 0; i < count; i++)
            {
                ushort index = reader.ReadOptUInt16();
                _chromIndexDictionary.Add(index, new IntervalChromIndex(reader));
            }

        }

        public (long startLocation, long endLocation) GetLocationRange(ushort chromIndex, int start, int end)
        {
            return !_chromIndexDictionary.TryGetValue(chromIndex, out var intervalChromIndex) ? (-1, -1) : intervalChromIndex.GetLocationRange(start, end);
        }

        public void Add(ISuppIntervalItem siItem, long fileLocation, ushort recordSize)
        {
            long endLocation = fileLocation + recordSize;
            if (_chromIndexDictionary.TryGetValue(siItem.Chromosome.Index, out var chromIndex))
            {
                chromIndex.Add(siItem.Start, siItem.End, fileLocation, endLocation);
            }
            else
            {
                var newChromIndex = new IntervalChromIndex(fileLocation);
                newChromIndex.Add(siItem.Start, siItem.End, fileLocation, endLocation);
                _chromIndexDictionary.Add(siItem.Chromosome.Index, newChromIndex);
            }
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(JsonKey);
            writer.WriteOpt((byte)ReportFor);
            writer.WriteOpt(_chromIndexDictionary.Count);
            foreach (var kvp in _chromIndexDictionary)
            {
                ushort index = kvp.Key;
                var chromIndex = kvp.Value;

                writer.WriteOpt(index);
                chromIndex.Write(writer);
            }
        }

    }
}