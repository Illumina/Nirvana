using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches
{
    public sealed class TranscriptCacheTests
    {
        private readonly ITranscriptCache _cache;
        private readonly IEnumerable<IDataSourceVersion> expectedDataSourceVersions;
        private readonly GenomeAssembly expectedGenomeAssembly = GenomeAssembly.hg19;
        private readonly ITranscript[] expectedTranscripts;
        private readonly IRegulatoryRegion[] expectedRegulatoryRegions;

        private readonly IChromosome _chr1  = new Chromosome("chr1", "1", 0);
        private readonly IChromosome _chr11 = new Chromosome("chr11", "11", 10);

        public TranscriptCacheTests()
        {
            expectedDataSourceVersions = GetDataSourceVersions();
            expectedTranscripts        = GetTranscripts();
            expectedRegulatoryRegions  = GetRegulatoryRegions();

            _cache = new TranscriptCache(expectedDataSourceVersions, expectedGenomeAssembly, expectedTranscripts,
                expectedRegulatoryRegions, 25);
        }

        [Fact]
        public void GetOverlappingFlankingTranscripts_TwoOverlaps()
        {
            var interval = new Mock<IChromosomeInterval>();
            interval.SetupGet(x => x.Chromosome).Returns(_chr1);
            interval.SetupGet(x => x.Start).Returns(100);
            interval.SetupGet(x => x.End).Returns(200);

            var overlappingTranscripts = _cache.GetOverlappingFlankingTranscripts(interval.Object);

            Assert.NotNull(overlappingTranscripts);
            Assert.Equal(2, overlappingTranscripts.Length);
        }

        [Fact]
        public void GetOverlappingFlankingTranscripts_NoOverlaps()
        {
            var interval = new Mock<IChromosomeInterval>();
            interval.SetupGet(x => x.Chromosome).Returns(_chr11);
            interval.SetupGet(x => x.Start).Returns(5000);
            interval.SetupGet(x => x.End).Returns(5001);

            var overlappingTranscripts = _cache.GetOverlappingFlankingTranscripts(interval.Object);

            Assert.Null(overlappingTranscripts);
        }

        [Fact]
        public void GetOverlappingRegulatoryRegions_OneOverlap()
        {
            var interval = new Mock<IChromosomeInterval>();
            interval.SetupGet(x => x.Chromosome).Returns(_chr1);
            interval.SetupGet(x => x.Start).Returns(100);
            interval.SetupGet(x => x.End).Returns(200);

            var overlappingRegulatoryRegions = _cache.GetOverlappingRegulatoryRegions(interval.Object);

            Assert.NotNull(overlappingRegulatoryRegions);
            Assert.Equal(1, overlappingRegulatoryRegions.Length);
        }

        [Fact]
        public void GetOverlappingRegulatoryRegions_NoOverlaps()
        {
            var interval = new Mock<IChromosomeInterval>();
            interval.SetupGet(x => x.Chromosome).Returns(_chr11);
            interval.SetupGet(x => x.Start).Returns(5000);
            interval.SetupGet(x => x.End).Returns(5001);

            var overlappingRegulatoryRegions = _cache.GetOverlappingRegulatoryRegions(interval.Object);

            Assert.Null(overlappingRegulatoryRegions);
        }

        [Fact]
        public void GenomeAssembly_Get()
        {
            var observedGenomeAssembly = _cache.GenomeAssembly;
            Assert.Equal(expectedGenomeAssembly, observedGenomeAssembly);
        }

        [Fact]
        public void DataSourceVersions_Get()
        {
            var observedDataSourceVersions = _cache.DataSourceVersions.ToArray();
            Assert.Equal(1, observedDataSourceVersions.Length);

            var expectedDataSourceVersion = expectedDataSourceVersions.ToArray()[0];
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

        private IRegulatoryRegion[] GetRegulatoryRegions()
        {
            var regulatoryRegions = new IRegulatoryRegion[3];

            regulatoryRegions[0] = new RegulatoryRegion(_chr11, 11000, 12000, CompactId.Empty,
                RegulatoryElementType.promoter);

            regulatoryRegions[1] = new RegulatoryRegion(_chr1, 120, 180, CompactId.Empty,
                RegulatoryElementType.promoter);

            regulatoryRegions[2] = new RegulatoryRegion(_chr1, 300, 320, CompactId.Empty,
                RegulatoryElementType.promoter);

            return regulatoryRegions;
        }

        private ITranscript[] GetTranscripts()
        {
            var transcripts = new ITranscript[3];

            transcripts[0] = new Transcript(_chr11, 11000, 12000, CompactId.Empty, 0, null, BioType.Unknown, null, 0, 0,
                false, null, null, null, 0, 0, Source.None);

            transcripts[1] = new Transcript(_chr1, 120, 180, CompactId.Empty, 0, null, BioType.Unknown, null, 0, 0,
                false, null, null, null, 0, 0, Source.None);

            transcripts[2] = new Transcript(_chr1, 300, 320, CompactId.Empty, 0, null, BioType.Unknown, null, 0, 0,
                false, null, null, null, 0, 0, Source.None);

            return transcripts;
        }
    }
}
