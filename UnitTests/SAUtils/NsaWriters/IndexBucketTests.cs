using System;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using IO;
using VariantAnnotation.NSA;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class IndexBucketTests
    {
        [Fact]
        public void TryAdd()
        {
            var bucket = new IndexBucket(100, 10345, 456);
            Assert.True(bucket.TryAdd(200, 10801, 7483));
            Assert.False(bucket.TryAdd(256000, 18284, 4736));

            int lastPosition = 200;
            long fileLocation = 18284;
            for (var i = 0; i < 100; i++)
            {
                var random = new Random(3);
                lastPosition += random.Next(0, ushort.MaxValue - 1);
                int recordLength = random.Next(0, ushort.MaxValue - 1);
                
                Assert.True(bucket.TryAdd(lastPosition, fileLocation, (ushort)recordLength));
                fileLocation += recordLength;

            }

            Assert.Equal(102, bucket.Count);
            Assert.Throws<UserErrorException>(() => bucket.TryAdd(131, 647382, 9832));
        }

        [Fact]
        public void Readback()
        {
            var bucket = new IndexBucket(100, 10345, 456);
            int lastPosition = 100;
            long fileLocation = 10801;
            for (var i = 0; i < 100; i++)
            {
                var random = new Random(3);
                lastPosition += random.Next(0, ushort.MaxValue - 1);
                int recordLength = random.Next(0, ushort.MaxValue - 1);

                Assert.True(bucket.TryAdd(lastPosition, fileLocation, (ushort)recordLength));
                fileLocation += recordLength;

            }
            var zstd = new Zstandard();
            var stream = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                bucket.Write(writer, zstd);
            }

            var readStream = new MemoryStream(stream.ToArray());
            readStream.Position = 0;
            using (var reader = new ExtendedBinaryReader(readStream))
            {
                var readBucket = new IndexBucket(reader, zstd);
                Assert.Equal(101, readBucket.Count);
            }
        }

        [Fact]
        public void GetAnnotationRecords()
        {
            var bucket = new IndexBucket(100, 10345, 456);
            Assert.True(bucket.TryAdd(200, 10801, 7483));
            Assert.True(bucket.TryAdd(256, 18284, 4736));
            Assert.True(bucket.TryAdd(356, 23020, 436));

            var zstd = new Zstandard();
            var stream = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                bucket.Write(writer, zstd);
            }

            var readStream = new MemoryStream(stream.ToArray());
            readStream.Position=0;
            using (var reader = new ExtendedBinaryReader(readStream))
            {
                var readBucket = new IndexBucket(reader, zstd);
                (long location, ushort length) = readBucket.GetAnnotationRecord(100);
                Assert.Equal(((long)10345, (ushort)456), (location, length));

                (location, length) = readBucket.GetAnnotationRecord(200);
                Assert.Equal(((long)10801, (ushort)7483), (location, length));

                (location, length) = readBucket.GetAnnotationRecord(85);
                Assert.Equal(((long)-1, (ushort)0), (location, length));

                (location, length) = readBucket.GetAnnotationRecord(356);
                Assert.Equal(((long)23020, (ushort)436), (location, length));

                (location, length) = readBucket.GetAnnotationRecord(585);
                Assert.Equal(((long)-1, (ushort)0), (location, length));

            }
        }

    }
}