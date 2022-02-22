using System;
using System.Collections.Generic;
using Cache.Data;
using Cache.Utilities;
using Intervals;

namespace Cache.IO;

public sealed class CacheBin : IEquatable<CacheBin>
{
    public readonly byte EarliestTranscriptBin;
    public readonly byte EarliestRegulatoryRegionBin;

    public readonly Gene[]?             Genes;
    public readonly TranscriptRegion[]? TranscriptRegions;
    public readonly string[]?           CdnaSeqs;
    public readonly string[]?           ProteinSeqs;
    public readonly Transcript[]?       Transcripts;
    public readonly RegulatoryRegion[]? RegulatoryRegions;

    // not serialized
    private readonly IntervalArray<Transcript> _transcriptSearch;
    private readonly IntervalArray<RegulatoryRegion> _regulatoryRegionSearch;

    public CacheBin(byte earliestTranscriptBin, byte earliestRegulatoryRegionBin, Gene[]? genes,
        TranscriptRegion[]? transcriptRegions, string[]? cdnaSeqs, string[]? proteinSeqs, Transcript[]? transcripts,
        RegulatoryRegion[]? regulatoryRegions)
    {
        EarliestTranscriptBin       = earliestTranscriptBin;
        EarliestRegulatoryRegionBin = earliestRegulatoryRegionBin;
        Genes                       = genes;
        TranscriptRegions           = transcriptRegions;
        CdnaSeqs                    = cdnaSeqs;
        ProteinSeqs                 = proteinSeqs;
        Transcripts                 = transcripts;
        RegulatoryRegions           = regulatoryRegions;

        _transcriptSearch = new IntervalArray<Transcript>(IntervalUtilities.CreateIntervals(transcripts));
        _regulatoryRegionSearch =
            new IntervalArray<RegulatoryRegion>(IntervalUtilities.CreateIntervals(regulatoryRegions));
    }

    public void AddTranscripts(List<Transcript> transcripts, int begin, int end) =>
        _transcriptSearch.AddOverlappingValues(transcripts, begin, end);

    public void AddRegulatoryRegions(List<RegulatoryRegion> regulatoryRegions, int begin, int end) =>
        _regulatoryRegionSearch.AddOverlappingValues(regulatoryRegions, begin, end);

    public bool Equals(CacheBin? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        bool genesEqual             = EqualityUtilities.ArrayEquals(Genes,             other.Genes);
        bool transcriptRegionsEqual = EqualityUtilities.ArrayEquals(TranscriptRegions, other.TranscriptRegions);
        bool cdnaSeqsEqual          = EqualityUtilities.ArrayEquals(CdnaSeqs,          other.CdnaSeqs);
        bool proteinSeqsEqual       = EqualityUtilities.ArrayEquals(ProteinSeqs,       other.ProteinSeqs);
        bool transcriptsEqual       = EqualityUtilities.ArrayEquals(Transcripts,       other.Transcripts);
        bool regulatoryRegionsEqual = EqualityUtilities.ArrayEquals(RegulatoryRegions, other.RegulatoryRegions);

        return EarliestTranscriptBin       == other.EarliestTranscriptBin       &&
               EarliestRegulatoryRegionBin == other.EarliestRegulatoryRegionBin &&
               genesEqual                                                       &&
               transcriptRegionsEqual                                           &&
               cdnaSeqsEqual                                                    &&
               proteinSeqsEqual                                                 &&
               transcriptsEqual                                                 &&
               regulatoryRegionsEqual;
    }

    public override int GetHashCode() => HashCode.Combine(EarliestTranscriptBin, EarliestRegulatoryRegionBin, Genes,
        TranscriptRegions, CdnaSeqs, ProteinSeqs, Transcripts, RegulatoryRegions);
}