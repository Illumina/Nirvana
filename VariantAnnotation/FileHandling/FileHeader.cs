using System.IO;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling
{
    public sealed class FileHeader : IFileHeader
    {
        private const int InternalSize = 14;
        public int Size { get; }

        public string Identifier { get; }
        public ushort SchemaVersion { get; }
        public ushort DataVersion { get; }
        public TranscriptDataSource TranscriptSource { get; }
        public long CreationTimeTicks { get; }
        public GenomeAssembly GenomeAssembly { get; }
        public ICustomFileHeader Custom { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public FileHeader(string identifier, ushort schemaVersion, ushort dataVersion,
            TranscriptDataSource transcriptSource, long creationTimeTicks, GenomeAssembly genomeAssembly,
            ICustomFileHeader customHeader)
        {
            Identifier        = identifier;
            SchemaVersion     = schemaVersion;
            DataVersion       = dataVersion;
            TranscriptSource  = transcriptSource;
            CreationTimeTicks = creationTimeTicks;
            GenomeAssembly    = genomeAssembly;
            Custom            = customHeader;
            Size              = identifier.Length + customHeader.Size + InternalSize + 1;
        }

        /// <summary>
        /// returns an empty header
        /// </summary>
        public static FileHeader GetHeader(long creationTimeTicks, GenomeAssembly genomeAssembly,
            ICustomFileHeader customFileHeader)
        {
            return new FileHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion, CacheConstants.DataVersion,
                TranscriptDataSource.BothRefSeqAndEnsembl, creationTimeTicks, genomeAssembly, customFileHeader);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(SchemaVersion);
            writer.Write(DataVersion);
            writer.Write((byte)TranscriptSource);
            writer.Write(CreationTimeTicks);
            writer.Write((byte)GenomeAssembly);
            Custom.Write(writer);
        }

        public IFileHeader Read(BinaryReader reader)
        {
            var identifier        = reader.ReadString();
            var schemaVersion     = reader.ReadUInt16();
            var dataVersion       = reader.ReadUInt16();
            var transcriptSource  = (TranscriptDataSource)reader.ReadByte();
            var creationTimeTicks = reader.ReadInt64();
            var genomeAssembly    = (GenomeAssembly)reader.ReadByte();
            var customHeader      = Custom.Read(reader);

            return new FileHeader(identifier, schemaVersion, dataVersion, transcriptSource, creationTimeTicks,
                genomeAssembly, customHeader);
        }
    }
}
