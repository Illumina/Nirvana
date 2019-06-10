using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using Moq;
using SAUtils.Custom;
using SAUtils.Schema;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class ParserTests
    {
        private static readonly Dictionary<string, IChromosome> RefChromDict = new Dictionary<string, IChromosome>
        {
            {"chr1", new Chromosome("chr1", "1", 0) },
            {"chr2", new Chromosome("chr2", "2", 1) }
        };

        private static readonly ISequence Sequence = GetMockedSequence();

        private static readonly ISequenceProvider SequenceProvider = GetMockedSequenceProvider();

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
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#title=topmed"), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseTitle());
            }
        }

        [Fact]
        public void ParseTitle_IncorrectFormat()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#title:customSA"), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseTitle());
            }
        }

        [Fact]
        public void ParseGenomeAssembly_UnsupportedAssembly_ThrowException()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#assembly=hg19"), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseGenomeAssembly());
            }
        }

        [Fact]
        public void ParseGenomeAssembly_IncorrectFormat_ThrowException()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("#assembly-GRCh38"), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseGenomeAssembly());
            }
        }

        [Fact]
        public void ReadlineAndCheckPrefix_InvalidPrefix_ThrowException()
        {
            using (var custParser = new CustomAnnotationsParser(GetReadStream("invalidPrefix=someValue"), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => custParser.ReadlineAndCheckPrefix("expectedPrefix", "anyRow"));
            }
        }

        [Fact]
        public void CheckPosAndRefColumns_InvalidPosOrRef_ThrowException()
        {
            const string tagLine = "#CHROM\t\tREF\tALT\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine), null))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }

            const string tagLine2 = "#CHROM\tPOS\tREFERENCE\tALT\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine2), null))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }
        }

        [Fact]
        public void ParseTags_NoAltAndEnd_ThrowException()
        {
            const string tagLine = "#CHROM\tPOS\tREF\tNote\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine), null))
            {
                Assert.Throws<UserErrorException>(() => caParser.ParseTags());
            }
        }

        [Fact]
        public void ParseTags_LessThanFourColumn_ThrowException()
        {
            const string tagLine = "#CHROM\tPOS\tREF\n";
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagLine), null))
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
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagAndTypeLines), null))
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
            using (var caParser = new CustomAnnotationsParser(GetReadStream(tagAndTypeLines), null))
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
                                       "#matchVariantsBy=allele\n" +
                                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring";


            using (var custParser = new CustomAnnotationsParser(GetReadStream(headerLines), null))
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

            using (var custParser = new CustomAnnotationsParser(GetReadStream(invalidHeaderLines), null))
            {
                Assert.Throws<UserErrorException>(() => custParser.ParseHeaderLines());
            }
        }

        [Fact]
        public void GetItems()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n" +
                                "chr1\t14783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\n" +
                                "chr2\t10302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                                "chr2\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";
            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\"", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421", items[1].GetJsonString());
            }
        }

        [Fact]
        public void GetItems_OnlyAlleleFrequencyTreatedAsDouble_OtherNumbersPrintAsIs()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t1.000\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926";
            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(3, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\",\"anyNumber\":1.000", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"anyNumber\":3", items[1].GetJsonString());
                Assert.Equal("\"refAllele\":\"T\",\"altAllele\":\"C\",\"allAc\":10,\"allAn\":1000,\"allAf\":0.01,\"anyNumber\":100.1234567", items[2].GetJsonString());
            }
        }

        [Fact]
        public void GetItems_ExtractCustomFilters()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\tcustomFilter\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\t.\tFilter\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\tstring\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t1.000\tgood variant\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\tbad variant\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\tugly variant\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926\tvery ugly variant";
            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(3, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\",\"anyNumber\":1.000,\"customFilter\":\"good variant\"", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"anyNumber\":3,\"customFilter\":\"bad variant\"", items[1].GetJsonString());
                Assert.Equal("\"refAllele\":\"T\",\"altAllele\":\"C\",\"allAc\":10,\"allAn\":1000,\"allAf\":0.01,\"anyNumber\":100.1234567,\"customFilter\":\"ugly variant\"", items[2].GetJsonString());
            }
        }

        [Fact]
        public void GetItems_ExtractCustomFilters_failsOnLargeText()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\tcustomFilter\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\t.\tFilter\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\tstring\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t1.000\tthe good variant, the bad variant and the ugly variant\n";
                                
            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(()=>custParser.GetItems().ToArray());
                
            }
        }

        [Fact]
        public void GetItems_UnsortedData_ThrowException()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t1.000\n" +
                                "chr1\t3302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926";
            using (var caParser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => caParser.GetItems().ToArray());
            }
        }

        [Fact]
        public void GetIntervals()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";

            using (var custParser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
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

        [Fact]
        public void ExtractItems_TrimmedAndLeftShifted()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n"+
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            using (var parser = CustomAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var item = parser.ExtractItems("chr1\t12783\tA\tATA\t.\t20\t125568\t0.000159\ttrue\tVUS\t");
                Assert.Equal(12782, item.Position);
                Assert.Equal("", item.RefAllele);
                Assert.Equal("TA", item.AltAllele);
            }
        }

        private static ISequenceProvider GetMockedSequenceProvider()
        {
            var seqProviderMock = new Mock<ISequenceProvider>();
            seqProviderMock.SetupGet(x => x.RefNameToChromosome).Returns(RefChromDict);
            seqProviderMock.SetupGet(x => x.Sequence).Returns(Sequence);

            return seqProviderMock.Object;
        }

        private static ISequence GetMockedSequence()
        {
            var sequenceMock = new Mock<ISequence>();
            sequenceMock.Setup(x => x.Substring(12783, 0)).Returns("");
            sequenceMock.Setup(x => x.Substring(12283, 500)).Returns("ACGTA");
            return sequenceMock.Object;
        }
    }
}