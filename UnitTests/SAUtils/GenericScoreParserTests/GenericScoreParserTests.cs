using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.GenericScoreParserTests
{
    public sealed class GenericScoreParserTests
    {
        private ParserSettings _parserSettings = new(
            new ColumnIndex(0, 2, 3, 4, 5, null),
            new[] {"A", "C", "G", "T"},
            GenericScoreParser.MaxRepresentativeScores
        );

        [Fact]
        public void TestParserNonNumericValues()
        {
            var writer = new StreamWriter(new MemoryStream());
            writer.WriteLine("#chr\tpos\tref\talt\tscore");
            writer.WriteLine("1\t10003\t10003\tA\tG\taskdlj");
            writer.WriteLine("asd\t10003\t10003\tA\tG\taskdlj");
            writer.Flush();
            writer.BaseStream.Position = 0;

            using (var streamReader = new StreamReader(writer.BaseStream))
            using (var reader = new GenericScoreParser(_parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                GenericScoreItem[] genericScoreItems = reader.GetItems().ToArray();
                Assert.Empty(genericScoreItems);
            }
        }

        [Fact]
        public void TestMaxScore()
        {
            var writer = new StreamWriter(new MemoryStream());
            writer.WriteLine("#chr\tpos\tref\talt\tscore");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.1");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.5");
            writer.Flush();

            writer.BaseStream.Position = 0;
            _parserSettings = new ParserSettings(
                new ColumnIndex(0, 2, 3, 4, 5, null),
                new[] {"A", "C", "G", "T"},
                GenericScoreParser.MaxRepresentativeScores
            );

            using (var streamReader = new StreamReader(writer.BaseStream))
            using (var reader = new GenericScoreParser(_parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                GenericScoreItem[] genericScoreItems = reader.GetItems().ToArray();
                Assert.Single(genericScoreItems);
                Assert.Equal(0.5, genericScoreItems[0].Score);
            }
        }

        [Fact]
        public void TestMinScore()
        {
            var writer = new StreamWriter(new MemoryStream());
            writer.WriteLine("#chr\tpos\tref\talt\tscore");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.1");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.5");
            writer.Flush();

            writer.BaseStream.Position = 0;
            _parserSettings = new ParserSettings(
                new ColumnIndex(0, 2, 3, 4, 5, null),
                new[] {"A", "C", "G", "T"},
                GenericScoreParser.MinRepresentativeScores
            );
            using (var streamReader = new StreamReader(writer.BaseStream))
            using (var reader = new GenericScoreParser(_parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                GenericScoreItem[] genericScoreItems = reader.GetItems().ToArray();
                Assert.Single(genericScoreItems);
                Assert.Equal(0.1, genericScoreItems[0].Score);
            }
        }

        [Fact]
        public void TestNonConflictingScores()
        {
            var writer = new StreamWriter(new MemoryStream());
            writer.WriteLine("#chr\tpos\tref\talt\tscore");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.1");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.5");
            writer.Flush();

            writer.BaseStream.Position = 0;
            _parserSettings = new ParserSettings(
                new ColumnIndex(0, 2, 3, 4, 5, null),
                new[] {"A", "C", "G", "T"},
                GenericScoreParser.NonConflictingScore
            );
            using (var streamReader = new StreamReader(writer.BaseStream))
            using (var reader = new GenericScoreParser(_parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => reader.GetItems().ToArray());
            }
        }
    }
}