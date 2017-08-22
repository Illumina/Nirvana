using System.IO;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.IO.Caches
{
    public sealed class PredictionCacheCustomHeader : ICustomCacheHeader
    {
        public readonly IndexEntry[] Entries;

        public PredictionCacheCustomHeader(IndexEntry[] entries)
        {
            Entries = entries;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((ushort)Entries.Length);
            foreach (var entry in Entries) entry.Write(writer);
        }

        public static ICustomCacheHeader Read(BinaryReader reader)
        {
            var numReferenceSeqs = reader.ReadUInt16();
            var entries          = new IndexEntry[numReferenceSeqs];
            for (int i = 0; i < numReferenceSeqs; i++) entries[i].Read(reader);
            return new PredictionCacheCustomHeader(entries);
        }
    }
}