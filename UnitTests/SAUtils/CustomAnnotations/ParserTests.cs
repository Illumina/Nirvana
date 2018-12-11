using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                custParser.ParseHeaderLine("#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes");
                custParser.ParseHeaderLine("#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.");
                custParser.ParseHeaderLine("#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.");
                custParser.ParseHeaderLine("#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring");

                var item = custParser.ExtractItems("chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t");

                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"pathogenicity\":\"VUSS\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true", item.GetJsonString());
            }
        }

        [Fact]
        public void GetItems()
        {
            var text = "#title=IcslAlleleFrequencies\n" +
                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n"+
                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n"+
                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n"+
                       "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t\n" +
                       "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                       "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";
            using (var custParser = new CustomAnnotationsParser(GetReadStream(text), _refChromDict))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"pathogenicity\":\"VUSS\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"failedFilter\":false", items[1].GetJsonString());
            }
        }

        [Fact]
        public void GetIntervals()
        {
            var text = "#title=IcslAlleleFrequencies\n" +
                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n" +
                       "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t\n" +
                       "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                       "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";

            using (var custParser = new CustomAnnotationsParser(GetReadStream(text), _refChromDict))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);

                var intervals = custParser.GetCustomIntervals();
                Assert.Single(intervals);
                Assert.Equal("\"start\":46993,\"end\":50879,\"pathogenicity\":\"benign\",\"allAc\":50,\"allAn\":250,\"allAf\":0.001,\"failedFilter\":false", intervals[0].GetJsonString());
            }
        }

    }
}