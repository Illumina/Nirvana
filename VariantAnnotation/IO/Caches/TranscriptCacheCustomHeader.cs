using System.IO;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.IO.Caches
{
    public sealed class TranscriptCacheCustomHeader : ICustomCacheHeader
    {
        public readonly ushort VepVersion;
        public readonly long VepReleaseTicks;

        public TranscriptCacheCustomHeader(ushort vepVersion, long vepReleaseTicks)
        {
            VepVersion       = vepVersion;
            VepReleaseTicks = vepReleaseTicks;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(VepReleaseTicks);
            writer.Write(VepVersion);
        }

        public static ICustomCacheHeader Read(BinaryReader reader)
        {
            var vepReleaseTicks = reader.ReadInt64();
            var vepVersion      = reader.ReadUInt16();
            return new TranscriptCacheCustomHeader(vepVersion, vepReleaseTicks);
        }
    }
}