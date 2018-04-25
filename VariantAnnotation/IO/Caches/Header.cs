using System.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.IO.Caches
{
    public class Header
    {
        public readonly string Identifier;
        public readonly ushort SchemaVersion;
        public readonly ushort DataVersion;
        public readonly Source Source;
        public readonly long CreationTimeTicks;
        public readonly GenomeAssembly GenomeAssembly;

        public Header(string identifier, ushort schemaVersion, ushort dataVersion, Source source,
            long creationTimeTicks, GenomeAssembly genomeAssembly)
        {
            Identifier        = identifier;
            SchemaVersion     = schemaVersion;
            DataVersion       = dataVersion;
            Source            = source;
            CreationTimeTicks = creationTimeTicks;
            GenomeAssembly    = genomeAssembly;
        }

        protected void Write(BinaryWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(SchemaVersion);
            writer.Write(DataVersion);
            writer.Write((byte)Source);
            writer.Write(CreationTimeTicks);
            writer.Write((byte)GenomeAssembly);
        }

        protected static Header Read(BinaryReader reader)
        {
            string identifier      = reader.ReadString();
            ushort schemaVersion   = reader.ReadUInt16();
            ushort dataVersion     = reader.ReadUInt16();
            var source             = (Source)reader.ReadByte();
            long creationTimeTicks = reader.ReadInt64();
            var genomeAssembly     = (GenomeAssembly)reader.ReadByte();

            return new Header(identifier, schemaVersion, dataVersion, source, creationTimeTicks, genomeAssembly);
        }
    }
}
