using System;
using Cache.IO;
using Cache.Utilities;
using Genome;
using Intervals;
using IO;

namespace Cache.Data;

public sealed class RegulatoryRegion : IEquatable<RegulatoryRegion>, IWritable, IInterval
{
    public readonly Chromosome Chromosome;
    public          int        Start { get; }
    public          int        End   { get; }
    
    public readonly string  Id;
    public readonly BioType BioType;
    public readonly string? Note;
    public readonly int[]?  PubMedIds;
    public readonly int?    EcoId;

    public RegulatoryRegion(Chromosome chromosome, int start, int end, string id, BioType bioType, string? note,
        int[]? pubMedIds, int? ecoId)
    {
        Chromosome = chromosome;
        Start      = start;
        End        = end;
        Id         = id;
        BioType    = bioType;
        Note       = note;
        PubMedIds  = pubMedIds;
        EcoId      = ecoId;
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(Start);
        writer.WriteOpt(End);
        writer.Write((byte) BioType);
        writer.Write(Id);

        // encoded data
        bool hasNote      = Note      != null;
        bool hasPubMedIds = PubMedIds != null;
        bool hasEcoId     = EcoId     != null;
        byte flags        = EncodeFlags(hasNote, hasPubMedIds, hasEcoId);
        writer.Write(flags);

        if (hasNote) writer.Write(Note!);
        if (hasEcoId) writer.WriteOpt(EcoId!.Value);

        if (!hasPubMedIds) return;
        writer.WriteOpt(PubMedIds!.Length);
        foreach (int pubMedId in PubMedIds) writer.WriteOpt(pubMedId);
    }

    public static RegulatoryRegion Read(ref ReadOnlySpan<byte> byteSpan, Chromosome chromosome)
    {
        int    start   = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int    end     = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        var    bioType = (BioType) SpanBufferBinaryReader.ReadByte(ref byteSpan);
        string id      = SpanBufferBinaryReader.ReadUtf8String(ref byteSpan);

        // encoded data
        byte flags = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        (bool hasNote, bool hasPubMedIds, bool hasEcoId) = DecodeFlags(flags);

        string? note  = hasNote ? SpanBufferBinaryReader.ReadUtf8String(ref byteSpan) : null;
        int?    ecoId = hasEcoId ? SpanBufferBinaryReader.ReadOptInt32(ref byteSpan) : null;
        if (!hasPubMedIds) return new RegulatoryRegion(chromosome, start, end, id, bioType, note, null, ecoId);

        int numPubMedIds = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        var pubMedIds    = new int[numPubMedIds];

        for (var i = 0; i < numPubMedIds; i++) pubMedIds[i] = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);

        return new RegulatoryRegion(chromosome, start, end, id, bioType, note, pubMedIds, ecoId);
    }

    private static byte EncodeFlags(bool hasNote, bool hasPubMedIds, bool hasEcoId)
    {
        // +====+====+====+====+====+====+====+====+
        // |////|////|////|////|////|Note|PubM|EcoI|
        // +====+====+====+====+====+====+====+====+

        var flags               = (byte) 0;
        if (hasNote) flags      |= 0x4;
        if (hasPubMedIds) flags |= 0x2;
        if (hasEcoId) flags     |= 0x1;

        return flags;
    }

    private static (bool HasNote, bool HasPubMedIds, bool HasEcoId) DecodeFlags(ushort flags)
    {
        bool hasNote      = (flags & 0x4) != 0;
        bool hasPubMedIds = (flags & 0x2) != 0;
        bool hasEcoId     = (flags & 0x1) != 0;
        return (hasNote, hasPubMedIds, hasEcoId);
    }

    // a record would have automatically added equality and hashcode methods, but they can't handle arrays properly
    public bool Equals(RegulatoryRegion? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        bool pubMedIdsEqual = EqualityUtilities.ArrayEquals(PubMedIds, other.PubMedIds);

        return Start   == other.Start   &&
               End     == other.End     &&
               Id      == other.Id      &&
               BioType == other.BioType &&
               Note    == other.Note    &&
               pubMedIdsEqual           &&
               EcoId == other.EcoId;
    }

    public override int GetHashCode() => HashCode.Combine(Start, End, Id, (int) BioType, Note, PubMedIds, EcoId);
}