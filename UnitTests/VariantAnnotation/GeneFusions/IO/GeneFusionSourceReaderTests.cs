using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IO.v2;
using SAUtils.FusionCatcher;
using UnitTests.SAUtils.FusionCatcher;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.IO
{
    public sealed class GeneFusionSourceReaderTests
    {
        [Theory]
        [InlineData(FileType.FusionCatcher,  GeneFusionSourceReader.SupportedFileFormatVersion, true)]
        [InlineData(FileType.GeneFusionJson, GeneFusionSourceReader.SupportedFileFormatVersion, false)]
        [InlineData(FileType.FusionCatcher,  0,                                                 false)]
        public void CheckHeader_ExpectedResults(FileType fileType, ushort fileFormatVersion, bool expectedIsValid)
        {
            Exception ex            = Record.Exception(() => { GeneFusionSourceReader.CheckHeader(fileType, fileFormatVersion); });
            bool      actualIsValid = ex == null;
            Assert.Equal(expectedIsValid, actualIsValid);
        }

        [Fact]
        public void AddAnnotations_ExpectedResults()
        {
            const string expectedJson =
                "[{\"genes\":{\"first\":{\"hgnc\":\"A\",\"isOncogene\":true},\"second\":{\"hgnc\":\"B\"},\"isParalogPair\":true},\"germlineSources\":[\"1000 Genomes Project\",\"Healthy (strong support)\",\"Illumina Body Map 2.0\"],\"somaticSources\":[\"Alaei-Mahabadi 18 cancers\",\"DepMap CCLE\"]},{\"genes\":{\"first\":{\"hgnc\":\"E\"},\"second\":{\"hgnc\":\"F\"}},\"somaticSources\":[\"CCLE Vellichirammal\",\"Cancer Genome Project\"]}]";

            using var ms = new MemoryStream();
            WriteGeneFusionSourceFile(ms);

            var supplementaryAnnotations = new List<ISupplementaryAnnotation>();

            IGeneFusionPair[] fusionPairs =
            {
                new GeneFusionPair(1000, "A", 123, "B", 456),
                new GeneFusionPair(1500, "C", 234, "D", 567), // no matching SA
                new GeneFusionPair(3000, "E", 345, "F", 678)
            };

            using (var reader = new GeneFusionSourceReader(ms))
            {
                reader.LoadAnnotations();
                reader.AddAnnotations(fusionPairs, supplementaryAnnotations);
            }

            Assert.Single(supplementaryAnnotations);
            ISupplementaryAnnotation sa = supplementaryAnnotations[0];

            var sb = new StringBuilder();
            sa.SerializeJson(sb);
            var actualJson = sb.ToString();

            Assert.Equal("fusionCatcher", sa.JsonKey);
            Assert.Equal(expectedJson,    actualJson);
        }

        [Fact]
        public void AddAnnotations_NoResults()
        {
            using var ms = new MemoryStream();
            WriteGeneFusionSourceFile(ms);

            var supplementaryAnnotations = new List<ISupplementaryAnnotation>();

            IGeneFusionPair[] fusionPairs =
            {
                new GeneFusionPair(1500, "C", 234, "D", 567) // no matching SA
            };

            using (var reader = new GeneFusionSourceReader(ms))
            {
                reader.LoadAnnotations();
                reader.AddAnnotations(fusionPairs, supplementaryAnnotations);
            }

            Assert.Empty(supplementaryAnnotations);
        }

        private static void WriteGeneFusionSourceFile(MemoryStream ms)
        {
            (uint[] expectedOncogeneKeys, GeneFusionSourceCollection[] expectedIndex, GeneFusionIndexEntry[] expectedIndexEntries) =
                GeneFusionSourceWriterTests.GetKeyToGeneFusion();

            IDataSourceVersion expectedVersion = new DataSourceVersion("FusionCatcher", "1.33", DateTime.Now.Ticks, "gene fusions");
            const string       expectedJsonKey = "fusionCatcher";

            using (var writer = new GeneFusionSourceWriter(ms, expectedJsonKey, expectedVersion, true))
            {
                writer.Write(expectedOncogeneKeys, expectedIndex, expectedIndexEntries);
            }

            ms.Position = 0;
        }
    }
}