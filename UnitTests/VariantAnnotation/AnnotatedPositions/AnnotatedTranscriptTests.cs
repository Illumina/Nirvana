using OptimizedCore;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedTranscriptTests
    {
        [Fact]
        public void SerializeJson_ExpectedResults()
        {
            var variant     = new Variant(ChromosomeUtilities.Chr1, 1263141, 1263143, "TAG", "", VariantType.deletion, "1:1263141:1263143", false, false, false, null, null, new AnnotationBehavior(false, false, false, false, false));
            var refSequence = new SimpleSequence(HgvsProteinNomenclatureTests.Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = HgvsProteinNomenclatureTests.GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, AminoAcidCommon.StandardAminoAcids);
            var sb = StringBuilderCache.Acquire();
            annotatedTranscript.SerializeJson(sb);
            string jsonString = StringBuilderCache.GetStringAndRelease(sb);

            Assert.Contains("ENSP00000343890.4:p.(Ter215GlyextTer43)", jsonString);
        }
    }
}