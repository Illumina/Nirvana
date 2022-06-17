using System;
using IO;

namespace VariantAnnotation.GenericScore;

public interface IScoreEncoder
{
    public ushort      BytesRequired { get; }
    public byte[]      EncodeToBytes(double number);
    public double      DecodeFromBytes(ReadOnlySpan<byte> encodedArray);

    public void Write(ExtendedBinaryWriter writer);
}