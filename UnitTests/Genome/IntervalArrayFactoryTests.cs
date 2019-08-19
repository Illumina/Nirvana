using Genome;
using Intervals;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class IntervalArrayFactoryTests
    {
        private readonly IRegulatoryRegion[] _regulatoryRegions;

        public IntervalArrayFactoryTests() => _regulatoryRegions = GetRegulatoryRegions();

        [Fact]
        public void CreateIntervalForest_WithIntervals()
        {
            var intervalForest = IntervalArrayFactory.CreateIntervalForest(_regulatoryRegions, 17);
            bool observedResult = intervalForest.OverlapsAny(ChromosomeUtilities.Chr11.Index, 160, 170);
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
            regulatoryRegions[0] = new RegulatoryRegion(ChromosomeUtilities.Chr17, 100,200,CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[1] = new RegulatoryRegion(ChromosomeUtilities.Chr17, 200, 300, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[2] = new RegulatoryRegion(ChromosomeUtilities.Chr17, 300, 400, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[3] = new RegulatoryRegion(ChromosomeUtilities.Chr1, 1000, 2000, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[4] = new RegulatoryRegion(ChromosomeUtilities.Chr1, 2000, 3000, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[5] = new RegulatoryRegion(ChromosomeUtilities.Chr11, 150, 250, CompactId.Empty, RegulatoryRegionType.enhancer);
            regulatoryRegions[6] = new RegulatoryRegion(ChromosomeUtilities.Chr11, 250, 350, CompactId.Empty, RegulatoryRegionType.enhancer);
            return regulatoryRegions;
        }
    }
}
