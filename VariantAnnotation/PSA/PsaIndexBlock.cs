using System;
using IO;

namespace VariantAnnotation.PSA;

public sealed class PsaIndexBlock : IComparable<string>
{
    public readonly string GeneName;
    public readonly int    Start;
    public readonly int    End;
    public readonly long   FilePosition;

    public PsaIndexBlock(string geneName, int start, int end, long filePosition)
    {
        GeneName     = geneName;
        Start        = start;
        End          = end;
        FilePosition = filePosition;
    }

    public static PsaIndexBlock Read(ExtendedBinaryReader reader)
    {
        string geneName     = reader.ReadAsciiString();
        int    start        = reader.ReadOptInt32();
        int    end          = reader.ReadOptInt32();
        long   filePosition = reader.ReadOptInt64();

        return new PsaIndexBlock(geneName, start, end, filePosition);
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOptAscii(GeneName);
        writer.WriteOpt(Start);
        writer.WriteOpt(End);
        writer.WriteOpt(FilePosition);
    }

    public int CompareTo(string geneName)
    {
        return string.Compare(GeneName, geneName, StringComparison.Ordinal);
    }
}