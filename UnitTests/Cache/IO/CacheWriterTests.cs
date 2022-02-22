using System;
using System.Collections.Generic;
using System.IO;
using Cache.Data;
using Cache.IO;
using UnitTests.TestUtilities;
using Versioning;
using Xunit;

namespace UnitTests.Cache.IO;

public sealed class CacheWriterTests
{
    [Fact]
    public void Write_EndToEnd_ExpectedResults()
    {
        ReferenceCache[] expectedReferenceCaches = GetReferenceCaches();
        var              indexBuilder            = new CacheIndexBuilder(3);

        Dictionary<int, string> hgncIdToSymbol = new();

        var expectedVersion = new DataSourceVersion("Name", "Description", "1.2.3", DateTime.Now.Ticks);

        int       expectedFilePairId;
        using var ms = new MemoryStream();

        using (var writer = new CacheWriter(ms, expectedVersion, indexBuilder, true))
        {
            writer.Write(expectedReferenceCaches);
            expectedFilePairId = writer.FilePairId;
        }

        ms.Position = 0;

        ReferenceCache[]  actualReferenceCaches;
        int               actualFilePairId;
        DataSourceVersion actualVersion;

        using (var reader = new CacheReader(ms, ChromosomeUtilities.Chromosomes, hgncIdToSymbol))
        {
            actualReferenceCaches = reader.GetReferenceCaches();
            actualFilePairId      = reader.FilePairId;
            actualVersion         = reader.DataSourceVersion;
        }

        Assert.Equal(expectedReferenceCaches, actualReferenceCaches);
        Assert.Equal(expectedFilePairId, actualFilePairId);
        Assert.Equal(expectedVersion, actualVersion);
    }

    internal static ReferenceCache[] GetReferenceCaches()
    {
        // chr1
        var chr1Genes       = new[] {new Gene("BOB", "ENSG123", false, null) {Symbol = "TP53"}};
        var chr1CdnaSeqs    = new[] {"ACGT"};
        var chr1ProteinSeqs = new[] {"MARS"};
        var chr1TranscriptRegions = new[]
        {
            new TranscriptRegion(123, 456, 12, 34, TranscriptRegionType.Exon, 4, null)
        };
        var chr1CodingRegion = new CodingRegion(303, 505, 12, 87, "NP_456", chr1ProteinSeqs[0], 0, 0, 0, null, null);
        var chr1Transcripts = new[]
        {
            new Transcript(ChromosomeUtilities.Chr1, 111, 222, "NM_123", BioType.C_gene_segment, true, Source.RefSeq,
                chr1Genes[0], chr1TranscriptRegions, chr1CdnaSeqs[0], chr1CodingRegion)
        };
        var chr1CacheBins = new CacheBin[]
        {
            new(0, 0, chr1Genes, chr1TranscriptRegions, chr1CdnaSeqs, chr1ProteinSeqs, chr1Transcripts, null)
        };

        // chr3
        var chr3Genes       = new[] {new Gene("JOHN", "ENSG456", true, null) {Symbol = "KRAS"}};
        var chr3CdnaSeqs    = new[] {"AGAT"};
        var chr3ProteinSeqs = new[] {"MSSK"};
        var chr3TranscriptRegions = new[]
        {
            new TranscriptRegion(1203, 4560, 120, 340, TranscriptRegionType.Intron, 7, null)
        };
        var chr3CodingRegion =
            new CodingRegion(981, 1322, 202, 870, "NP_4556", chr3ProteinSeqs[0], 0, 0, 0, null, null);
        var chr3Transcripts = new[]
        {
            new Transcript(ChromosomeUtilities.Chr3, 141, 543, "NM_1011", BioType.mRNA, false, Source.RefSeq,
                chr3Genes[0], chr3TranscriptRegions, chr3CdnaSeqs[0], chr3CodingRegion)
        };
        var chr3CacheBins = new CacheBin[]
        {
            new(0, 0, null, null, null, null, null, null),
            new(1, 1, chr3Genes, chr3TranscriptRegions, chr3CdnaSeqs, chr3ProteinSeqs, chr3Transcripts, null)
        };

        return new ReferenceCache[]
        {
            new(ChromosomeUtilities.Chr1, chr1CacheBins),
            null,
            new(ChromosomeUtilities.Chr3, chr3CacheBins)
        };
    }
}