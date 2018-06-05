using System.Collections.Generic;
using System.IO;
using System.Text;
using Genome;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class RegulatoryRegionTests
    {
        [Fact]
        public void RegulatoryRegion_EndToEnd()
        {
            IChromosome expectedChromosome          = new Chromosome("chrBob", "Bob", 1);
            const int expectedStart                 = int.MaxValue;
            const int expectedEnd                   = int.MinValue;
            const string expectedId                 = "ENST00000540021";
            const RegulatoryRegionType expectedType = RegulatoryRegionType.open_chromatin_region;

            var indexToChromosome = new Dictionary<ushort, IChromosome>
            {
                [expectedChromosome.Index] = expectedChromosome
            };

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var regulatoryRegion = new RegulatoryRegion(expectedChromosome, expectedStart, expectedEnd,
                CompactId.Convert(expectedId), expectedType);
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            IRegulatoryRegion observedRegulatoryRegion;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    regulatoryRegion.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new BufferedBinaryReader(ms))
                {
                    observedRegulatoryRegion = RegulatoryRegion.Read(reader, indexToChromosome);
                }
            }

            Assert.NotNull(observedRegulatoryRegion);
            Assert.Equal(expectedStart,            observedRegulatoryRegion.Start);
            Assert.Equal(expectedEnd,              observedRegulatoryRegion.End);
            Assert.Equal(expectedId,               observedRegulatoryRegion.Id.WithoutVersion);
            Assert.Equal(expectedType,             observedRegulatoryRegion.Type);
            Assert.Equal(expectedChromosome.Index, observedRegulatoryRegion.Chromosome.Index);
        }
    }
}
