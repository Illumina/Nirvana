using System.IO;
using System.Linq;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.GERP
{
    public sealed class GerpParserTests
    {
        private static Stream GetGerpWigStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("#bedGraph section 1:12646-13697\n" +
                             "1\t12646\t12647\t0.298\n"          +
                             "1\t12647\t12648\t2.63\n"           +
                             "1\t12648\t12649\t1.87\n"           +
                             "1\t12649\t12650\t0.252\n"          +
                             "1\t12650\t12651\t-2.06\n"          +
                             "1\t12651\t12652\t2.61\n"           +
                             "1\t12652\t12653\t3.97\n"           +
                             "1\t12653\t12654\t4.9\n"            +
                             "1\t12654\t12655\t1.98\n"           +
                             "1\t12655\t12656\t4.72");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private static Stream GetGerpTsvStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("#chrom\tpos\tGERP\n" +
                             "1\t10000\t0\n"       +
                             "1\t12596\t-0.159\n"  +
                             "1\t12597\t0.848\n"   +
                             "1\t12598\t0.848\n"   +
                             "1\t12599\t-1.13\n"   +
                             "1\t12600\t-0.649\n"  +
                             "1\t12601\t0.698\n"   +
                             "1\t12602\t-0.194\n"  +
                             "1\t12603\t0.848\n"   +
                             "1\t12604\t-0.479\n"  +
                             "1\t12605\t0.848");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void ReadWigItems()
        {
            var nucleotides = new[] {"N"};

            var parserSettings = new ParserSettings(
                new ColumnIndex(0, 2, null, null, 3, null),
                nucleotides,
                GenericScoreParser.NonConflictingScore
            );

            using (var streamReader = new StreamReader(GetGerpWigStream()))
            using (var scoreParser = new GenericScoreParser(parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                GenericScoreItem[] items = scoreParser.GetItems().ToArray();
                Assert.Equal(10, items.Length);
            }
        }

        [Fact]
        public void ReadTsvItems()
        {
            var nucleotides = new[] {"N"};

            var parserSettings = new ParserSettings(
                new ColumnIndex(0, 1, null, null, 2, null),
                nucleotides,
                GenericScoreParser.NonConflictingScore
            );

            using (var streamReader = new StreamReader(GetGerpTsvStream()))
            using (var scoreParser = new GenericScoreParser(parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                GenericScoreItem[] items = scoreParser.GetItems().ToArray();
                Assert.Equal(11, items.Length);
            }
        }
        
        [Fact]
        public void TestScientificNotationScore()
        {
            var writer = new StreamWriter(new MemoryStream());
            writer.WriteLine("#chr\tpos\tscore");
            writer.WriteLine("21\t21757144\t-2.57");
            writer.WriteLine("21\t21757145\t3.7e-5");
            writer.Flush();
            writer.BaseStream.Position = 0;

            var parserSettings = new ParserSettings(
                new ColumnIndex(0, 1, null, null, 2, null),
                new[] {"N"},
                GenericScoreParser.NonConflictingScore
            );
            using (var streamReader = new StreamReader(writer.BaseStream))
            using (var reader = new GenericScoreParser(parserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                GenericScoreItem[] genericScoreItems = reader.GetItems().ToArray();
                Assert.Equal(2,        genericScoreItems.Length);
                Assert.Equal(-2.57,      genericScoreItems[0].Score);
                Assert.Equal(0.000037, genericScoreItems[1].Score);

            }
        }
    }
}