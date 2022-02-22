using System;
using IO;

namespace Cache.Data;

public readonly struct TranslationalSlip : IEquatable<TranslationalSlip>
{
    public readonly int  Position;
    public readonly byte Length;

    public TranslationalSlip(int position, byte length)
    {
        Position = position;
        Length   = length;
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(Position);
        writer.Write(Length);
    }

    public static TranslationalSlip Read(ref ReadOnlySpan<byte> byteSpan)
    {
        int  position = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        byte length   = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        return new TranslationalSlip(position, length);
    }

    public bool Equals(TranslationalSlip other) =>
        Position == other.Position &&
        Length   == other.Length;

    public override int GetHashCode() => HashCode.Combine(Position, Length);
}