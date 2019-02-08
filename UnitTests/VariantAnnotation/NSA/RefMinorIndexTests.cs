using System;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.NSA
{
    public sealed class RefMinorIndexTests
    {
        [Fact]
        public void CreateAndQuery_one_chromosome()
        {
            using (var stream = new MemoryStream())
            using(var writer = new ExtendedBinaryWriter(stream))
            {
                var index = new RefMinorIndex(writer, GenomeAssembly.GRCh37, new DataSourceVersion("name", "1",  DateTime.Now.Ticks), SaCommon.SchemaVersion );

                index.Add(0, 100);
                index.Add(0, 105);
                index.Add(0, 110);
                index.Add(0, 115);
                index.Write(120);

                (long location, int byteCount, int count) = index.GetFileRange(0);
                Assert.Equal(100, location);
                Assert.Equal(20, byteCount);
                Assert.Equal(4, count);
            }
        }
        [Fact]
        public void CreateAndQuery_multiple_chromosomes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                var index = new RefMinorIndex(writer, GenomeAssembly.GRCh37, new DataSourceVersion("name", "1", DateTime.Now.Ticks), SaCommon.SchemaVersion);

                index.Add(0, 100);
                index.Add(0, 105);
                index.Add(0, 110);
                index.Add(0, 115);
                index.Add(1, 200);
                index.Add(1, 205);
                index.Add(1, 210);
                index.Add(2, 315);

                index.Write(320);

                (long location, int byteCount, int count) = index.GetFileRange(0);
                Assert.Equal(100, location);
                Assert.Equal(100, byteCount);
                Assert.Equal(4, count);

                (location, byteCount, count) = index.GetFileRange(1);
                Assert.Equal(200, location);
                Assert.Equal(115, byteCount);
                Assert.Equal(3, count);

                (location, byteCount, count) = index.GetFileRange(2);
                Assert.Equal(315, location);
                Assert.Equal(5, byteCount);
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void ReadBack()
        {
            var stream = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                var index = new RefMinorIndex(writer, GenomeAssembly.GRCh37, new DataSourceVersion("name", "1", DateTime.Now.Ticks), SaCommon.SchemaVersion);

                index.Add(0, 100);
                index.Add(0, 105);
                index.Add(0, 110);
                index.Add(0, 115);
                index.Add(1, 200);
                index.Add(1, 205);
                index.Add(1, 210);
                index.Add(2, 315);

                index.Write(320);
                
            }
            var readStream = new MemoryStream(stream.ToArray()) { Position = 0 };
            using (var reader = new ExtendedBinaryReader(readStream))
            {
                var index = new RefMinorIndex(reader);
                (long location, int byteCount, int count) = index.GetFileRange(0);
                Assert.Equal(100, location);
                Assert.Equal(100, byteCount);
                Assert.Equal(4, count);

                (location, byteCount, count) = index.GetFileRange(1);
                Assert.Equal(200, location);
                Assert.Equal(115, byteCount);
                Assert.Equal(3, count);

                (location, byteCount, count) = index.GetFileRange(2);
                Assert.Equal(315, location);
                Assert.Equal(5, byteCount);
                Assert.Equal(1, count);
            }
            
        }
    }
}