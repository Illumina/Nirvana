using System;
using System.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.IO.Caches
{
    public sealed class CacheHeader : IFileHeader
    {
        private readonly string _identifier;
        public readonly ushort SchemaVersion;
        public readonly ushort DataVersion;
        public readonly Source TranscriptSource;
        public readonly long CreationTimeTicks;
        public readonly GenomeAssembly GenomeAssembly;
        public readonly ICustomCacheHeader CustomHeader;

        public CacheHeader(string identifier, ushort schemaVersion, ushort dataVersion, Source transcriptSource,
            long creationTimeTicks, GenomeAssembly genomeAssembly, ICustomCacheHeader customHeader)
        {
            _identifier       = identifier;
            SchemaVersion     = schemaVersion;
            DataVersion       = dataVersion;
            TranscriptSource  = transcriptSource;
            CreationTimeTicks = creationTimeTicks;
            GenomeAssembly    = genomeAssembly;
            CustomHeader      = customHeader;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_identifier);
            writer.Write(SchemaVersion);
            writer.Write(DataVersion);
            writer.Write((byte)TranscriptSource);
            writer.Write(CreationTimeTicks);
            writer.Write((byte)GenomeAssembly);
            CustomHeader.Write(writer);
        }

        public static IFileHeader Read(BinaryReader reader, Func<BinaryReader, ICustomCacheHeader> customRead)
        {
            var identifier        = reader.ReadString();
            var schemaVersion     = reader.ReadUInt16();
            var dataVersion       = reader.ReadUInt16();
            var transcriptSource  = (Source)reader.ReadByte();
            var creationTimeTicks = reader.ReadInt64();
            var genomeAssembly    = (GenomeAssembly)reader.ReadByte();
            var customHeader      = customRead(reader);

            return new CacheHeader(identifier, schemaVersion, dataVersion, transcriptSource,
                creationTimeTicks, genomeAssembly, customHeader);
        }
    }
}
