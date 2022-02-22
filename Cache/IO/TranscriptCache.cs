using System.Collections.Generic;
using System.IO;
using Cache.Data;
using Cache.Utilities;
using Genome;
using Versioning;

namespace Cache.IO;

public sealed class TranscriptCache
{
    private readonly ReferenceCache?[]  _referenceCaches;
    public readonly  IDataSourceVersion DataSourceVersion;

    public TranscriptCache(ReferenceCache?[] referenceCaches, IDataSourceVersion dataSourceVersion)
    {
        _referenceCaches  = referenceCaches;
        DataSourceVersion = dataSourceVersion;
    }

    public static TranscriptCache Read(Stream stream, Chromosome[] chromosomes, Dictionary<int, string> hgncIdToSymbol)
    {
        using var         reader            = new CacheReader(stream, chromosomes, hgncIdToSymbol);
        ReferenceCache?[] referenceCaches   = reader.GetReferenceCaches();
        var               dataSourceVersion = reader.DataSourceVersion;
        return new TranscriptCache(referenceCaches, dataSourceVersion);
    }

    public void AddTranscripts(ushort refIndex, int start, int end, List<Transcript> transcripts)
    {
        ReferenceCache? referenceCache = _referenceCaches[refIndex];
        if (referenceCache == null) return;

        byte     startBin      = BinUtilities.GetBin(start);
        CacheBin startCacheBin = referenceCache.CacheBins[startBin];

        startBin = startCacheBin.EarliestTranscriptBin;

        int endBin = BinUtilities.GetBin(end);

        for (int binIndex = startBin; binIndex <= endBin; binIndex++)
        {
            CacheBin cacheBin = referenceCache.CacheBins[binIndex];
            cacheBin.AddTranscripts(transcripts, start, end);
        }
    }

    public void AddRegulatoryRegions(ushort refIndex, int start, int end, List<RegulatoryRegion> regulatoryRegions)
    {
        ReferenceCache? referenceCache = _referenceCaches[refIndex];
        if (referenceCache == null) return;

        int      startBin      = BinUtilities.GetBin(start);
        CacheBin startCacheBin = referenceCache.CacheBins[startBin];

        startBin = startCacheBin.EarliestRegulatoryRegionBin;

        int endBin = BinUtilities.GetBin(end);

        for (int binIndex = startBin; binIndex <= endBin; binIndex++)
        {
            CacheBin cacheBin = referenceCache.CacheBins[binIndex];
            cacheBin.AddRegulatoryRegions(regulatoryRegions, start, end);
        }
    }
}