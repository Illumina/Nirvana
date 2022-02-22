using IO;

namespace Cache.Index;

public readonly struct BinPosition
{
    public readonly byte Bin;
    public readonly long Position;

    public BinPosition(byte bin, long position)
    {
        Bin      = bin;
        Position = position;
    }

    public void Write(ExtendedBinaryWriter writer, ref long prevPosition)
    {
        writer.Write(Bin);
        long delta = Position - prevPosition;
        prevPosition = Position;
        writer.WriteOpt(delta);
    }

    public static BinPosition Read(ExtendedBinaryReader reader, ref long prevPosition)
    {
        byte bin      = reader.ReadByte();
        long delta    = reader.ReadOptInt64();
        long position = prevPosition + delta;
        prevPosition = position;
        return new BinPosition(bin, position);
    }
}