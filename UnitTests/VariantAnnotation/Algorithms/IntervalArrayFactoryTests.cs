using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Algorithms
{
    public sealed class IntervalArrayFactoryTests
    {
        private readonly IRegulatoryRegion[] _regulatoryRegions;
        private readonly IChromosome _chr1 = new Chromosome("chr1", "1", 0);
        private readonly IChromosome _chr11 = new Chromosome("chr11", "11", 10);
        private readonly IChromosome _chr17 = new Chromosome("chr17", "17", 16);

        public IntervalArrayFactoryTests()
        {
            _regulatoryRegions = GetRegulatoryRegions();
        }

        [Fact]
        public void CreateIntervalForest_WithIntervals()
        {
            var intervalForest = IntervalArrayFactory.CreateIntervalForest(_regulatoryRegions, 17);
            var observedResult = intervalForest.OverlapsAny(_chr11.Index, 160, 170);
            Assert.True(observedResult);
        }

        [Fact]
        public void CreateIntervalForest_NullIntervals()
        {
            var intervalForest = IntervalArrayFactory.CreateIntervalForest<IRegulatoryRegion>(null, 17);
            Assert.True(intervalForest is NullIntervalSearch<IRegulatoryRegion>);
        }

        private IRegulatoryRegion[] GetRegulatoryRegions()
        {
            var regulatoryRegions = new IRegulatoryRegion[7];
            regulatoryRegions[0] = new RegulatoryRegion(_chr17, 100,200,CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[1] = new RegulatoryRegion(_chr17, 200, 300, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[2] = new RegulatoryRegion(_chr17, 300, 400, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[3] = new RegulatoryRegion(_chr1, 1000, 2000, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[4] = new RegulatoryRegion(_chr1, 2000, 3000, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[5] = new RegulatoryRegion(_chr11, 150, 250, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[6] = new RegulatoryRegion(_chr11, 250, 350, CompactId.Empty, RegulatoryRegionType.enhancer);
            return regulatoryRegions;
        }
    }
}
