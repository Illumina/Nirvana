using System;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using IO.v2;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile
{
    public sealed class ScoreIndexTests
    {
        [Fact]
        public void ScoreIndexTest()
        {
            (Stream indexStream, ScoreIndex scoreIndex) = GetScoreIndex();

            // Add chromosome blocks
            scoreIndex.AddChromosomeBlock(1, 10);
            scoreIndex.Add(1, 0, 1, 1);

            scoreIndex.AddChromosomeBlock(2, 80);
            scoreIndex.Add(2, 1, 2, 3);
            scoreIndex.Add(2, 3, 2, 3);
            scoreIndex.Add(2, 5, 2, 3);

            scoreIndex.AddChromosomeBlock(3, 70);
            scoreIndex.Add(3, 7,  20, 30);
            scoreIndex.Add(3, 27, 30, 30);
            scoreIndex.Add(3, 57, 20, 30);

            // Serialization and deserialization 
            scoreIndex.Write();
            indexStream.Position = 0;
            ScoreIndex scoreIndexDeserialized = ScoreIndex.Read(indexStream, 1);
            indexStream.Close();

            Assert.Equal(scoreIndex.GetBlockNumber(1, 10),  scoreIndexDeserialized.GetBlockNumber(1, 10));
            Assert.Equal(scoreIndex.GetBlockNumber(2, 104), scoreIndexDeserialized.GetBlockNumber(2, 104));
            Assert.Equal(scoreIndex.GetBlockLength(),       scoreIndexDeserialized.GetBlockLength());
            Assert.Equal(scoreIndex.GetNucleotideCount(),   scoreIndexDeserialized.GetNucleotideCount());

            // LastBlockNumber
            Assert.Equal(0, scoreIndexDeserialized.GetLastBlockNumber(1));
            Assert.Equal(2, scoreIndexDeserialized.GetLastBlockNumber(2));
            Assert.Equal(2, scoreIndexDeserialized.GetLastBlockNumber(3));

            // BlockNumber
            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(1, 9));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(1, 10));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(1, 34));
            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(1, 35));

            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(2, 70));
            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(2, 75));
            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(2, 79));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(2, 80));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(2, 104));
            Assert.Equal(1,  scoreIndexDeserialized.GetBlockNumber(2, 105));
            Assert.Equal(1,  scoreIndexDeserialized.GetBlockNumber(2, 129));
            Assert.Equal(2,  scoreIndexDeserialized.GetBlockNumber(2, 130));
            Assert.Equal(2,  scoreIndexDeserialized.GetBlockNumber(2, 154));
            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(2, 155));

            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(3, 68));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(3, 70));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(3, 80));
            Assert.Equal(0,  scoreIndexDeserialized.GetBlockNumber(3, 94));
            Assert.Equal(1,  scoreIndexDeserialized.GetBlockNumber(3, 95));
            Assert.Equal(1,  scoreIndexDeserialized.GetBlockNumber(3, 119));
            Assert.Equal(2,  scoreIndexDeserialized.GetBlockNumber(3, 120));
            Assert.Equal(2,  scoreIndexDeserialized.GetBlockNumber(3, 144));
            Assert.Equal(-1, scoreIndexDeserialized.GetBlockNumber(3, 145));

            // Position before chromosome starts
            Assert.Equal((-1, -1), scoreIndex.PositionToBlockLocation((ushort) 3, 67));
            Assert.Equal((-1, -1), scoreIndex.PositionToBlockLocation((ushort) 3, 67));

            // Chromosome not added
            Assert.Equal(-1,       scoreIndex.GetBlockNumber(4, 67));
            Assert.Equal(-1,       scoreIndex.GetFilePosition(4, 67));
            Assert.Equal((-1, -1), scoreIndex.PositionToBlockLocation((ushort) 4, 1));
        }

        [Fact]
        public void PositionToBlockIndexTest()
        {
            (Stream indexStream, ScoreIndex scoreIndex) = GetScoreIndex();
            // Position to block location tests
            var testData = new[]
            {
                // Start psotion, postiion, expected Block number, expected block index
                (10, 11, 0, 4),
                (10, 26, 0, 64),
                (10, 34, 0, 96),
                (10, 35, 1, 0),
                (10, 40, 1, 20),
            };

            foreach ((int startingPosition, int position, int expectedBlockNumber, int expectedBlockIndex) in testData)
            {
                Assert.Equal((expectedBlockNumber, expectedBlockIndex), scoreIndex.PositionToBlockLocation(position, startingPosition));
            }
        }

        [Fact]
        public void AddGetChromosomeBlocksTest()
        {
            (_, ScoreIndex scoreIndex) = GetScoreIndex();

            // Add and get chromosome blocks
            scoreIndex.AddChromosomeBlock(1, 10);
            scoreIndex.Add(1, 0, 1, 1);
            Assert.Single(scoreIndex.GetChromosomeBlocks());
            Assert.Equal(1, scoreIndex.GetChromosomeBlocks()[1].BlockCount);

            scoreIndex.AddChromosomeBlock(2, 80);
            scoreIndex.Add(2, 1, 2, 3);
            scoreIndex.Add(2, 3, 2, 3);
            scoreIndex.Add(2, 5, 2, 3);
            Assert.Equal(2, scoreIndex.GetChromosomeBlocks().Count);
            Assert.Equal(1, scoreIndex.GetChromosomeBlocks()[1].BlockCount);
            Assert.Equal(3, scoreIndex.GetChromosomeBlocks()[2].BlockCount);
        }

        [Fact]
        public void TestGetNucleotidePosition()
        {
            (_, ScoreIndex scoreIndex) = GetScoreIndex();

            // Add and get chromosome blocks
            scoreIndex.AddChromosomeBlock(1, 10);
            scoreIndex.Add(1, 0, 1, 1);

            Assert.Null(scoreIndex.GetNucleotidePosition("F"));
            Assert.Equal(0, (short) scoreIndex.GetNucleotidePosition("A"));
            Assert.Equal(1, (short) scoreIndex.GetNucleotidePosition("C"));
            Assert.Equal(2, (short) scoreIndex.GetNucleotidePosition("G"));
            Assert.Equal(3, (short) scoreIndex.GetNucleotidePosition("T"));
        }

        private static (Stream stream, ScoreIndex scoreIndex) GetScoreIndex()
        {
            var indexStream = new MemoryStream();
            var indexWriter = new ExtendedBinaryWriter(indexStream, System.Text.Encoding.Default);
            var version     = new DataSourceVersion("Test", "1", DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks, "No description");
            var header      = new Header(FileType.GsaIndex, 1);

            var readerSettings = new ReaderSettings(
                new ScoreEncoder(2, 1),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new[] {"A", "C", "G", "T"},
                25
            );

            var scoreIndex = new ScoreIndex(
                indexWriter,
                readerSettings,
                GenomeAssembly.Unknown,
                version,
                0,
                header,
                1
            );
            return (indexStream, scoreIndex);
        }

        [Fact]
        public void TestHeader()
        {
            var indexStream = new MemoryStream();
            var indexWriter = new ExtendedBinaryWriter(indexStream, System.Text.Encoding.Default);
            var version     = new DataSourceVersion("Test", "1", DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks, "No description");
            var header      = new Header(FileType.GsaWriter, 1);

            var readerSettings = new ReaderSettings(
                new ScoreEncoder(2, 1),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new[] {"A", "C", "G", "T"},
                25
            );

            var scoreIndex = new ScoreIndex(
                indexWriter,
                readerSettings,
                GenomeAssembly.Unknown,
                version,
                0,
                header,
                1
            );

            scoreIndex.Write();

            indexStream.Position = 0;

            Assert.Throws<UserErrorException>(() => ScoreIndex.Read(indexStream, 1));
        }
    }
}