using System;
using System.Collections.Generic;
using System.IO;
using Cache.Index;
using Cache.IO;
using UnitTests.TestUtilities;
using Versioning;
using Xunit;

namespace UnitTests.Cache.IO;

public sealed class CacheReaderTests
{
    private readonly ReferenceCache[] _expectedReferenceCaches;
    private readonly byte[]           _cacheData;
    private readonly CacheIndex       _index;

    public CacheReaderTests()
    {
        _expectedReferenceCaches = CacheWriterTests.GetReferenceCaches();

        var version      = new DataSourceVersion("Name", "Description", "1.2.3", DateTime.Now.Ticks);
        var indexBuilder = new CacheIndexBuilder(_expectedReferenceCaches.Length);

        using (var ms = new MemoryStream())
        {
            using (var writer = new CacheWriter(ms, version, indexBuilder, true))
            {
                writer.Write(_expectedReferenceCaches);
            }

            _cacheData = ms.ToArray();
        }

        _index = indexBuilder.Build();
    }

    [Fact]
    public void GetReferenceCaches_ReadEntireCache_ExpectedResults()
    {
        ReferenceCache[] actualReferenceCaches;

        Dictionary<int, string> hgncIdToSymbol = new();

        using var ms = new MemoryStream(_cacheData);
        using (var reader = new CacheReader(ms, ChromosomeUtilities.Chromosomes, hgncIdToSymbol))
        {
            actualReferenceCaches = reader.GetReferenceCaches();
        }

        Assert.Equal(_expectedReferenceCaches, actualReferenceCaches);
    }

    [Fact]
    public void GetReferenceCache_ReadIndividualChromosomes_ExpectedResults()
    {
        Dictionary<int, string> hgncIdToSymbol = new();

        using var ms     = new MemoryStream(_cacheData);
        using var reader = new CacheReader(ms, ChromosomeUtilities.Chromosomes, hgncIdToSymbol);

        long? chr1Position = _index.GetReferencePosition(ChromosomeUtilities.Chr1);
        Assert.NotNull(chr1Position);
        reader.SetPosition(chr1Position.Value);
        var chr1Cache = reader.GetReferenceCache(ChromosomeUtilities.Chr1.Index);
        Assert.Equal(_expectedReferenceCaches[0], chr1Cache);

        long? chr2Position = _index.GetReferencePosition(ChromosomeUtilities.Chr2);
        Assert.Null(chr2Position);

        long? chr3Position = _index.GetReferencePosition(ChromosomeUtilities.Chr3);
        Assert.NotNull(chr3Position);
        reader.SetPosition(chr3Position.Value);
        var chr3Cache = reader.GetReferenceCache(ChromosomeUtilities.Chr3.Index);
        Assert.Equal(_expectedReferenceCaches[2], chr3Cache);
    }

    [Fact]
    public void GetCacheBin_ReadIndividualBins_ExpectedResults()
    {
        Dictionary<int, string> hgncIdToSymbol = new();

        using var ms     = new MemoryStream(_cacheData);
        using var reader = new CacheReader(ms, ChromosomeUtilities.Chromosomes, hgncIdToSymbol);

        var chr3Index = _index.GetIndexReference(ChromosomeUtilities.Chr3);
        Assert.NotNull(chr3Index);

        Assert.Equal(1, chr3Index.MaxBin);

        // read the first bin
        long? position = chr3Index.GetBinPosition(0);
        Assert.NotNull(position);
        Assert.NotNull(_expectedReferenceCaches[2].CacheBins[0]);

        // read the second bin
        position = chr3Index.GetBinPosition(1);
        Assert.NotNull(position);

        reader.SetPosition(position.Value);
        var cacheBin = reader.GetCacheBin(ChromosomeUtilities.Chr3);
        Assert.Equal(_expectedReferenceCaches[2].CacheBins[1], cacheBin);
    }
}