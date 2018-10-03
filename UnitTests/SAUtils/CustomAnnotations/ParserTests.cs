using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using SAUtils.Custom;
using Xunit;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class ParserTests
    {
        private readonly Dictionary<string, IChromosome> _refChromDict= new Dictionary<string, IChromosome>()
        {
            {"chr1", new Chromosome("chr1", "1", 0) }
        };

        private StreamReader GetReadStream(string text)
        {
            byte[] data;
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            {
                writer.Write(text);
                writer.Flush();
                data = memStream.ToArray();
            }

            return new StreamReader(new MemoryStream(data));
        }

        [Fact]
        public void HeaderLine_valid_title()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#title=IcslAlleleFrequencies"), _refChromDict))
            {
                Assert.Equal("IcslAlleleFrequencies", custParser.JsonTag);
            }
        }

        [Fact]
        public void HeaderLine_invalid_title()
        {
            Assert.Throws<UserErrorException>(() => new CustomAnnotationsParser(GetReadStream("#title=topmed"), _refChromDict));
        }

        [Fact]
        public void HeaderLine_fieldTags()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#title=IcslAlleleFrequencies"), _refChromDict))
            {
                custParser.ParseHeaderLine("#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tallHc\tfailedFilter");
                var item = custParser.ExtractItems("chr1\t100\tA\tC\t.\t295\t125568\t0.000159\t0\ttrue");

                Assert.Equal("\"allAc\":\"295\",\"allAn\":\"125568\",\"allAf\":\"0.000159\",\"allHc\":\"0\",\"failedFilter\":\"true\"", item.GetJsonString());
            }
        }

        [Fact]
        public void GetItems()
        {
            var text = "#title=IcslAlleleFrequencies\n" +
                       "#CHROM\tPOS\tREF\tALT\tEND\ttype\tallAc\tallAn\tallAf\tallHc\tfailedFilter\n" +
                       "chr1\t100\tA\tC\t.\t.\t295\t125568\t0.000159\t0\ttrue\n" +
                       "chr1\t102\tT\tG\t.\t.\t79\t100981\t0.000325\t1\tfalse\n" +
                        "chr1\t200\tT\t.\t1250\tDEL\t20\t253\t0.0003\t2\ttrue";
            using (var custParser = new CustomAnnotationsParser(GetReadStream(text), _refChromDict))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);
                Assert.Equal("\"allAc\":\"295\",\"allAn\":\"125568\",\"allAf\":\"0.000159\",\"allHc\":\"0\",\"failedFilter\":\"true\"", items[0].GetJsonString());
                Assert.Equal("\"allAc\":\"79\",\"allAn\":\"100981\",\"allAf\":\"0.000325\",\"allHc\":\"1\",\"failedFilter\":\"false\"", items[1].GetJsonString());
            }
        }

        [Fact]
        public void GetIntervals()
        {
            var text = "#title=IcslAlleleFrequencies\n" +
                       "#CHROM\tPOS\tREF\tALT\tEND\ttype\tallAc\tallAn\tallAf\tallHc\tfailedFilter\n" +
                       "chr1\t100\tA\tC\t.\t.\t295\t125568\t0.000159\t0\ttrue\n" +
                       "chr1\t102\tT\tG\t.\t.\t79\t100981\t0.000325\t1\tfalse\n" +
                       "chr1\t200\tT\t.\t1250\tDEL\t20\t253\t0.0003\t2\ttrue";
            using (var custParser = new CustomAnnotationsParser(GetReadStream(text), _refChromDict))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);

                var intervals = custParser.GetCustomIntervals();
                Assert.Single(intervals);
                Assert.Equal("\"start\":\"200\",\"end\":\"1250\",\"type\":\"DEL\",\"allAc\":\"20\",\"allAn\":\"253\",\"allAf\":\"0.0003\",\"allHc\":\"2\",\"failedFilter\":\"true\"", intervals[0].GetJsonString());
            }
        }

    }
}