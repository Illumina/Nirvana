using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.IO;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.IO
{
    public sealed class GeneFusionJsonWriterTests
    {
        [Fact]
        public void GeneFusionJsonWriter_ExpectedResults()
        {
            Dictionary<ulong, string[]> expectedGeneKeyToJson = GetKeyToJson();
            IDataSourceVersion          expectedVersion       = new DataSourceVersion("COSMIC Gene Fusions", "102", DateTime.Now.Ticks, "COSMIC");

            using var ms = new MemoryStream();
            using (var writer = new GeneFusionJsonWriter(ms, "cosmicGeneFusions", expectedVersion, true))
            {
                writer.Write(expectedGeneKeyToJson);
            }

            ms.Position = 0;

            Dictionary<ulong, string[]> actualGeneKeyToJson;
            IDataSourceVersion          actualVersion;

            using (var reader = new GeneFusionJsonReader(ms))
            {
                reader.LoadAnnotations();
                actualGeneKeyToJson = reader.FusionKeyToFusions;
                actualVersion       = reader.Version;
            }

            Assert.Equal(expectedVersion,             actualVersion, new DataSourceVersionComparer());
            Assert.Equal(expectedGeneKeyToJson.Count, actualGeneKeyToJson.Count);
            foreach (ulong geneKey in expectedGeneKeyToJson.Keys)
            {
                Assert.Equal(expectedGeneKeyToJson[geneKey], actualGeneKeyToJson[geneKey]);
            }
        }

        private static Dictionary<ulong, string[]> GetKeyToJson()
        {
            var geneKeyToFusion = new Dictionary<ulong, string[]>();

            var json =
                "\"id\":\"COSF2245\",\"numSamples\":13,\"geneSymbols\":[\"ETV6\",\"RUNX1\"],\"hgvsr\":\"ENST00000396373.8(ETV6):r.1_1283_ENST00000300305.7(RUNX1):r.504_6222\",\"histologies\":[{\"histology\":\"lymphoid neoplasm\",\"numSamples\":14}],\"sites\":[{\"site\":\"haematopoietic and lymphoid tissue\",\"numSamples\":11}]";
            var json2 =
                "\"id\":\"COSF100\",\"numSamples\":2,\"geneSymbols\":[\"A\",\"B\"],\"hgvsr\":\"ENST00000396373.8(A):r.1_1283_ENST00000300305.7(B):r.504_6222\",\"histologies\":[{\"histology\":\"lymphoid neoplasm\",\"numSamples\":14}]";
            var json3 =
                "\"id\":\"COSF200\",\"numSamples\":7,\"geneSymbols\":[\"C\",\"D\"],\"hgvsr\":\"ENST00000396373.8(C):r.1_1283_ENST00000300305.7(D):r.504_6222\",\"sites\":[{\"site\":\"haematopoietic and lymphoid tissue\",\"numSamples\":11}]";

            geneKeyToFusion[1000] = new[] {json, json2};
            geneKeyToFusion[2000] = new[] {json3};

            return geneKeyToFusion;
        }
    }
}