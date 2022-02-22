using System;
using IO;

namespace Cache.Data;

public sealed record CigarOp(CigarType Type, int Length)
{
    public void Write(ExtendedBinaryWriter writer)
    {
        writer.Write((byte) Type);
        writer.WriteOpt(Length);
    }

    public static CigarOp Read(ref ReadOnlySpan<byte> byteSpan)
    {
        var type   = (CigarType) SpanBufferBinaryReader.ReadByte(ref byteSpan);
        int length = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        return new CigarOp(type, length);
    }
}

public enum CigarType : byte
{
    Match,     // M
    Insertion, // I
    Deletion   // D
}