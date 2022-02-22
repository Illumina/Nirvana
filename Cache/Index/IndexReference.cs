using System;
using System.Collections.Generic;
using System.Linq;
using IO;

namespace Cache.Index;

public sealed class IndexReference : IEquatable<IndexReference>
{
    public readonly  ushort        RefIndex;
    public readonly  long          Position;
    private readonly BinPosition[] _binPositions;

    // not serialized
    public readonly  byte                   MaxBin;
    private readonly Dictionary<byte, long> _binToPosition;

    public IndexReference(ushort refIndex, long position, BinPosition[] binPositions)
    {
        RefIndex      = refIndex;
        Position      = position;
        _binPositions = binPositions;
        MaxBin        = _binPositions[^1].Bin;

        _binToPosition = new Dictionary<byte, long>(binPositions.Length);
        foreach (var binPosition in binPositions) _binToPosition[binPosition.Bin] = binPosition.Position;
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(RefIndex);
        writer.WriteOpt(Position);
        writer.WriteOpt(_binPositions.Length);

        long prevPosition = Position;
        foreach (BinPosition binPosition in _binPositions)
        {
            binPosition.Write(writer, ref prevPosition);
        }
    }

    public static IndexReference Read(ExtendedBinaryReader reader)
    {
        ushort refIndex = reader.ReadOptUInt16();
        long   position = reader.ReadOptInt64();
        int    numBins  = reader.ReadOptInt32();

        var  binPositions = new BinPosition[numBins];
        long prevPosition = position;

        for (var i = 0; i < numBins; i++) binPositions[i] = BinPosition.Read(reader, ref prevPosition);

        return new IndexReference(refIndex, position, binPositions);
    }

    public long? GetBinPosition(byte bin) => _binToPosition.TryGetValue(bin, out long position) ? position : null;

    public bool Equals(IndexReference? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return RefIndex == other.RefIndex &&
            Position    == other.Position &&
            _binPositions.SequenceEqual(other._binPositions);
    }

    public override int GetHashCode() => HashCode.Combine(RefIndex, Position, _binPositions);
}