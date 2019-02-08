using System.Collections.Generic;
using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.NSA
{
    public sealed class IntervalIndex
    {
        private readonly Dictionary<ushort, IntervalChromIndex> _chromIndexDictionary;
        private readonly string _jsonKey;
        private readonly ReportFor _reportFor;

        public IntervalIndex(string jsonKey, ReportFor reportFor)
        {
            _jsonKey              = jsonKey;
            _reportFor            = reportFor;
            _chromIndexDictionary = new Dictionary<ushort, IntervalChromIndex>();
        }

        public IntervalIndex(ExtendedBinaryReader reader)
        {
            _jsonKey   = reader.ReadAsciiString();
            _reportFor = (ReportFor)reader.ReadByte();
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
            writer.WriteOptAscii(_jsonKey);
            writer.WriteOpt((byte)_reportFor);
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