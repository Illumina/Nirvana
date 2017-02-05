using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling
{
    public interface IFileHeader
    {
        int Size { get; }
        string Identifier { get; }
        ushort SchemaVersion { get; }
        ushort DataVersion { get; }
        TranscriptDataSource TranscriptSource { get; }
        long CreationTimeTicks { get; }
        GenomeAssembly GenomeAssembly { get; }
        ICustomFileHeader Custom { get; }
        void Write(BinaryWriter writer);
        IFileHeader Read(BinaryReader reader);
    }

    public interface ICustomFileHeader
    {
        int Size { get; }
        void Write(BinaryWriter writer);
        ICustomFileHeader Read(BinaryReader reader);
    }
}
