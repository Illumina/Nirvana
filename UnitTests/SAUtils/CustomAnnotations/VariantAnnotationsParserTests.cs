﻿using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using Moq;
using SAUtils.Custom;
using SAUtils.Schema;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class VariantAnnotationsParserTests
    {
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
        public void CheckPosAndRefColumns_InvalidPosOrRef_ThrowException()
        {
            var caParser = new VariantAnnotationsParser(null, null) {Tags = new[] {"#CHROM", "", "REF", "ALT"}};

            Assert.Throws<UserErrorException>(() => caParser.CheckPosAndRefColumns());

            caParser.Tags = new[] { "#CHROM", "POS", "REFERENCE", "ALT" };
            Assert.Throws<UserErrorException>(() => caParser.CheckPosAndRefColumns());
        }

        [Fact]
        public void CheckAltAndEndColumns_NoAltAndEnd_ThrowException()
        {
            var caParser = new VariantAnnotationsParser(null, null) {Tags = new[] {"#CHROM", "POS", "REF", "Note"}};

            Assert.Throws<UserErrorException>(() => caParser.CheckAltAndEndColumns());
        }

        [Fact]
        public void ParseHeaderLines_AsExpected()
        {
            const string headerLines = "#title=IcslAlleleFrequencies \n" +
                                       "#assembly=GRCh38\t\n" +
                                       "#matchVariantsBy=allele\n" +
                                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tdeNovoQual\tnotes\n" +
                                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\tScore\t.\n" +
                                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tnumber\tstring";


            using (var custParser = new VariantAnnotationsParser(GetReadStream(headerLines), null))
            {
                custParser.ParseHeaderLines();
                var expectedJsonKeys = new[]
                    {"refAllele", "altAllele", "allAc", "allAn", "allAf", "failedFilter", "pathogenicity", "deNovoQual", "notes"};
                var expectedIntervalJsonKeys = new[]
                    {"start", "end", "allAc", "allAn", "allAf", "failedFilter", "pathogenicity", "deNovoQual", "notes"};
                var expectedCategories = new[]
                {
                    CustomAnnotationCategories.AlleleCount, CustomAnnotationCategories.AlleleNumber,
                    CustomAnnotationCategories.AlleleFrequency, CustomAnnotationCategories.Unknown,
                    CustomAnnotationCategories.Prediction, CustomAnnotationCategories.Score,
                    CustomAnnotationCategories.Unknown
                };
                var expectedDescriptions = new[] { "ALL", "ALL", "ALL", null, null, null, null };
                var expectedTypes = new[]
                {
                    SaJsonValueType.Number,
                    SaJsonValueType.Number,
                    SaJsonValueType.Number,
                    SaJsonValueType.Bool,
                    SaJsonValueType.String,
                    SaJsonValueType.Number, 
                    SaJsonValueType.String
                };

                Assert.Equal("IcslAlleleFrequencies", custParser.JsonTag);
                Assert.Equal(GenomeAssembly.GRCh38, custParser.Assembly);
                Assert.Equal(expectedJsonKeys, custParser.JsonKeys);
                Assert.Equal(expectedIntervalJsonKeys, custParser.IntervalJsonKeys);
                Assert.Equal(expectedCategories, custParser.Categories);
                Assert.Equal(expectedDescriptions, custParser.Descriptions);
                Assert.Equal(expectedTypes, custParser.ValueTypes);
            }
        }

        [Fact]
        public void ParseHeaderLines_matchBy_sv()
        {
            const string headerLines = "#title=IcslAlleleFrequencies\n" +
                                       "#assembly=GRCh38\n" +
                                       "#matchVariantsBy=sv\n" +
                                       "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                       "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                       "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                       "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring";


            using (var custParser = new VariantAnnotationsParser(GetReadStream(headerLines), null))
            {
                custParser.ParseHeaderLines();
                Assert.Equal(ReportFor.StructuralVariants, custParser.ReportFor);
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

            using (var parser = new VariantAnnotationsParser(GetReadStream(invalidHeaderLines), null))
            {
                Assert.Throws<UserErrorException>(() => parser.ParseHeaderLines());
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
            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(2, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\"", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421", items[1].GetJsonString());
            }
        }
        
        [Fact]
        public void GetIntervals_noALT()
        {
            const string text = "#title=IcslAlleleFrequencies\n"                                                          +
                                "#assembly=GRCh38\n"                                                                      +
                                "#matchVariantsBy=allele\n"                                                               +
                                "#CHROM\tPOS\tREF\tEND\tnotes\n"   +
                                "#categories\t.\t.\t.\t.\n" +
                                "#descriptions\t.\t.\t.\t.\n"                                     +
                                "#type\t.\t.\t.\tstring\n"                       +
                                "chr16\t20000000\tT\t70000000\tLots of false positives in this region";
            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Empty(items);
                var intervals = custParser.GetCustomIntervals();
                Assert.Single(intervals);
                Assert.Equal("\"start\":20000000,\"end\":70000000,\"notes\":\"Lots of false positives in this region\"", intervals[0].GetJsonString());
            }
        }
        
        [Fact]
        public void GetIntervals_start()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n"             +
                                "#matchVariantsBy=allele\n"      +
                                "#CHROM\tPOS\tREF\tALT\tEND\tnotes\n" +
                                "#categories\t.\t.\t.\t.\t.\n"      +
                                "#descriptions\t.\t.\t.\t.\t.\n"    +
                                "#type\t.\t.\t.\t.\tstring\n"       +
                                "chr21\t10510818\tT\t.\t10699435\tinterval 1\n"+
                                "chr21\t10510818\tT\t<DEL>\t10699435\tinterval 2";
            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Empty(items);
                var intervals = custParser.GetCustomIntervals();
                Assert.Equal(2,intervals.Count);
                Assert.Equal("\"start\":10510818,\"end\":10699435,\"notes\":\"interval 1\"", intervals[0].GetJsonString());
                Assert.Equal("\"start\":10510819,\"end\":10699435,\"notes\":\"interval 2\"", intervals[1].GetJsonString());
            }
        }

        [Fact]
        public void GetItems_OnlyAlleleFrequencyTreatedAsDouble_OtherNumbersPrintAsIs()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\tscore\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t1.000\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926";
            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(3, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\",\"anyNumber\":1.000", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"anyNumber\":3", items[1].GetJsonString());
                Assert.Equal("\"refAllele\":\"T\",\"altAllele\":\"C\",\"allAc\":10,\"allAn\":1000,\"allAf\":0.01,\"anyNumber\":100.1234567", items[2].GetJsonString());
            }
        }
        
        [Fact]
        public void GetItems_invalid_scores()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\tscore\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t1.0\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n" +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t\tthree";
            using (var parser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(()=> parser.GetItems().ToArray());
            }
        }
        
        [Fact]
        public void GetItems_missing_scores()
        {
            const string text = "#title=IcslAlleleFrequencies\n"                                                                   +
                                "#assembly=GRCh38\n"                                                                               +
                                "#matchVariantsBy=allele\n"                                                                        +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\tanyNumber\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\tscore\n"   +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\t.\n"                                           +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\tnumber\n"                        +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\t.\n"                                     +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\t3\n"                                        +
                                "chr1\t18972\tT\tC\t.\t10\t1000\t0.01\tfalse\t.\t\t100.1234567\n";
                                
            using (var parser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = parser.GetItems().ToArray();
                
                Assert.DoesNotContain("anyNumber", items[0].GetJsonString());
                Assert.Contains("anyNumber", items[1].GetJsonString());
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
            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(3, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\",\"anyNumber\":1.000,\"customFilter\":\"good variant\"", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"anyNumber\":3,\"customFilter\":\"bad variant\"", items[1].GetJsonString());
                Assert.Equal("\"refAllele\":\"T\",\"altAllele\":\"C\",\"allAc\":10,\"allAn\":1000,\"allAf\":0.01,\"anyNumber\":100.1234567,\"customFilter\":\"ugly variant\"", items[2].GetJsonString());
            }
        }
        
        [Fact]
        public void GetItems_missing_filter()
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
                                "chr1\t46993\tA\tG\t.\t50\t250\t0.001\tfalse\tbenign\t\t3.1415926\t.";
            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(4, items.Length);
                Assert.Equal("\"refAllele\":\"G\",\"altAllele\":\"A\",\"allAc\":20,\"allAn\":125568,\"allAf\":0.000159,\"failedFilter\":true,\"pathogenicity\":\"VUS\",\"anyNumber\":1.000,\"customFilter\":\"good variant\"", items[0].GetJsonString());
                Assert.Equal("\"refAllele\":\"C\",\"altAllele\":\"A\",\"allAc\":53,\"allAn\":8928,\"allAf\":0.001421,\"anyNumber\":3,\"customFilter\":\"bad variant\"", items[1].GetJsonString());
                Assert.Equal("\"refAllele\":\"T\",\"altAllele\":\"C\",\"allAc\":10,\"allAn\":1000,\"allAf\":0.01,\"anyNumber\":100.1234567,\"customFilter\":\"ugly variant\"", items[2].GetJsonString());
                Assert.DoesNotContain("customFilter",items[3].GetJsonString());
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

            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => custParser.GetItems().ToArray());

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
            using (var caParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                Assert.Throws<UserErrorException>(() => caParser.GetItems().ToArray());
            }
        }

        [Fact]
        public void GetIntervals()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=sv\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n" +
                                "chr1\t12783\tG\tA\t.\t20\t125568\t0.000159\ttrue\tVUS\t\n" +
                                "chr1\t13302\tC\tA\t.\t53\t8928\t0.001421\tfalse\t.\t\n" +
                                "chr1\t46993\tA\t<DEL>\t50879\t50\t250\t0.001\tfalse\tbenign\t";

            using (var custParser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var items = custParser.GetItems().ToArray();
                Assert.Equal(ReportFor.StructuralVariants, custParser.ReportFor);
                Assert.Equal(2, items.Length);

                var intervals = custParser.GetCustomIntervals();
                Assert.Single(intervals);
                Assert.Equal("\"start\":46994,\"end\":50879,\"allAc\":50,\"allAn\":250,\"allAf\":0.001,\"pathogenicity\":\"benign\"", intervals[0].GetJsonString());
            }
        }

        [Fact]
        public void IsValidNucleotideSequence_IsValidSequence_Pass()
        {
            Assert.True(VariantAnnotationsParser.IsValidAltAllele("actgnACTGN"));
            Assert.True(VariantAnnotationsParser.IsValidAltAllele("AAAAAAAAAAAAAAAAAATTAGTCAGGCAC[chr3:153444911["));
            Assert.False(VariantAnnotationsParser.IsValidAltAllele("AC-GT"));
        }

        [Fact]
        public void ExtractItems_TrimmedAndLeftShifted()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            using (var parser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                var item = parser.ExtractItems("chr1\t12783\tA\tATA\t.\t20\t125568\t0.000159\ttrue\tVUS\t");
                Assert.Equal(12782, item.Position);
                Assert.Equal("", item.RefAllele);
                Assert.Equal("TA", item.AltAllele);
            }
        }

        [Fact]
        public void Extract_symbolic_alleles()
        {
            const string text = "#title=IcslAlleleFrequencies\n"                                                          +
                                "#assembly=GRCh38\n"                                                                      +
                                "#matchVariantsBy=allele\n"                                                               +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n"   +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n"                                     +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            using (var parser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                parser.ExtractItems("chr1\t12783\tA\t<DEL>\t24486\t20\t125568\t0.000159\ttrue\tVUS\t");
                var intervals = parser.GetCustomIntervals();
                Assert.Single(intervals);
                Assert.Equal(12784, intervals[0].Start);
                Assert.Equal(24486, intervals[0].End);

            }
        }

        [Fact]
        public void ParseTitle_Conflict_JsonTag()
        {
            const string text = "#title=topmed\n"                                                          +
                                "#assembly=GRCh38\n"                                                                      +
                                "#matchVariantsBy=allele\n"                                                               +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n"   +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n"                                     +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            Assert.Throws<UserErrorException>(() => VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider));
            
        }

        [Fact]
        public void ParseTitle_IncorrectFormat()
        {
            const string text = "#title:IcslAlleleFrequencies\n"                                                          +
                                "#assembly=GRCh38\n"                                                                      +
                                "#matchVariantsBy=allele\n"                                                               +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n"   +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n"                                     +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            Assert.Throws<UserErrorException>(() => VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider));
        }

        [Fact]
        public void ParseGenomeAssembly_UnsupportedAssembly_ThrowException()
        {
            const string text = "#title=IcslAlleleFrequencies\n"                                                          +
                                "#assembly=hg20\n"                                                                      +
                                "#matchVariantsBy=allele\n"                                                               +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n"   +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n"                                     +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            Assert.Throws<UserErrorException>(() => VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider));
        }

        [Fact]
        public void ParseGenomeAssembly_IncorrectFormat_ThrowException()
        {
            const string text = "#title=IcslAlleleFrequencies\n"                                                          +
                                "#assembly-hg20\n"                                                                      +
                                "#matchVariantsBy=allele\n"                                                               +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n"   +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n"                                     +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            Assert.Throws<UserErrorException>(() => VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider));
        }


        [Fact]
        public void ParseHeader_version_and_description()
        {
            const string text = "#title=IcslAlleleFrequencies\n" +
                                "#assembly=GRCh38\n" +
                                "#version=v4.5\t\n"+
                                "#description=Internal allele frequencies\t\n" +
                                "#matchVariantsBy=allele\n" +
                                "#CHROM\tPOS\tREF\tALT\tEND\tallAc\tallAn\tallAf\tfailedFilter\tpathogenicity\tnotes\n" +
                                "#categories\t.\t.\t.\t.\tAlleleCount\tAlleleNumber\tAlleleFrequency\t.\tPrediction\t.\n" +
                                "#descriptions\t.\t.\t.\t.\tALL\tALL\tALL\t.\t.\t.\n" +
                                "#type\t.\t.\t.\t.\tnumber\tnumber\tnumber\tbool\tstring\tstring\n";

            using (var parser = VariantAnnotationsParser.Create(GetReadStream(text), SequenceProvider))
            {
                Assert.Equal("v4.5", parser.Version);
                Assert.Equal("Internal allele frequencies", parser.DataSourceDescription);
            }
        }

        private static ISequenceProvider GetMockedSequenceProvider()
        {
            var seqProviderMock = new Mock<ISequenceProvider>();
            seqProviderMock.SetupGet(x => x.RefNameToChromosome).Returns(ChromosomeUtilities.RefNameToChromosome);
            seqProviderMock.SetupGet(x => x.Sequence).Returns(Sequence);

            return seqProviderMock.Object;
        }

        private static ISequence GetMockedSequence()
        {
            var sequenceMock = new Mock<ISequence>();
            sequenceMock.Setup(x => x.Substring(12783, 0)).Returns("");
            sequenceMock.Setup(x => x.Substring(12733, 50)).Returns("ACGTA");
            sequenceMock.Setup(x => x.Substring(12283, 500)).Returns("ACGTA");
            return sequenceMock.Object;
        }
    }
}