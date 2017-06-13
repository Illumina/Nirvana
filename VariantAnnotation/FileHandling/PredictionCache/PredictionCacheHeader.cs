using System.IO;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.PredictionCache
{
    public sealed class PredictionCacheHeader : IFileHeader
    {
        private const int InternalSize = 14;
        public int Size { get; }

        public string Identifier { get; }
        public ushort SchemaVersion { get; }
        public ushort DataVersion { get; }
        public TranscriptDataSource TranscriptSource { get; }
        public long CreationTimeTicks { get; }
        public GenomeAssembly GenomeAssembly { get; }
        public ICustomFileHeader Custom => Index;

        public readonly PredictionCustomHeader Index;

        /// <summary>
        /// constructor
        /// </summary>
        private PredictionCacheHeader(string identifier, ushort schemaVersion, ushort dataVersion,
            TranscriptDataSource transcriptSource, long creationTimeTicks, GenomeAssembly genomeAssembly,
            PredictionCustomHeader index)
        {
            Identifier        = identifier;
            SchemaVersion     = schemaVersion;
            DataVersion       = dataVersion;
            TranscriptSource  = transcriptSource;
            CreationTimeTicks = creationTimeTicks;
            GenomeAssembly    = genomeAssembly;
            Index             = index;
            Size              = Identifier.Length + Custom.Size + InternalSize;
        }

        /// <summary>
        /// returns an empty header
        /// </summary>
        public static PredictionCacheHeader GetHeader(long creationTimeTicks, GenomeAssembly genomeAssembly, int numReferenceSeqs)
        {
            return new PredictionCacheHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, TranscriptDataSource.BothRefSeqAndEnsembl, creationTimeTicks, genomeAssembly,
                new PredictionCustomHeader(new IndexEntry[numReferenceSeqs]));
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
            var header            = (PredictionCustomHeader)Index.Read(reader);

            return new PredictionCacheHeader(identifier, schemaVersion, dataVersion, transcriptSource, creationTimeTicks,
                genomeAssembly, header);
        }
    }

    public sealed class PredictionCustomHeader : ICustomFileHeader
    {
        public readonly IndexEntry[] Entries;
        public int Size { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public PredictionCustomHeader(IndexEntry[] entries)
        {
            Entries = entries;
            Size = entries?.Length ?? 0;
        }

        public ICustomFileHeader Read(BinaryReader reader)
        {
            var numReferenceSeqs = reader.ReadUInt16();
            var entries = new IndexEntry[numReferenceSeqs];
            for (int i = 0; i < numReferenceSeqs; i++) entries[i].Read(reader);
            return new PredictionCustomHeader(entries);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((ushort)Entries.Length);
            foreach (var entry in Entries) entry.Write(writer);
        }
    }
}
