using System;
using IO;

namespace Cache.Data;

public sealed record AminoAcidEdit(int Position, char AminoAcid)
{
    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(Position);
        writer.Write((byte) AminoAcid);
    }

    public static AminoAcidEdit Read(ref ReadOnlySpan<byte> byteSpan)
    {
        int position  = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        var aminoAcid = (char) SpanBufferBinaryReader.ReadByte(ref byteSpan);
        return new AminoAcidEdit(position, aminoAcid);
    }
}