using System.IO;
using System.Text;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class EncodedTranscriptDataTests
    {
        [Fact]
        public void EncodedTranscriptData_EndToEnd()
        {
            const BioType expectedBiotype       = BioType.non_stop_decay;
            const bool expectedCdsStartNotFound = true;
            const bool expectedCdsEndNotFound   = true;
            const Source expectedSource         = Source.BothRefSeqAndEnsembl;
            const bool expectedCanonical        = true;
            const bool expectedSift             = true;
            const bool expectedPolyPhen         = true;
            const bool expectedMirnas           = true;
            const bool expectedRnaEdits         = true;
            const bool expectedSelenocysteines  = true;
            const bool expectedIntrons          = true;
            const bool expectedCdnaMaps         = true;
            const bool expectedTranslation      = true;
            const byte expectedStartExonPhase   = 3;

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var encodedData = EncodedTranscriptData.GetEncodedTranscriptData(expectedBiotype, expectedCdsStartNotFound,
                expectedCdsEndNotFound, expectedSource, expectedCanonical, expectedSift, expectedPolyPhen,
                expectedMirnas, expectedRnaEdits, expectedSelenocysteines, expectedIntrons, expectedCdnaMaps,
                expectedTranslation, expectedStartExonPhase);
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            EncodedTranscriptData observedEncodedTranscriptData;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    encodedData.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    var info     = reader.ReadUInt16();
                    var contents = reader.ReadByte();
                    observedEncodedTranscriptData = new EncodedTranscriptData(info, contents);
                }
            }

            Assert.NotNull(observedEncodedTranscriptData);
            Assert.Equal(expectedBiotype,          observedEncodedTranscriptData.BioType);
            Assert.Equal(expectedSource, observedEncodedTranscriptData.TranscriptSource);
            Assert.Equal(expectedCanonical,        observedEncodedTranscriptData.IsCanonical);
            Assert.Equal(expectedSift,             observedEncodedTranscriptData.HasSift);
            Assert.Equal(expectedPolyPhen,         observedEncodedTranscriptData.HasPolyPhen);
            Assert.Equal(expectedMirnas,           observedEncodedTranscriptData.HasMirnas);
            Assert.Equal(expectedIntrons,          observedEncodedTranscriptData.HasIntrons);
            Assert.Equal(expectedCdnaMaps,         observedEncodedTranscriptData.HasCdnaMaps);
            Assert.Equal(expectedTranslation,      observedEncodedTranscriptData.HasTranslation);
            Assert.Equal(expectedStartExonPhase,   observedEncodedTranscriptData.StartExonPhase);
        }
    }
}
