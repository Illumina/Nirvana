using CommonUtilities;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Sequence;
using VariantAnnotation.TranscriptAnnotation;
using Vcf;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedTranscriptTests
    {
        [Fact]
        public void SerializeJson_NominalUsage()
        {
            var chromosome  = new Chromosome("chr1", "1", 0);
            var variant     = new Variant(chromosome, 1263141, 1263143, "TAG", "", VariantType.deletion, "1:1263141:1263143", false, false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
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