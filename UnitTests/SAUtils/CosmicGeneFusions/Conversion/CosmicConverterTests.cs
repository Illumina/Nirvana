using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.Cache;
using SAUtils.CosmicGeneFusions.Conversion;
using VariantAnnotation.GeneFusions.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.Conversion
{
    public sealed class CosmicConverterTests
    {
        [Fact]
        public void Convert_ExpectedResults()
        {
            (TranscriptCache transcriptCache, ITranscript transcript, ITranscript transcript2) = HgvsRnaParserTests.GetTranscriptCache();
            Dictionary<int, HashSet<RawCosmicGeneFusion>> fusionIdToEntries = GetFusionIdToEntries(transcript, transcript2);

            ulong expectedFusionKey = GeneFusionKey.Create(
                GeneFusionKey.CreateGeneKey(transcript.Gene.EnsemblId.WithoutVersion),
                GeneFusionKey.CreateGeneKey(transcript2.Gene.EnsemblId.WithoutVersion));

            string[] expectedJsonEntries =
            {
                "\"id\":\"COSF665\",\"numSamples\":1,\"geneSymbols\":[\"MED8\",\"PTPN18\"],\"hgvsr\":\"ENST00000290663.10(MED8):r.1_3555::ENST00000347849.7(PTPN18):r.2100_3452\",\"histologies\":[{\"name\":\"ductal carcinoma\",\"numSamples\":1}],\"sites\":[{\"name\":\"breast\",\"numSamples\":1}],\"pubMedIds\":[20033038]",
                "\"id\":\"COSF667\",\"numSamples\":1,\"geneSymbols\":[\"MED8\",\"PTPN18\"],\"hgvsr\":\"ENST00000290663.10(MED8):r.1_1234::ENST00000347849.7(PTPN18):r.5678_6789\",\"histologies\":[{\"name\":\"ductal carcinoma\",\"numSamples\":1}],\"sites\":[{\"name\":\"breast\",\"numSamples\":1}],\"pubMedIds\":[20033038]"
            };

            Dictionary<ulong, string[]> actualFusionKeyToJson = CosmicConverter.Convert(fusionIdToEntries, transcriptCache);
            Assert.Single(actualFusionKeyToJson);

            string[] actualJsonEntries = actualFusionKeyToJson[expectedFusionKey];
            Assert.NotNull(actualJsonEntries);
            Assert.Equal(expectedJsonEntries, actualJsonEntries);
        }

        private static Dictionary<int, HashSet<RawCosmicGeneFusion>> GetFusionIdToEntries(ITranscript transcript, ITranscript transcript2)
        {
            string transcriptId5 = transcript.Id.WithVersion;
            string geneSymbol5   = transcript.Gene.Symbol;
            string transcriptId3 = transcript2.Id.WithVersion;
            string geneSymbol3   = transcript2.Gene.Symbol;

            var rawGeneFusion = new RawCosmicGeneFusion(749711, 665, "breast", "NS", "carcinoma", "ductal carcinoma",
                $"{transcriptId5}({geneSymbol5}):r.1_3555_{transcriptId3}({geneSymbol3}):r.2100_3452", 20033038);

            var rawGeneFusion2 = new RawCosmicGeneFusion(749712, 667, "breast", "NS", "carcinoma", "ductal carcinoma",
                $"{transcriptId5}({geneSymbol5}):r.1_1234_{transcriptId3}({geneSymbol3}):r.5678_6789", 20033038);


            return new Dictionary<int, HashSet<RawCosmicGeneFusion>>
            {
                [rawGeneFusion.FusionId]  = new() {rawGeneFusion},
                [rawGeneFusion2.FusionId] = new() {rawGeneFusion2}
            };
        }

        [Fact]
        public void ToJsonArray_ExpectedResults()
        {
            var geneKeyToJsonList = new Dictionary<ulong, List<string>>
            {
                [123] = new() {"A", "B", "C"},
                [456] = new() {"A"},
                [789] = new()
            };

            Dictionary<ulong, string[]> actualResults = geneKeyToJsonList.ToJsonArray();
            Assert.Equal(3, actualResults.Count);
            Assert.Equal(3, actualResults[123].Length);
            Assert.Single(actualResults[456]);
            Assert.Empty(actualResults[789]);
        }

        [Fact]
        public void GetCosmicGeneFusion_NullHgvs_ReturnNull()
        {
            const string hgvsNotation = "ENST00000283243.12(PLA2R1):r.1_2802";

            var fusionEntries = new HashSet<RawCosmicGeneFusion>
            {
                new(10, 0, null, null, null, null, hgvsNotation, 123)
            };

            const ulong expectedFusionKey = 0;

            (ulong actualFusionKey, string actualJson) = CosmicConverter.GetCosmicGeneFusion(0, fusionEntries, null);
            Assert.Equal(expectedFusionKey, actualFusionKey);
            Assert.Null(actualJson);
        }

        [Fact]
        public void AggregateRawCosmicGeneFusions_ExpectedResults()
        {
            const int    expectedNumSamples   = 4;
            const int    expectedNumPubMedIds = 2;
            const string hgvsNotation         = "ENST00000000123.1(ABC):r.1_1000_ENST00000000456.2(DEF):r.300_2000";
            const string expectedHgvsNotation = "ENST00000000123.1(ABC):r.1_1000::ENST00000000456.2(DEF):r.300_2000";

            var fusionEntries = new HashSet<RawCosmicGeneFusion>
            {
                new(10, 0, null, null, null, null, hgvsNotation, 123),
                new(20, 0, null, null, null, null, hgvsNotation, 123),
                new(30, 0, null, null, null, null, hgvsNotation, 200),
                new(40, 0, null, null, null, null, hgvsNotation, 123)
            };

            (int[] actualPubMedIds, int actualNumSamples, string actualHgvsNotation) = CosmicConverter.AggregateRawCosmicGeneFusions(fusionEntries);

            Assert.Equal(expectedNumSamples,   actualNumSamples);
            Assert.Equal(expectedNumPubMedIds, actualPubMedIds.Length);
            Assert.Equal(expectedHgvsNotation, actualHgvsNotation);
        }

        [Fact]
        public void AggregateRawCosmicGeneFusions_MultipleHgvsStrings_ThrowException()
        {
            const string hgvsNotation  = "ENST00000000123.1(ABC):r.1_1000_ENST00000000456.2(DEF):r.300_2000";
            const string hgvsNotation2 = "ENST00000000789.3(GHI):r.1_1000_ENST00000000456.2(DEF):r.300_2000";

            var fusionEntries = new HashSet<RawCosmicGeneFusion>
            {
                new(10, 0, null, null, null, null, hgvsNotation, 123),
                new(20, 0, null, null, null, null, hgvsNotation, 123),
                new(30, 0, null, null, null, null, hgvsNotation2, 200),
                new(40, 0, null, null, null, null, hgvsNotation, 123)
            };

            Assert.Throws<InvalidDataException>(delegate { CosmicConverter.AggregateRawCosmicGeneFusions(fusionEntries); });
        }
    }
}