using System;
using Cache.IO;
using Cache.Utilities;
using Intervals;
using IO;

namespace Cache.Data;

public sealed class TranscriptRegion : IInterval, IEquatable<TranscriptRegion>, IWritable
{
    public int Start { get; }
    public int End   { get; }

    public readonly int                  CdnaStart;
    public readonly int                  CdnaEnd;
    public readonly TranscriptRegionType Type;
    public readonly ushort               Id;
    public readonly CigarOp[]?           CigarOps;

    public TranscriptRegion(int start, int end, int cdnaStart, int cdnaEnd, TranscriptRegionType type, ushort id,
        CigarOp[]? cigarOps)
    {
        Start     = start;
        End       = end;
        CdnaStart = cdnaStart;
        CdnaEnd   = cdnaEnd;
        Type      = type;
        Id        = id;
        CigarOps  = cigarOps;
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(Start);
        writer.WriteOpt(End);
        writer.WriteOpt(CdnaStart);
        writer.WriteOpt(CdnaEnd);
        writer.WriteOpt(Id);

        bool hasCigarOps = CigarOps != null;
        byte flags       = EncodeFlags(Type, hasCigarOps);
        writer.Write(flags);

        if (!hasCigarOps) return;
        writer.Write((byte) CigarOps!.Length);
        foreach (CigarOp cigarOp in CigarOps) cigarOp.Write(writer);
    }

    public static TranscriptRegion Read(ref ReadOnlySpan<byte> byteSpan)
    {
        int    start     = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int    end       = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int    cdnaStart = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int    cdnaEnd   = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        ushort id        = SpanBufferBinaryReader.ReadOptUInt16(ref byteSpan);

        byte flags = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        (TranscriptRegionType type, bool hasCigarOps) = DecodeFlags(flags);

        if (!hasCigarOps) return new TranscriptRegion(start, end, cdnaStart, cdnaEnd, type, id, null);

        int numCigarOps = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        var cigarOps    = new CigarOp[numCigarOps];

        for (var i = 0; i < numCigarOps; i++) cigarOps[i] = CigarOp.Read(ref byteSpan);
        return new TranscriptRegion(start, end, cdnaStart, cdnaEnd, type, id, cigarOps);
    }

    private static byte EncodeFlags(TranscriptRegionType type, bool hasCigarOps)
    {
        // +====+====+====+====+====+====+====+====+
        // |////|////|////|////|////|Cigr|   Type  |
        // +====+====+====+====+====+====+====+====+

        var flags              = (byte) type;
        if (hasCigarOps) flags |= 0x4;
        return flags;
    }

    private static (TranscriptRegionType Type, bool HasCigarOps) DecodeFlags(byte flags)
    {
        bool hasCigarOps = (flags                        & 0x4) != 0;
        var  type        = (TranscriptRegionType) (flags & 3);
        return (type, hasCigarOps);
    }

    // a record would have automatically added equality and hashcode methods, but they can't handle arrays properly
    public bool Equals(TranscriptRegion? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        bool cigarOpsEqual = EqualityUtilities.ArrayEquals(CigarOps, other.CigarOps);
        
        return Start     == other.Start     &&
               End       == other.End       &&
               CdnaStart == other.CdnaStart &&
               CdnaEnd   == other.CdnaEnd   &&
               Type      == other.Type      &&
               Id        == other.Id        &&
               cigarOpsEqual;
    }

    public override int GetHashCode() => HashCode.Combine(Start, End, CdnaStart, CdnaEnd, (int) Type, Id, CigarOps);
}

public enum TranscriptRegionType : byte
{
    Exon,
    Intron
}