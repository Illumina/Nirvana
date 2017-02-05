using System.IO;

namespace VariantAnnotation.FileHandling.PredictionCache
{
    public struct IndexEntry
    {
        public long FileOffset;
        public int Count;

        public void Read(BinaryReader reader)
        {
            FileOffset = reader.ReadInt64();
            Count = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FileOffset);
            writer.Write(Count);
        }
    }
}
