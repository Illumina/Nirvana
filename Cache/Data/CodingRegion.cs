using System;
using System.Collections.Generic;
using Cache.Utilities;
using Intervals;
using IO;

namespace Cache.Data;

public sealed class CodingRegion : IEquatable<CodingRegion>, IInterval
{
    public          int Start { get; }
    public          int End   { get; }
    public readonly int CdnaStart;
    public readonly int CdnaEnd;

    public readonly string ProteinId;
    public readonly string ProteinSeq;

    // if the coding region starts with an incomplete codon, this specifies how many cDNA bases to insert
    // at the beginning
    public readonly byte CdsPadding;

    // normally we consider the first position in the coding region to be CDS position 1, but sometimes only part
    // of the coding region is aligned to the genome. CdsOffset captures that offset
    public readonly ushort CdsOffset;

    // if the protein sequence is partially aligned to the genome, this provides the proper offset
    public readonly ushort ProteinOffset;

    // if we encounter non-standard codons that encode for another amino acid, this provides the overrides 
    public readonly AminoAcidEdit[]? AminoAcidEdits;

    // if there is a ribosomal frameshift, this provides the position and location
    public readonly TranslationalSlip? Slip;

    public CodingRegion(int start, int end, int cdnaStart, int cdnaEnd, string proteinId, string proteinSeq,
        byte cdsPadding, ushort cdsOffset, ushort proteinOffset, AminoAcidEdit[]? aminoAcidEdits,
        TranslationalSlip? slip)
    {
        Start          = start;
        End            = end;
        CdnaStart      = cdnaStart;
        CdnaEnd        = cdnaEnd;
        ProteinId      = proteinId;
        ProteinSeq     = proteinSeq;
        CdsPadding     = cdsPadding;
        CdsOffset      = cdsOffset;
        ProteinOffset  = proteinOffset;
        AminoAcidEdits = aminoAcidEdits;
        Slip           = slip;
    }

    public void Write(ExtendedBinaryWriter writer, Dictionary<string, int> proteinSeqIndices)
    {
        writer.WriteOpt(Start);
        writer.WriteOpt(End);
        writer.WriteOpt(CdnaStart);
        writer.WriteOpt(CdnaEnd);

        writer.Write(ProteinId);
        int proteinIndex = OutputUtilities.GetIndex(ProteinSeq, proteinSeqIndices);
        writer.WriteOpt(proteinIndex);

        writer.Write(CdsPadding);
        writer.Write(CdsOffset);
        writer.Write(ProteinOffset);

        bool hasAminoAcidEdits = AminoAcidEdits != null;
        bool hasSlip           = Slip           != null;
        byte flags             = EncodeFlags(hasAminoAcidEdits, hasSlip);
        writer.Write(flags);

        if (hasAminoAcidEdits)
        {
            writer.WriteOpt(AminoAcidEdits!.Length);
            foreach (AminoAcidEdit aminoAcidEdit in AminoAcidEdits) aminoAcidEdit.Write(writer);
        }

        if (hasSlip) Slip!.Value.Write(writer);
    }

    public static CodingRegion Read(ref ReadOnlySpan<byte> byteSpan, string[] proteinSeqs)
    {
        int start     = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int end       = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int cdnaStart = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int cdnaEnd   = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);

        string proteinId    = SpanBufferBinaryReader.ReadUtf8String(ref byteSpan);
        int    proteinIndex = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        string proteinSeq   = proteinSeqs[proteinIndex];

        byte   cdsPadding    = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        ushort cdsOffset     = SpanBufferBinaryReader.ReadUInt16(ref byteSpan);
        ushort proteinOffset = SpanBufferBinaryReader.ReadUInt16(ref byteSpan);

        byte flags = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        (bool hasAminoAcidEdits, bool hasSlip) = DecodeFlags(flags);

        AminoAcidEdit[]?   aminoAcidEdits = hasAminoAcidEdits ? GetAminoAcidEdits(ref byteSpan) : null;
        TranslationalSlip? slip           = hasSlip ? TranslationalSlip.Read(ref byteSpan) : null;

        return new CodingRegion(start, end, cdnaStart, cdnaEnd, proteinId, proteinSeq, cdsPadding, cdsOffset,
            proteinOffset, aminoAcidEdits, slip);
    }

    private static AminoAcidEdit[] GetAminoAcidEdits(ref ReadOnlySpan<byte> byteSpan)
    {
        int numAminoAcidEdits = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        var aminoAcidEdits    = new AminoAcidEdit[numAminoAcidEdits];

        for (var i = 0; i < numAminoAcidEdits; i++) aminoAcidEdits[i] = AminoAcidEdit.Read(ref byteSpan);
        return aminoAcidEdits;
    }

    private static byte EncodeFlags(bool hasAminoAcidEdits, bool hasSlip)
    {
        byte flags                   = 0;
        if (hasAminoAcidEdits) flags |= 0x1;
        if (hasSlip) flags           |= 0x2;
        return flags;
    }

    private static (bool HasAminoAcidEdits, bool HasSlip) DecodeFlags(byte flags)
    {
        bool hasAminoAcidOffset = (flags & 0x1) != 0;
        bool hasSlip            = (flags & 0x2) != 0;
        return (hasAminoAcidOffset, hasSlip);
    }

    // a record would have automatically added equality and hashcode methods, but they can't handle arrays properly
    public bool Equals(CodingRegion? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        bool aminoAcidEditsEqual = EqualityUtilities.ArrayEquals(AminoAcidEdits, other.AminoAcidEdits);

        return Start      == other.Start         &&
            End           == other.End           &&
            CdnaStart     == other.CdnaStart     &&
            CdnaEnd       == other.CdnaEnd       &&
            ProteinId     == other.ProteinId     &&
            ProteinSeq    == other.ProteinSeq    &&
            CdsPadding    == other.CdsPadding    &&
            ProteinOffset == other.ProteinOffset &&
            aminoAcidEditsEqual                  &&
            Nullable.Equals(Slip, other.Slip);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Start);
        hashCode.Add(End);
        hashCode.Add(CdnaStart);
        hashCode.Add(CdnaEnd);
        hashCode.Add(ProteinId);
        hashCode.Add(ProteinSeq);
        hashCode.Add(CdsPadding);
        hashCode.Add(ProteinOffset);
        hashCode.Add(AminoAcidEdits);
        hashCode.Add(Slip);
        return hashCode.ToHashCode();
    }
}