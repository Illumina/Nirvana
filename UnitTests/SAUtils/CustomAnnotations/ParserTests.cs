using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using SAUtils.Custom;
using SAUtils.Schema;
using UnitTests.TestUtilities;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class ParserTests
    {
        private static StreamReader GetReadStream(string text)
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
        public void ParseTitle_Conflict_JsonTag()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#title=topmed"), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseTitle());
            }
        }

        [Fact]
        public void ParseTitle_IncorrectFormat()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#title:customSA"), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseTitle());
            }
        }

        [Fact]
        public void ParseGenomeAssembly_UnsupportedAssembly_ThrowException()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#assembly=hg19"), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseGenomeAssembly());
            }
        }

        [Fact]
        public void ParseGenomeAssembly_IncorrectFormat_ThrowException()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#assembly-GRCh38"), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseGenomeAssembly());
            }
        }

        [Fact]
        public void ReadlineAndCheckPrefix_InvalidPrefix_ThrowException()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("invalidPrefix=someValue"), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => custParser.ReadlineAndCheckPrefix("expectedPrefix", "anyRow"));
            }
        }

        [Fact]
        public void CheckPosAndRefColumns_InvalidPosOrRef_ThrowException()
        {
            const string tagLine = "#CHROM\t\tREF\tALT\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }

            const string tagLine2 = "#CHROM\tPOS\tREFERENCE\tALT\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine2), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }
        }

        [Fact]
        public void ParseTags_NoAltAndEnd_ThrowException()
        {
            const string tagLine = "#CHROM\tPOS\tREF\tNote\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }
        }

        [Fact]
        public void ParseTags_LessThanFourColumn_ThrowException()
        {
            const string tagLine = "#CHROM\tPOS\tREF\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }
        }

        [Theory]
        [InlineData("String")]
        [InlineData("NUMBER")]
        [InlineData("Bool")]
        public void ParseTypes_ValidType_Pass(string type)
        {
            string tagAndTypeLines = "#CHROM\tPOS\tREF\tALT\tValue\n" +
                                     $"#type\t.\t.\t.\t{type}";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagAndTypeLines), ChromosomeUtilities.RefNameToChromosome))
            {
                caParser.ParseTags();
                caParser.ParseTypes();
            }
        }

        [Theory]
        [InlineData("boolean")]
        [InlineData("double")]
        [InlineData("int")]
        public void ParseTypes_InvalidType_ThrowException(string type)
        {
            string tagAndTypeLines = "#CHROM\tPOS\tREF\tALT\tValue\n" +
                                     $"#type\t.\t.\t.\t{type}";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagAndTypeLines), ChromosomeUtilities.RefNameToChromosome))
            {
                caParser.ParseTags();
                Assert.Throws<UserErrorException>(() => caParser.ParseTypes());
            }
        }

        [Fact]
        public void ParseHeaderLines_AsExpected()
        {
            const string headerLines = "#title=IcslAlleleFrequencies\n" +
                                       "#assembly=GRCh38\n" +
                                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring";


            using (var custParser = new CustomAnnotationsParser(GetReadStream(headerLines), ChromosomeUtilities.RefNameToChromosome))
            {
                custParser.ParseHeaderLines();
                var expectedJsonKeys = new[]
                    {"refAllele", "altAllele", "allAc", "allAn", "allAf", "failedFilter", "pathogenicity", "notes"};
                var expectedIntervalJsonKeys = new[]
                    {"start", "end", "allAc", "allAn", "allAf", "failedFilter", "pathogenicity", "notes"};
                var expectedCategories = new[]
                {
                    CustomAnnotationCategories.AlleleCount, CustomAnnotationCategories.AlleleNumber,
                    CustomAnnotationCategories.AlleleFrequency, CustomAnnotationCategories.Unknown,
                    CustomAnnotationCategories.Prediction, CustomAnnotationCategories.Unknown
                };
                var expectedDescriptions = new[] { "ALL", "ALL", "ALL", null, null, null };
                var expectedTypes = new[]
                {
                    SaJsonValueType.Number,
                    SaJsonValueType.Number,
                    SaJsonValueType.Number,
                    SaJsonValueType.Bool,
                    SaJsonValueType.String,
                    SaJsonValueType.String
                };

                Assert.Equal("IcslAlleleFrequencies", custParser.JsonTag);
                Assert.Equal(GenomeAssembly.GRCh38, custParser.Assembly);
                Assert.True(expectedJsonKeys.SequenceEqual(custParser.JsonKeys));
                Assert.True(expectedIntervalJsonKeys.SequenceEqual(custParser.IntervalJsonKeys));
                Assert.True(expectedCategories.SequenceEqual(custParser.Categories));
                Assert.True(expectedDescriptions.SequenceEqual(custParser.Descriptions));
                Assert.Equal(expectedTypes, custParser.ValueTypes);
            }
        }

        [Fact]
        public void ParseHeaderLines_InconsistentFields()
        {
            const string invalidHeaderLines = "#title=IcslAlleleFrequencies\n" +
                                       "#assembly=GRCh38\n" +
                                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\n" +
                                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\tMore\n" +
                                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\n" +
                                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring";

            using (var custParser = new CustomAnnotationsParser(GetReadStream(invalidHeaderLines), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseHeaderLines());
            }
        }

        [Fact]
        public void GetItems()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n" +
                                "chr1\t14783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t\n" +
                                "chr2\t10302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                                "chr2\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";
            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), ChromosomeUtilities.RefNameToChromosome))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUSS\"", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421", items[1].GetJsonString());
            }
        }

        [Fact]
        public void GetItems_OnlyAlleleFrequencyTreatedAsDouble_OtherNumbersPrintAsIs()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t\t1.000\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926";
            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), ChromosomeUtilities.RefNameToChromosome))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(3, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUSS\",\"anyNumber\":1.000", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"anyNumber\":3", items[1].GetJsonString());
                Assert.Equal("\"refAllele\":\"T\",\"altAllele\":\"C\",\"allAc\":10,\"allAn\":1000,\"allAf\":0.01,\"anyNumber\":100.1234567", items[2].GetJsonString());
            }
        }

        [Fact]
        public void GetItems_UnsortedData_ThrowException()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t\t1.000\n" +
                                "chr1\t3302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926";
            using (var caParser = CustomAnnotationsParser.Create(GetReadStream(text), ChromosomeUtilities.RefNameToChromosome))
            {
                Assert.Throws<UserErrorException>(() => caParser.GetItems().ToArray());
            }
        }

        [Fact]
        public void GetIntervals()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUSS\t\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";

            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), ChromosomeUtilities.RefNameToChromosome))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);

                var intervals = custParser.GetCustomIntervals();
                Assert.Single(intervals);
                Assert.Equal("\"start\":46993,\"end\":50879,\"allAc\":50,\"allAn\":250,\"allAf\":0.001,\"pathogenicity\":\"benign\"", intervals[0].GetJsonString());
            }
        }

        [Fact]
        public void IsValidNucleotideSequence_IsValidSequence_Pass()
        {
           Assert.True(CustomAnnotationsParser.IsValidNucleotideSequence("actgnACTGN"));
           Assert.False(CustomAnnotationsParser.IsValidNucleotideSequence("AC-GT"));
        }
    }
}