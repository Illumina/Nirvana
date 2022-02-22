using System;
using System.Collections.Generic;
using Cache.Data;
using Cache.IO;
using Intervals;
using UnitTests.MockedData;
using UnitTests.TestUtilities;
using VariantAnnotation.Providers;
using Versioning;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches
{
    public sealed class TranscriptCacheTests
    {
        private readonly TranscriptCache    _cache;
        private readonly IDataSourceVersion _expectedDataSourceVersion;

        public TranscriptCacheTests()
        {
            _expectedDataSourceVersion =
                new DataSourceVersion("VEP", Source.RefSeq.ToString(), "87", DateTime.Now.Ticks);

            var referenceCaches = new ReferenceCache[20];

            var chr1Transcripts = new Transcript[]
            {
                new(ChromosomeUtilities.Chr1, 120, 180, "ABC", BioType.mRNA, false, Source.RefSeq, Genes.MED8,
                    TranscriptRegions.ENST00000290663, "ACGT", CodingRegions.ENST00000290663),
                new(ChromosomeUtilities.Chr1, 300, 320, "ABC", BioType.mRNA, false, Source.RefSeq, Genes.MED8,
                    TranscriptRegions.ENST00000290663, "ACGT", CodingRegions.ENST00000290663)
            };

            var chr1RegulatoryRegions = new RegulatoryRegion[]
            {
                new(ChromosomeUtilities.Chr1, 120, 180, string.Empty, BioType.promoter, null, null, null),
                new(ChromosomeUtilities.Chr1, 300, 320, string.Empty, BioType.promoter, null, null, null)
            };

            var chromosome = ChromosomeUtilities.Chr1;
            var chr1CacheBins = new CacheBin[1];
            chr1CacheBins[0] = new CacheBin(0, 0, null, null, null, null, chr1Transcripts, chr1RegulatoryRegions);
            referenceCaches[chromosome.Index] = new ReferenceCache(chromosome, chr1CacheBins);

            var chr11Transcripts = new Transcript[]
            {
                new(ChromosomeUtilities.Chr11, 11000, 12000, "ABC", BioType.mRNA, false, Source.RefSeq, Genes.MED8,
                    TranscriptRegions.ENST00000290663, "ACGT", CodingRegions.ENST00000290663)
            };

            var chr11RegulatoryRegions = new RegulatoryRegion[]
            {
                new(ChromosomeUtilities.Chr11, 11000, 12000, string.Empty, BioType.promoter, null, null, null)
            };

            chromosome = ChromosomeUtilities.Chr11;
            var chr11CacheBins = new CacheBin[1];
            chr11CacheBins[0] = new CacheBin(0, 0, null, null, null, null, chr11Transcripts, chr11RegulatoryRegions);
            referenceCaches[chromosome.Index] = new ReferenceCache(chromosome, chr11CacheBins);

            _cache = new TranscriptCache(referenceCaches, _expectedDataSourceVersion);
        }

        [Fact]
        public void AddTranscripts_TwoTranscripts()
        {
            ushort    refIndex = ChromosomeUtilities.Chr1.Index;
            IInterval variant  = new Interval(100, 200);

            (int start, int end) = TranscriptAnnotationProvider.GetFlankingRegion(variant);

            List<Transcript> transcripts = new();
            _cache.AddTranscripts(refIndex, start, end, transcripts);

            Assert.Equal(2, transcripts.Count);
        }

        [Fact]
        public void AddTranscripts_NoTranscripts()
        {
            ushort    refIndex = ChromosomeUtilities.Chr11.Index;
            IInterval variant  = new Interval(5000, 5001);
            (int start, int end) = TranscriptAnnotationProvider.GetFlankingRegion(variant);

            List<Transcript> transcripts = new();
            _cache.AddTranscripts(refIndex, start, end, transcripts);

            Assert.Empty(transcripts);
        }

        [Fact]
        public void AddRegulatoryRegions_OneRegulatoryRegion()
        {
            ushort    refIndex = ChromosomeUtilities.Chr1.Index;
            const int start    = 100;
            const int end      = 200;

            List<RegulatoryRegion> regulatoryRegions = new();
            _cache.AddRegulatoryRegions(refIndex, start, end, regulatoryRegions);

            Assert.Single(regulatoryRegions);
        }

        [Fact]
        public void AddRegulatoryRegions_NoRegulatoryRegions()
        {
            ushort    refIndex = ChromosomeUtilities.Chr11.Index;
            const int start    = 5000;
            const int end      = 5001;

            List<RegulatoryRegion> regulatoryRegions = new();
            _cache.AddRegulatoryRegions(refIndex, start, end, regulatoryRegions);

            Assert.Empty(regulatoryRegions);
        }

        [Fact]
        public void DataSourceVersion_ExpectedResults()
        {
            IDataSourceVersion actual = _cache.DataSourceVersion;
            Assert.Equal(_expectedDataSourceVersion, actual);
        }
    }
}