using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.Cache;
using SAUtils.CosmicGeneFusions.Conversion;
using VariantAnnotation.GeneFusions.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.Conversion
{
    public sealed class HgvsRnaParserTests
    {
        [Theory]
        [InlineData(             "ENST00000332149.5(TMPRSS2):r.1_79+?_ENST00000442448.1(ERG):r.312_5034", "ENST00000332149.5", "ENST00000442448.1")]
        [InlineData(             "ENST00000415083.2(SS18):r.1_1286_ENST00000415083.2(SS18):r.1286+683_1286+701_ENST00000336777.5(SSX2):r.351_1410",
            "ENST00000415083.2", "ENST00000336777.5")]
        [InlineData(
            "ENST00000397938.2(EWSR1):r.1_1112_ENST00000527786.2(FLI1):r.1079_1144_ENST00000527786.2(FLI1):r.1145-1478_1145-1410_ENST00000527786.2(FLI1):r.1145_4127",
            "ENST00000397938.2", "ENST00000527786.2")]
        [InlineData("ENST00000305877.8(BCR):r.1_2866::ENST00000372348.2(ABL1):r.511-?_511-?::ENST00000318560.5(ABL1):r.461_5766", "ENST00000305877.8",
            "ENST00000318560.5")]
        [InlineData(              "ENST00000305877.12(BCR):r.1_2866::ENST00000372348.6(ABL1):r.511-?_511-?::ENST00000318560.5(ABL1):r.461_5766",
            "ENST00000305877.12", "ENST00000318560.5")]
        public void Parse_ExpectedResults(string hgvsString, string expectedTranscriptId5, string expectedTranscriptId3)
        {
            (string actualTranscriptId5, string actualTranscriptId3) = HgvsRnaParser.Parse(hgvsString);
            Assert.Equal(expectedTranscriptId5, actualTranscriptId5);
            Assert.Equal(expectedTranscriptId3, actualTranscriptId3);
        }

        [Theory]
        [InlineData("ENST00000305877.8(BCR):r.1_2866")]
        [InlineData("ENST00000000123.1(ABC):r.1_2866::ENST00000000456.2(ABC):r.511-?_511-?::ENST00000000789.3(ABC):r.461_5766")]
        public void Parse_UnexpectedTranscriptCount_ThrowException(string hgvsString)
        {
            Assert.Throws<InvalidDataException>(delegate { HgvsRnaParser.Parse(hgvsString); });
        }

        [Fact]
        public void GetTranscripts_ExpectedResults()
        {
            (TranscriptCache transcriptCache, ITranscript transcript, ITranscript transcript2) = GetTranscriptCache();
            string[] expectedGeneSymbols = {transcript.Gene.Symbol, transcript2.Gene.Symbol};
            ulong    expectedFusionKey   = GeneFusionKey.Create(transcript.Gene.EnsemblId.WithoutVersion, transcript2.Gene.EnsemblId.WithoutVersion);

            (string[] actualGeneSymbols, ulong actualFusionKey) =
                HgvsRnaParser.GetTranscripts("ENST00000290663.10(MED8):r.1_79+?_ENST00000347849.7(ERG):r.312_5034", transcriptCache);

            Assert.Equal(expectedGeneSymbols, actualGeneSymbols);
            Assert.Equal(expectedFusionKey,   actualFusionKey);
        }

        public static (TranscriptCache TranscriptCache, ITranscript Transcript, ITranscript Transcript2) GetTranscriptCache()
        {
            ITranscript transcript  = MockedData.Transcripts.ENST00000290663;
            ITranscript transcript2 = MockedData.Transcripts.ENST00000347849;

            var idToTranscript = new Dictionary<string, ITranscript>
            {
                [transcript.Id.WithoutVersion]  = transcript,
                [transcript2.Id.WithoutVersion] = transcript2
            };

            return (new TranscriptCache(idToTranscript), transcript, transcript2);
        }
    }
}