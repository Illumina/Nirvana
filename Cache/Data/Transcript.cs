using System;
using System.Collections.Generic;
using System.Linq;
using Cache.Utilities;
using Genome;
using Intervals;
using IO;

namespace Cache.Data;

public sealed class Transcript : IEquatable<Transcript>, IInterval
{
    public readonly Chromosome Chromosome;
    public          int        Start { get; }
    public          int        End   { get; }

    public readonly string             Id;
    public readonly BioType            BioType;
    public readonly bool               IsCanonical;
    public readonly Source             Source;
    public readonly Gene               Gene;
    public readonly TranscriptRegion[] TranscriptRegions;
    public readonly string             CdnaSeq;
    public readonly CodingRegion?      CodingRegion;

    public Transcript(Chromosome chromosome, int start, int end, string id, BioType bioType, bool isCanonical,
        Source source, Gene gene, TranscriptRegion[] transcriptRegions, string cdnaSeq, CodingRegion? codingRegion)
    {
        Chromosome        = chromosome;
        Start             = start;
        End               = end;
        Id                = id;
        BioType           = bioType;
        IsCanonical       = isCanonical;
        Source            = source;
        Gene              = gene;
        TranscriptRegions = transcriptRegions;
        CdnaSeq           = cdnaSeq;
        CodingRegion      = codingRegion;
    }

    public void Write(ExtendedBinaryWriter writer, Dictionary<Gene, int> geneIndices,
        Dictionary<TranscriptRegion, int> transcriptRegionIndices, Dictionary<string, int> cdnaSeqIndices,
        Dictionary<string, int> proteinSeqIndices)
    {
        writer.WriteOpt(Start);
        writer.WriteOpt(End);
        writer.Write(Id);

        // gene
        writer.WriteOpt(OutputUtilities.GetIndex(Gene, geneIndices));

        // encoded data
        bool   hasCodingRegion = CodingRegion != null;
        ushort flags           = EncodeFlags(BioType, Source, IsCanonical, hasCodingRegion);
        writer.Write(flags);

        // transcript regions
        writer.WriteOpt(TranscriptRegions.Length);
        foreach (TranscriptRegion transcriptRegion in TranscriptRegions)
            writer.WriteOpt(OutputUtilities.GetIndex(transcriptRegion, transcriptRegionIndices));

        if (hasCodingRegion) CodingRegion!.Write(writer, proteinSeqIndices);

        int cdnaIndex = OutputUtilities.GetIndex(CdnaSeq, cdnaSeqIndices);
        writer.WriteOpt(cdnaIndex);
    }

    public static Transcript Read(ref ReadOnlySpan<byte> byteSpan, Chromosome chromosome, Gene[] genes,
        TranscriptRegion[] transcriptRegions, string[] cdnaSeqs, string[] proteinSeqs)
    {
        int    start = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        int    end   = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        string id    = SpanBufferBinaryReader.ReadUtf8String(ref byteSpan);

        // gene
        int  geneIndex = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        Gene gene      = genes[geneIndex];

        // encoded data
        ushort flags = SpanBufferBinaryReader.ReadUInt16(ref byteSpan);
        (BioType bioType, Source source, bool isCanonical, bool hasCodingRegion) = DecodeFlags(flags);

        // transcript regions
        int numTranscriptRegions = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        var regions              = new TranscriptRegion[numTranscriptRegions];
        for (var i = 0; i < numTranscriptRegions; i++)
        {
            int transcriptRegionIndex = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
            regions[i] = transcriptRegions[transcriptRegionIndex];
        }

        CodingRegion? codingRegion = hasCodingRegion ? CodingRegion.Read(ref byteSpan, proteinSeqs) : null;

        int    cdnaIndex = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        string cdnaSeq   = cdnaSeqs[cdnaIndex];

        return new Transcript(chromosome, start, end, id, bioType, isCanonical, source, gene, regions, cdnaSeq,
            codingRegion);
    }

    private static ushort EncodeFlags(BioType bioType, Source source, bool isCanonical, bool hasCodingRegion)
    {
        // +====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+
        // |\\\\\\\\\\\\\\\\\\\|Cano|CdRg|  Source |                BioType                |
        // +====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+

        var flags = (ushort) ((ushort) source << 8);
        flags |= (ushort) bioType;
        if (hasCodingRegion) flags |= 0x400;
        if (isCanonical) flags     |= 0x800;

        return flags;
    }

    private static (BioType bioType, Source source, bool isCanonical, bool hasCodingRegion) DecodeFlags(ushort flags)
    {
        bool hasCodingRegion = (flags & 0x400) != 0;
        bool isCanonical     = (flags & 0x800) != 0;

        var bioType = (BioType) (flags       & 0xFF);
        var source  = (Source) ((flags >> 8) & 0x3);

        return (bioType, source, isCanonical, hasCodingRegion);
    }

    // a record would have automatically added equality and hashcode methods, but they can't handle arrays properly
    public bool Equals(Transcript? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        bool regionsEqual = TranscriptRegions.SequenceEqual(other.TranscriptRegions);

        bool codingRegionEqual = CodingRegion == null && other.CodingRegion == null ||
            CodingRegion != null && other.CodingRegion != null && CodingRegion.Equals(other.CodingRegion);

        return Start    == other.Start       &&
            End         == other.End         &&
            Id          == other.Id          &&
            BioType     == other.BioType     &&
            IsCanonical == other.IsCanonical &&
            Source      == other.Source      &&
            Gene.Equals(other.Gene)          &&
            regionsEqual                     &&
            CdnaSeq == other.CdnaSeq         &&
            codingRegionEqual;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Start);
        hashCode.Add(End);
        hashCode.Add(Id);
        hashCode.Add((int) BioType);
        hashCode.Add(IsCanonical);
        hashCode.Add((int) Source);
        hashCode.Add(Gene);
        hashCode.Add(TranscriptRegions);
        hashCode.Add(CdnaSeq);
        hashCode.Add(CodingRegion);
        return hashCode.ToHashCode();
    }
}