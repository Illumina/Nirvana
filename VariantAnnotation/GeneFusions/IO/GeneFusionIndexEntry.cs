using System;
using IO;

namespace VariantAnnotation.GeneFusions.IO
{
    public readonly struct GeneFusionIndexEntry
    {
        private readonly ulong  _geneKey;
        public readonly  ushort Index;

        public GeneFusionIndexEntry(ulong geneKey, ushort index)
        {
            _geneKey = geneKey;
            Index    = index;
        }

        public int Compare(ulong geneKey)
        {
            if (_geneKey < geneKey) return -1;
            return _geneKey > geneKey ? 1 : 0;
        }

        public static GeneFusionIndexEntry Read(ref ReadOnlySpan<byte> byteSpan)
        {
            ulong  geneKey = SpanBufferBinaryReader.ReadUInt64(ref byteSpan);
            ushort index   = SpanBufferBinaryReader.ReadOptUInt16(ref byteSpan);
            return new GeneFusionIndexEntry(geneKey, index);
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(_geneKey);
            writer.WriteOpt(Index);
        }
    }
}