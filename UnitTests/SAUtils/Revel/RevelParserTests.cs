using System.IO;
using System.Linq;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.Revel
{
    public sealed class RevelParserTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##REVEL");
            writer.WriteLine("#chr\tpos\tref\talt\trefAA\taltAA\tscore");
            writer.WriteLine("1\t35290\tG\tA\tP\tD\t0.035");
            writer.WriteLine("1\t35290\tG\tA\tP\tS\t0.031");
            writer.WriteLine("1\t35290\tG\tC\tP\tA\t0.040");
            writer.WriteLine("1\t35290\tG\tT\tP\tT\t0.035");
            writer.WriteLine("1\t35290\tG\tC\tP\tA\t0.063");
            writer.WriteLine("1\t35291\tG\tC\tF\tL\t0.022");
            writer.WriteLine("1\t35291\tG\tT\tF\tL\t0.022");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void RevelReader_GetItems_AsExpected()
        {
            var nucleotides = new[] {"A", "C", "G", "T"};

            var revelParserSettings = new ParserSettings(
                new ColumnPositions(0, 1, 2, 3, 6, null),
                nucleotides,
                GenericScoreParser.MaxRepresentativeScores
            );
            
            using (var streamReader = new StreamReader(GetStream()))
            using (var reader = new GenericScoreParser(revelParserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                var revelItems = reader.GetItems().ToArray();
                Assert.Equal(5,                 revelItems.Length);
                Assert.Equal(35290,             revelItems[0].Position);
                Assert.Equal("G",               revelItems[0].RefAllele);
                Assert.Equal("A",               revelItems[0].AltAllele);
                Assert.Equal("\"score\":0.035", revelItems[0].GetJsonString());
                Assert.Equal(35290,             revelItems[1].Position);
                Assert.Equal("G",               revelItems[1].RefAllele);
                Assert.Equal("C",               revelItems[1].AltAllele);
                Assert.Equal("\"score\":0.063", revelItems[1].GetJsonString());
                Assert.Equal(35291,             revelItems[4].Position);
                Assert.Equal("G",               revelItems[4].RefAllele);
                Assert.Equal("T",               revelItems[4].AltAllele);
                Assert.Equal("\"score\":0.022", revelItems[4].GetJsonString());
            }
        }
    }
}