using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class IntervalIndexTests
    {
        private static IEnumerable<(ushort index, int start, int end, long startLocation, ushort recordLength)> GetSiRecords(IChromosome chromosome, int count)
        {
            long location = 1000;
            var start = 100;

            for (int i = 0; i < count; i++)
            {
                var random = new Random(chromosome.Index);
                start += random.Next(0, byte.MaxValue - 1);
                var end = start + random.Next(0, byte.MaxValue - 1);
                location += random.Next(0, ushort.MaxValue - 1);
                ushort recordLength = (ushort)random.Next(0, ushort.MaxValue - 1);

                yield return (chromosome.Index, start, end, location, recordLength);
            }
            
        }
        [Fact]
        public void Readback_one_chromosome()
        {
            var index = new IntervalIndex("dataSource1", ReportFor.StructuralVariants);

            foreach ((ushort chromIndex, int start, int end, long startLocation, ushort recordLength) in GetSiRecords(ChromosomeUtilities.Chr1, 100 ))
            {
                var siItem = new Mock<ISuppIntervalItem>();
                siItem.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
                siItem.SetupGet(x => x.Start).Returns(start);
                siItem.SetupGet(x => x.End).Returns(end);

                index.Add(siItem.Object, startLocation, recordLength);
            }

            var writeStream = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(writeStream))
            {
                index.Write(writer);
            }

            var readStream = new MemoryStream(writeStream.ToArray()) { Position = 0 };
            using (var reader = new ExtendedBinaryReader(readStream))
            {
                var readIndex = new IntervalIndex(reader);

                var recordsRange = readIndex.GetLocationRange(ChromosomeUtilities.Chr1.Index, 100, 500);

                Assert.Equal(51331, recordsRange.startLocation);
                Assert.Equal(138240, recordsRange.endLocation);
            }
        }

        [Fact]
        public void Readback_two_chromosomes()
        {
            var index = new IntervalIndex("dataSource1", ReportFor.StructuralVariants);

            foreach ((ushort chromIndex, int start, int end, long startLocation, ushort recordLength) in GetSiRecords(ChromosomeUtilities.Chr1, 100))
            {
                var siItem = new Mock<ISuppIntervalItem>();
                siItem.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
                siItem.SetupGet(x => x.Start).Returns(start);
                siItem.SetupGet(x => x.End).Returns(end);

                index.Add(siItem.Object, startLocation, recordLength);
            }

            foreach ((ushort chromIndex, int start, int end, long startLocation, ushort recordLength) in GetSiRecords(ChromosomeUtilities.Chr2, 100))
            {
                var siItem = new Mock<ISuppIntervalItem>();
                siItem.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr2);
                siItem.SetupGet(x => x.Start).Returns(start);
                siItem.SetupGet(x => x.End).Returns(end);

                index.Add(siItem.Object, startLocation, recordLength);
            }

            var writeStream = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(writeStream))
            {
                index.Write(writer);
            }

            var readStream = new MemoryStream(writeStream.ToArray()) { Position = 0 };
            using (var reader = new ExtendedBinaryReader(readStream))
            {
                var readIndex = new IntervalIndex(reader);

                var recordsRange = readIndex.GetLocationRange(ChromosomeUtilities.Chr1.Index, 100, 500);

                Assert.Equal(51331, recordsRange.startLocation);
                Assert.Equal(138240, recordsRange.endLocation);

                recordsRange = readIndex.GetLocationRange(ChromosomeUtilities.Chr2.Index, 100, 500);
                Assert.Equal(31605, recordsRange.startLocation);
                Assert.Equal(235196, recordsRange.endLocation);

            }
        }
    }
}