using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using SAUtils.DataStructures;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.NSA
{
    public sealed class ChunkedIndexTests
    {
        private static (List<int>, List<long>, List<ushort>) AddRandomPositions(Chromosome chrom, ChunkedIndex index, int count)
        {
            var random = new Random(chrom.Index);
            int position = random.Next(1, byte.MaxValue);
            int fileLoc = random.Next(0, ushort.MaxValue - 1);

            var positions = new List<int>();
            var fileLocations = new List<long>();
            var recordLengths = new List<ushort>();

            for (var i = 0; i < count; i++)
            {
                var saItem = new DbSnpItem(chrom, position, fileLoc, "A", "T");
                var recordLength = (ushort)random.Next(0, ushort.MaxValue - 1);
                positions.Add(position);
                fileLocations.Add(fileLoc);
                recordLengths.Add(recordLength);
                index.Add(saItem.Chromosome.Index, saItem.Position, saItem.Position, fileLoc, recordLength);
                position += random.Next(0, ushort.MaxValue - 1);
                fileLoc += recordLength;
            }

            return (positions, fileLocations, recordLengths);
        }

        [Fact]
        public void Query_chunks_in_same_chrom()
        {
            var stream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(stream);
            var version = new DataSourceVersion("dbsnp", "150", DateTime.Now.Ticks, "dbsnp ids");
            var index = new ChunkedIndex(writer, GenomeAssembly.GRCh37, version, "dbsnp", true, true, SaCommon.SchemaVersion, false);

            index.Add(0, 100, 2000, 23457, 89320);
            index.Add(0, 2100, 4000, 112778, 58746);
            index.Add(0, 4100, 7000, 171525, 658794);

            (long start, long end, int chunkCount) = index.GetFileRange(0, 150, 2120);
            Assert.Equal(23457, start);
            Assert.Equal(171524, end);
            Assert.Equal(2, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 50, 98);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 150, 2010);
            Assert.Equal(23457, start);
            Assert.Equal(112777, end);
            Assert.Equal(1, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 2010, 4050);
            Assert.Equal(112778, start);
            Assert.Equal(171524, end);
            Assert.Equal(1, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 4010, 4050);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 7010, 7050);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);
        }

        [Fact]
        public void Query_chunks_in_different_chrom()
        {
            var stream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(stream);
            var version = new DataSourceVersion("dbsnp", "150", DateTime.Now.Ticks, "dbsnp ids");
            var index = new ChunkedIndex(writer, GenomeAssembly.GRCh37, version, "dbsnp", true, true, SaCommon.SchemaVersion, false);

            index.Add(0, 100, 2000, 23457, 89320);
            index.Add(0, 2100, 4000, 112778, 58746);
            index.Add(0, 4100, 7000, 171525, 658794);

            index.Add(1, 100, 2000, 23457, 89320);
            index.Add(1, 2100, 4000, 112778, 58746);
            index.Add(1, 4100, 7000, 171525, 658794);

            (long start, long end, int chunkCount) = index.GetFileRange(0, 150, 2120);
            Assert.Equal(23457, start);
            Assert.Equal(171524, end);
            Assert.Equal(2, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 50, 98);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 150, 2010);
            Assert.Equal(23457, start);
            Assert.Equal(112777, end);
            Assert.Equal(1, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 2010, 4050);
            Assert.Equal(112778, start);
            Assert.Equal(171524, end);
            Assert.Equal(1, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 4010, 4050);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 7010, 7050);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            //chr2
            (start, end, chunkCount) =  index.GetFileRange(0, 150, 2120);
            Assert.Equal(23457, start);
            Assert.Equal(171524, end);
            Assert.Equal(2, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 50, 98);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 150, 2010);
            Assert.Equal(23457, start);
            Assert.Equal(112777, end);
            Assert.Equal(1, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 2010, 4050);
            Assert.Equal(112778, start);
            Assert.Equal(171524, end);
            Assert.Equal(1, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 4010, 4050);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

            (start, end, chunkCount) = index.GetFileRange(0, 7010, 7050);
            Assert.Equal(-1, start);
            Assert.Equal(-1, end);
            Assert.Equal(0, chunkCount);

        }
    }
}