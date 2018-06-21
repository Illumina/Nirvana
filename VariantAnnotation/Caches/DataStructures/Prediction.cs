using System.IO;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Prediction
    {
        private readonly byte[] _data;
        private readonly Entry[] _lut;

        //                                                 A   X  C  D  E  F  G  H  I   X  K  L   M   N   X   P   Q   R   S   T   X   V   W   X   Y   X
        private static readonly int[] AminoAcidIndices = { 0, -1, 1, 2, 3, 4, 5, 6, 7, -1, 8, 9, 10, 11, -1, 12, 13, 14, 15, 16, -1, 17, 18, -1, 19, -1 };

        private const int NumAminoAcids = 20;
        private const byte NullEntry    = 0xff;

        public Prediction(byte[] data, Entry[] lut)
        {
            _data = data;
            _lut  = lut;
        }

        public Entry GetPrediction(char newAminoAcid, int aaPosition)
        {
            // sanity check: skip stop codons
            if (newAminoAcid == AminoAcids.StopCodonChar || newAminoAcid == 'X') return null;

            int index = GetIndex(newAminoAcid, aaPosition);

            // sanity check: skip instances where the data isn't long enough
            if (index >= _data.Length) return null;

            byte entry = _data[index];
            return entry == NullEntry ? null : _lut[entry];
        }

        private static int GetIndex(char newAminoAcid, int aaPosition)
        {
            int asciiIndex = char.ToUpper(newAminoAcid) - 'A';

            // sanity check: make sure the array index is within range
            if (asciiIndex < 0 || asciiIndex >= 26)
            {
                throw new InvalidDataException($"Expected an array index on the interval [0, 25], but observed the following: {asciiIndex} ({newAminoAcid})");
            }

            int aaIndex = AminoAcidIndices[asciiIndex];

            // sanity check: make sure the array index is within range
            if (aaIndex == -1)
            {
                throw new InvalidDataException($"An invalid amino acid was given: {newAminoAcid}");
            }

            return NumAminoAcids * (aaPosition - 1) + aaIndex;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_data.Length);
            writer.Write(_data);
        }

        public static Prediction Read(ExtendedBinaryReader reader, Entry[] lut)
        {
            int numBytes = reader.ReadInt32();
            var data     = reader.ReadBytes(numBytes);
            return new Prediction(data, lut);
        }

        public sealed class Entry
        {
            public readonly double Score;
            public readonly byte EnumIndex;

            public Entry(double score, byte enumIndex)
            {
                Score     = score;
                EnumIndex = enumIndex;
            }

            public static Entry ReadEntry(ExtendedBinaryReader reader)
            {
                double score   = reader.ReadDouble();
                byte enumIndex = reader.ReadByte();
                return new Entry(score, enumIndex);
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(Score);
                writer.Write(EnumIndex);
            }
        }
    }
}