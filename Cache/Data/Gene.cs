using System;
using System.Collections.Generic;
using Cache.IO;
using IO;

namespace Cache.Data;

public sealed record Gene(string? NcbiGeneId, string? EnsemblId, bool OnReverseStrand, int? HgncId) : IWritable
{
    public string? Symbol;

    public static Gene Read(ref ReadOnlySpan<byte> byteSpan, Dictionary<int, string> hgncIdToSymbol)
    {
        string symbol = SpanBufferBinaryReader.ReadUtf8String(ref byteSpan);

        // encoded data
        byte flags = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        (bool hasHgncId, bool onReverseStrand, bool hasNcbiGeneId, bool hasEnsemblId) = DecodeFlags(flags);

        string? ncbiGeneId = hasNcbiGeneId ? SpanBufferBinaryReader.ReadUtf8String(ref byteSpan) : null;
        string? ensemblId  = hasEnsemblId ? SpanBufferBinaryReader.ReadUtf8String(ref byteSpan) : null;
        int?    hgncId     = hasHgncId ? SpanBufferBinaryReader.ReadOptInt32(ref byteSpan) : null;

        if (hgncId != null && hgncIdToSymbol.TryGetValue(hgncId.Value, out string? newSymbol)) symbol = newSymbol;

        return new Gene(ncbiGeneId, ensemblId, onReverseStrand, hgncId) {Symbol = symbol};
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.Write(Symbol!);

        // encoded data
        bool hasNcbiGeneId = NcbiGeneId != null;
        bool hasEnsemblId  = EnsemblId  != null;
        bool hasHgncId     = HgncId     != null;
        byte flags         = EncodeFlags(hasHgncId, OnReverseStrand, hasNcbiGeneId, hasEnsemblId);
        writer.Write(flags);

        if (NcbiGeneId != null) writer.Write(NcbiGeneId);
        if (EnsemblId  != null) writer.Write(EnsemblId);
        if (HgncId     != null) writer.WriteOpt(HgncId.Value);
    }

    private const int HgncIdMask        = 1;
    private const int ReverseStrandMask = 2;
    private const int NcbiGeneIdMask    = 4;
    private const int EnsemblIdMask     = 8;

    private static byte EncodeFlags(bool hasHgncId, bool onReverseStrand, bool hasNcbiGeneId, bool hasEnsemblId)
    {
        byte flags = 0;

        if (hasHgncId) flags       |= HgncIdMask;
        if (onReverseStrand) flags |= ReverseStrandMask;
        if (hasNcbiGeneId) flags   |= NcbiGeneIdMask;
        if (hasEnsemblId) flags    |= EnsemblIdMask;
        return flags;
    }

    private static (bool hasHgncId, bool onReverseStrand, bool hasNcbiGeneId, bool hasEnsemblId) DecodeFlags(byte flags)
    {
        bool hasHgncId       = (flags & HgncIdMask)        != 0;
        bool onReverseStrand = (flags & ReverseStrandMask) != 0;
        bool hasNcbiGeneId   = (flags & NcbiGeneIdMask)    != 0;
        bool hasEnsemblId    = (flags & EnsemblIdMask)     != 0;
        return (hasHgncId, onReverseStrand, hasNcbiGeneId, hasEnsemblId);
    }
}