using System.IO;
using System.Linq;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.Dann
{
    public sealed class DannParserTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##DANN");
            writer.WriteLine("#chr\tpos\tref\talt\tscore");
            writer.WriteLine("1\t10001\t10001\tT\tC\t0.4396994049749739");
            writer.WriteLine("1\t10001\t10001\tT\tG\t0.38108629377072734");
            writer.WriteLine("1\t10002\t10002\tA\tC\t0.36182020272810128");
            writer.WriteLine("1\t10002\t10002\tA\tG\t0.44413258111779291");
            writer.WriteLine("1\t10002\t10002\tA\tT\t0.16812846819989813");
            writer.WriteLine("1\t10003\t10003\tA\tC\t0.36516159615040267");
            writer.WriteLine("1\t10003\t10003\tA\tG\t0.4480978029675266");
            writer.WriteLine("1\t10003\t10003\tA\tG\taskdlj");
            writer.WriteLine("asd\t10003\t10003\tA\tG\taskdlj");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void DannReader_GetItems_AsExpected()
        {
            var nucleotides = new[] {"A", "C", "G", "T"};

            var dannParserSettings = new ParserSettings(
                new ColumnPositions(0, 2, 3, 4, 5, null),
                nucleotides,
                GenericScoreParser.MaxRepresentativeScores
            );

            using (var streamReader = new StreamReader(GetStream()))
            using (var reader = new GenericScoreParser(dannParserSettings, streamReader, ChromosomeUtilities.RefNameToChromosome))
            {
                var dannItems = reader.GetItems().ToArray();
                Assert.Equal(7, dannItems.Length);

                Assert.Equal(10001,              dannItems[0].Position);
                Assert.Equal("T",                dannItems[0].RefAllele);
                Assert.Equal("C",                dannItems[0].AltAllele);
                Assert.Equal(0.4396994049749739, dannItems[0].Score);

                Assert.Equal(10001,               dannItems[1].Position);
                Assert.Equal("T",                 dannItems[1].RefAllele);
                Assert.Equal("G",                 dannItems[1].AltAllele);
                Assert.Equal(0.38108629377072734, dannItems[1].Score);

                Assert.Equal(10002,               dannItems[4].Position);
                Assert.Equal("A",                 dannItems[4].RefAllele);
                Assert.Equal("T",                 dannItems[4].AltAllele);
                Assert.Equal(0.16812846819989813, dannItems[4].Score);
            }
        }
    }
}