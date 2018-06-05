using System.IO;

namespace VariantAnnotation.IO.Caches
{
    public sealed class TranscriptCacheCustomHeader
    {
        public readonly ushort VepVersion;
        private readonly long _vepReleaseTicks;

        public TranscriptCacheCustomHeader(ushort vepVersion, long vepReleaseTicks)
        {
            VepVersion       = vepVersion;
            _vepReleaseTicks = vepReleaseTicks;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_vepReleaseTicks);
            writer.Write(VepVersion);
        }

        public static TranscriptCacheCustomHeader Read(BinaryReader reader)
        {
            long vepReleaseTicks = reader.ReadInt64();
            ushort vepVersion    = reader.ReadUInt16();
            return new TranscriptCacheCustomHeader(vepVersion, vepReleaseTicks);
        }
    }
}