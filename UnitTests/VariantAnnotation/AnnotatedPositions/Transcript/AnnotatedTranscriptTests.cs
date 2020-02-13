using OptimizedCore;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedTranscriptTests
    {
        [Fact]
        public void SerializeJson_NominalUsage()
        {
            var variant     = new Variant(ChromosomeUtilities.Chr1, 1263141, 1263143, "TAG", "", VariantType.deletion, "1:1263141:1263143", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(HgvsProteinNomenclatureTests.Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = HgvsProteinNomenclatureTests.GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));
            var sb = StringBuilderCache.Acquire();
            annotatedTranscript.SerializeJson(sb);
            var jsonString = StringBuilderCache.GetStringAndRelease(sb);

            Assert.Contains("ENST00000343938.4:p.(Ter215GlyextTer43)", jsonString);
        }
    }
}