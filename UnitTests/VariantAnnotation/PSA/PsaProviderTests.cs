using Cache.Data;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Providers;
using VariantAnnotation.PSA;
using Xunit;

namespace UnitTests.VariantAnnotation.PSA
{
    public sealed class PsaProviderTests
    {
        [Fact]
        public void Annotate()
        {
            PsaReader[] psaReaders  = {PsaTestUtilities.GetSiftPsaReader(), PsaTestUtilities.GetPolyPhenPsaReader()};
            var         psaProvider = new PsaProvider(psaReaders);

            var sift     = new PredictionScore("unknown", 0.9987);
            var polyPhen = new PredictionScore("unknown", 0.9987);

            var gene = new Gene("GENE2", "GENE2", false, null);
            var transcript = new Transcript(ChromosomeUtilities.Chr1, 100, 200, "ENST00000641515", BioType.mRNA, false,
                Source.Ensembl, gene, null, string.Empty, null);

            var annotatedTranscript =
                new AnnotatedTranscript(transcript, null, null, null, null, null, null, null, null, null, false);

            annotatedTranscript.AddSift(sift);
            psaProvider.Annotate(annotatedTranscript, 12, 'K');

            Assert.NotNull(annotatedTranscript.Sift);
            Assert.NotNull(annotatedTranscript.PolyPhen);
        }
    }
}