using System;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.NSA;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class IntervalChromIndexTests
    {
        
        private static IntervalChromIndex CreateIndex()
        {
            var chrom1 = new Chromosome("chr1", "1", 0);
            long location = 1000;
            var index = new IntervalChromIndex(location);
            var start = 100;

            for (int i = 0; i < 100; i++)
            {
                var random = new Random(3);
                start += random.Next(0, byte.MaxValue - 1);
                var end = start + random.Next(0, byte.MaxValue - 1);
                location += random.Next(0, ushort.MaxValue - 1);
                int recordLength = random.Next(0, ushort.MaxValue - 1);

                index.Add(start, end, location, location + recordLength);
            }

            return index;
        }

        [Fact]
        public void Add_randomItems()
        {
            IntervalChromIndex index = CreateIndex();

            Assert.Equal(100, index.Count);
        }

        [Fact]
        public void Readback_query()
        {
            IntervalChromIndex index = CreateIndex();

            var stream = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                index.Write(writer);
            }

            var readStream = new MemoryStream(stream.ToArray()) { Position = 0 };
            using (var reader = new ExtendedBinaryReader(readStream))
            {
                var readIndex = new IntervalChromIndex(reader);
                Assert.Equal(index.Count, readIndex.Count);

                var overlapRange = readIndex.GetLocationRange(100, 1000);

                Assert.Equal(57686, overlapRange.startLocation);
                Assert.Equal(694239, overlapRange.endLocation);
            }
        }

    }
}