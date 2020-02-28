using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using SAUtils.Custom;
using SAUtils.Schema;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class GeneAnnotationParserTests
    {

        private static readonly Dictionary<string, string> EntrezGeneIdToSymbol = new Dictionary<string, string>
        {
            {"1", "Gene1" },
            {"2", "Gene2" }
        };

        private static readonly Dictionary<string, string> EnsemblIdToSymbol = new Dictionary<string, string>
        {
            {"ENSG1", "Gene1" },
            {"ENSG2", "Gene2" }
        };

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
        public void ParseHeaderLines_AsExpected()
        {
            const string headerLines = "#title=InternalGeneAnnotation\n" +
                                      "#geneSymbol\tgeneId\tOMIM Description\tIs Oncogene\tphenotype\tmimNumber\tnotes\n" +
                                      "#categories\t.\tDescription\tFilter\t\tIdentifier\t.\n" +
                                      "#descriptions\t.\tGene description from OMIM\t\tGene phenotype\t\tFree text\n" +
                                      "#type\t\tstring\tbool\tstring\tnumber\tstring\n";


            using (var parser = new GeneAnnotationsParser(GetReadStream(headerLines), EntrezGeneIdToSymbol, EnsemblIdToSymbol))
            {
                parser.ParseHeaderLines();
                var expectedJsonKeys = new[] {"OMIM Description", "Is Oncogene", "phenotype", "mimNumber", "notes"};

                var expectedCategories = new[]
                {
                    CustomAnnotationCategories.Description, CustomAnnotationCategories.Filter,
                    CustomAnnotationCategories.Unknown, CustomAnnotationCategories.Identifier,
                    CustomAnnotationCategories.Unknown
                };
                var expectedDescriptions = new[] { "Gene description from OMIM", null, "Gene phenotype", null, "Free text" };
                var expectedTypes = new[]
                {
                    SaJsonValueType.String,
                    SaJsonValueType.Bool,
                    SaJsonValueType.String,
                    SaJsonValueType.Number,
                    SaJsonValueType.String
                };

                Assert.Equal("InternalGeneAnnotation", parser.JsonTag);
                Assert.Equal(expectedJsonKeys, parser.JsonKeys);
                Assert.Equal(expectedCategories, parser.Categories);
                Assert.Equal(expectedDescriptions, parser.Descriptions);
                Assert.Equal(expectedTypes, parser.ValueTypes);
            }
        }
        
        [Fact]
        public void ParseHeaderLines_InconsistentFields()
        {
            const string invalidHeaderLines = "#title=InternalGeneAnnotation\n" +
                                              "#geneSymbol\tgeneId\tphenotype\tmimNumber\tnotes\n" +
                                              "#categories\t\t\tstring\tnumber\t.\n" +
                                              "#descriptions\t.\t.\t.\t.\tSome\tText\tHere\n" +
                                              "#type\t\t\tstring\tnumber\t.\n";

            using (var parser = new GeneAnnotationsParser(GetReadStream(invalidHeaderLines), EntrezGeneIdToSymbol, EnsemblIdToSymbol))
            {
                Assert.Throws<UserErrorException>(() => parser.ParseHeaderLines());
            }
        }

        [Fact]
        public void GetItems_UnrecognizedGeneId_ThrowException()
        {
            const string lines = "#title=InternalGeneAnnotation\n" +
                                 "#geneSymbol\tgeneId\tOMIM Description\tIs Oncogene\tphenotype\tmimNumber\tnotes\n" +
                                 "#categories\t.\tDescription\tFilter\t\tIdentifier\t.\n" +
                                 "#descriptions\t.\tGene description from OMIM\t\tGene phenotype\t\tFree text\n" +
                                 "#type\t\tstring\tbool\tstring\tnumber\tstring\n" +
                                 "Abc\t3\tsome text\ttrue\tgood\t234\ttest\n";
                                 
            using (var parser = GeneAnnotationsParser.Create(GetReadStream(lines), EntrezGeneIdToSymbol, EnsemblIdToSymbol))
            {
                Assert.Throws<UserErrorException>(() => parser.GetItems());
            }
        }


        [Fact]
        public void GetItems_SameGene_MultipleEntries_ThrowException()
        {
            const string lines = "#title=InternalGeneAnnotation\n" +
                                 "#geneSymbol\tgeneId\tOMIM Description\tIs Oncogene\tphenotype\tmimNumber\tnotes\n" +
                                 "#categories\t.\tDescription\tFilter\t\tIdentifier\t.\n" +
                                 "#descriptions\t.\tGene description from OMIM\t\tGene phenotype\t\tFree text\n" +
                                 "#type\t\tstring\tbool\tstring\tnumber\tstring\n"+
                                 "Abc\t1\tsome text\ttrue\tgood\t234\ttest\n" + 
                                 "123\tENSG1\tsome other text\tfalse\tbad\t200\ttest2\n";

            using (var parser = GeneAnnotationsParser.Create(GetReadStream(lines), EntrezGeneIdToSymbol, EnsemblIdToSymbol))
            {
                Assert.Throws<UserErrorException>(() => parser.GetItems());
            }
        }

        [Fact]
        public void GetItems_EmptyAnnotation_ThrowException()
        {
            const string lines = "#title=InternalGeneAnnotation\n" +
                                 "#geneSymbol\tgeneId\tOMIM Description\tIs Oncogene\tphenotype\tmimNumber\tnotes\n" +
                                 "#categories\t.\tDescription\tFilter\t\tIdentifier\t.\n" +
                                 "#descriptions\t.\tGene description from OMIM\t\tGene phenotype\t\tFree text\n" +
                                 "#type\t\tstring\tbool\tstring\tnumber\tstring\n" +
                                 "Abc\t1\t\t.\t\t.\t\n" +
                                 "Abc\tENSG2\tsome other text\tfalse\tbad\t200\ttest2\n";

            using (var parser = GeneAnnotationsParser.Create(GetReadStream(lines), EntrezGeneIdToSymbol, EnsemblIdToSymbol))
            {
                Assert.Throws<UserErrorException>(() => parser.GetItems());
            }
        }


        [Fact]
        public void GetItems_AsExpected()
        {
            const string lines = "#title=InternalGeneAnnotation\n" +
                                 "#geneSymbol\tgeneId\tOMIM Description\tIs Oncogene\tphenotype\tmimNumber\tnotes\n" +
                                 "#categories\t.\tDescription\tFilter\t\tIdentifier\t.\n" +
                                 "#descriptions\t.\tGene description from OMIM\t\tGene phenotype\t\tFree text\n" +
                                 "#type\t\tstring\tbool\tstring\tnumber\tstring\n" +
                                 "Abc\t1\tsome text\ttrue\tgood\t234\ttest\n" +
                                 "Abc\tENSG2\tsome other text\tfalse\tbad\t200\ttest2\n";

            using (var parser = GeneAnnotationsParser.Create(GetReadStream(lines), EntrezGeneIdToSymbol, EnsemblIdToSymbol))
            {
                var geneSymbol2Items = parser.GetItems();
                Assert.Equal(2, geneSymbol2Items.Count);
                Assert.Single(geneSymbol2Items["Gene1"]);
                Assert.Single(geneSymbol2Items["Gene2"]);
                Assert.Equal("{\"OMIM Description\":\"some text\",\"Is Oncogene\":true,\"phenotype\":\"good\",\"mimNumber\":234,\"notes\":\"test\"}", geneSymbol2Items["Gene1"][0].GetJsonString());
                Assert.Equal("{\"OMIM Description\":\"some other text\",\"phenotype\":\"bad\",\"mimNumber\":200,\"notes\":\"test2\"}", geneSymbol2Items["Gene2"][0].GetJsonString());
            }
        }

    }
}