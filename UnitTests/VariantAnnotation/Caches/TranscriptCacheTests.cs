using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.TranscriptCache;
using Genome;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches
{
    public sealed class TranscriptCacheTests
    {
        private readonly ITranscriptCache _cache;
        private readonly IEnumerable<IDataSourceVersion> _expectedDataSourceVersions;
        private const GenomeAssembly ExpectedAssembly = GenomeAssembly.hg19;

        public TranscriptCacheTests()
        {
            _expectedDataSourceVersions        = GetDataSourceVersions();
            var transcriptIntervalArrays       = GetTranscripts().ToIntervalArrays(11);
            var regulatoryRegionIntervalArrays = GetRegulatoryRegions().ToIntervalArrays(11);

            _cache = new TranscriptCache(_expectedDataSourceVersions, ExpectedAssembly, transcriptIntervalArrays,
                regulatoryRegionIntervalArrays);
        }

        [Fact]
        public void GetOverlappingFlankingTranscripts_TwoOverlaps()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            ITranscript[] overlappingTranscripts = _cache.TranscriptIntervalForest.GetAllFlankingValues(interval);

            Assert.NotNull(overlappingTranscripts);
            Assert.Equal(2, overlappingTranscripts.Length);
        }

        [Fact]
        public void GetOverlappingFlankingTranscripts_NoOverlaps()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr11, 5000, 5001);
            ITranscript[] overlappingTranscripts = _cache.TranscriptIntervalForest.GetAllFlankingValues(interval);

            Assert.Null(overlappingTranscripts);
        }

        [Fact]
        public void GetOverlappingRegulatoryRegions_OneOverlap()
        {
            var overlappingRegulatoryRegions =
                _cache.RegulatoryIntervalForest.GetAllOverlappingValues(ChromosomeUtilities.Chr1.Index, 100, 200);

            Assert.NotNull(overlappingRegulatoryRegions);
            Assert.Single(overlappingRegulatoryRegions);
        }

        [Fact]
        public void GetOverlappingRegulatoryRegions_NoOverlaps()
        {
            var overlappingRegulatoryRegions =
                _cache.RegulatoryIntervalForest.GetAllOverlappingValues(ChromosomeUtilities.Chr1.Index, 5000, 5001);

            Assert.Null(overlappingRegulatoryRegions);
        }

        [Fact]
        public void Assembly_Get()
        {
            var observedAssembly = _cache.Assembly;
            Assert.Equal(ExpectedAssembly, observedAssembly);
        }

        [Fact]
        public void DataSourceVersions_Get()
        {
            var observedDataSourceVersions = _cache.DataSourceVersions.ToArray();
            Assert.Single(observedDataSourceVersions);

            var expectedDataSourceVersion = _expectedDataSourceVersions.ToArray()[0];
            var observedDataSourceVersion = observedDataSourceVersions[0];
            Assert.Equal(expectedDataSourceVersion.Name, observedDataSourceVersion.Name);
        }

        [Fact]
        private IEnumerable<IDataSourceVersion> GetDataSourceVersions()
        {
            return new List<IDataSourceVersion>
            {
                new DataSourceVersion("VEP", "87", DateTime.Now.Ticks, Source.BothRefSeqAndEnsembl.ToString())
            };
        }

        private static IRegulatoryRegion[] GetRegulatoryRegions()
        {
            var regulatoryRegions = new IRegulatoryRegion[3];

            regulatoryRegions[0] = new RegulatoryRegion(ChromosomeUtilities.Chr11, 11000, 12000, CompactId.Empty,
                RegulatoryRegionType.promoter);

            regulatoryRegions[1] = new RegulatoryRegion(ChromosomeUtilities.Chr1, 120, 180, CompactId.Empty,
                RegulatoryRegionType.promoter);

            regulatoryRegions[2] = new RegulatoryRegion(ChromosomeUtilities.Chr1, 300, 320, CompactId.Empty,
                RegulatoryRegionType.promoter);

            return regulatoryRegions;
        }

        internal static ITranscript[] GetTranscripts()
        {
            return new ITranscript[]
            {
                new Transcript(ChromosomeUtilities.Chr11, 11000, 12000, CompactId.Empty, null, BioType.other, null, 0, 0,
                    false, null, 0, null, 0, 0, Source.None, false, false, null, null),
                new Transcript(ChromosomeUtilities.Chr1, 120, 180, CompactId.Empty, null, BioType.other, null, 0, 0,
                    false, null, 0, null, 0, 0, Source.None, false, false, null, null),
                new Transcript(ChromosomeUtilities.Chr1, 300, 320, CompactId.Empty, null, BioType.other, null, 0, 0,
                    false, null, 0, null, 0, 0, Source.None, false, false, null, null)
            };
        }
    }
}
