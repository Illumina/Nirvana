using System.IO;

namespace VariantAnnotation.FileHandling.TranscriptCache
{
    public sealed class GlobalCustomHeader : ICustomFileHeader
    {
        private readonly long _vepReleaseTicks;
        public readonly ushort VepVersion;

        public int Size => 10;

        /// <summary>
        /// constructor
        /// </summary>
        public GlobalCustomHeader(long vepReleaseTicks, ushort vepVersion)
        {
            _vepReleaseTicks = vepReleaseTicks;
            VepVersion      = vepVersion;
        }

        public ICustomFileHeader Read(BinaryReader reader)
        {
            var vepReleaseTicks = reader.ReadInt64();
            var vepVersion      = reader.ReadUInt16();
            return new GlobalCustomHeader(vepReleaseTicks, vepVersion);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_vepReleaseTicks);
            writer.Write(VepVersion);
        }
    }
}