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
            var expectedBiotype          = BioType.non_stop_decay;
            byte expectedVersion         = 31;
            var expectedTranscriptSource = Source.BothRefSeqAndEnsembl;
            bool expectedCanonical       = true;
            bool expectedSift            = true;
            bool expectedPolyPhen        = true;
            bool expectedMirnas          = true;
            bool expectedIntrons         = true;
            bool expectedCdnaMaps        = true;
            bool expectedTranslation     = true;
            byte expectedStartExonPhase  = 3;

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var encodedData = new EncodedTranscriptData(expectedBiotype, expectedVersion, expectedTranscriptSource,                
                expectedCanonical, expectedSift, expectedPolyPhen, expectedMirnas, expectedIntrons,                
                expectedCdnaMaps, expectedTranslation, expectedStartExonPhase);
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
            Assert.Equal(expectedVersion,          observedEncodedTranscriptData.Version);
            Assert.Equal(expectedTranscriptSource, observedEncodedTranscriptData.TranscriptSource);
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
