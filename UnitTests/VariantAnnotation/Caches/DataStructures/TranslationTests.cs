using System.IO;
using System.Text;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class TranslationTests
    {
        [Fact]
        public void Translation_EndToEnd()
        {
            ICodingRegion expectedCodingRegion = new CodingRegion(100, 200, 300, 400, 101);
            const string expectedProteinId         = "ENSP00000446475.7";
            const string expectedPeptideSeq        = "VEIDSD";

            string[] peptideSeqs = { expectedPeptideSeq };

            ITranslation expectedTranslation =
                new Translation(expectedCodingRegion, CompactId.Convert(expectedProteinId, 7),
                    expectedPeptideSeq);

            ITranslation observedTranslation;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    expectedTranslation.Write(writer, 0);
                }

                ms.Position = 0;

                using (var reader = new BufferedBinaryReader(ms))
                {
                    observedTranslation = Translation.Read(reader, peptideSeqs);
                }
            }

            Assert.NotNull(observedTranslation);
            Assert.Equal(expectedCodingRegion.CdnaStart, observedTranslation.CodingRegion.CdnaStart);
            Assert.Equal(expectedProteinId,              observedTranslation.ProteinId.WithVersion);
            Assert.Equal(expectedPeptideSeq,             observedTranslation.PeptideSeq);
        }
    }
}
