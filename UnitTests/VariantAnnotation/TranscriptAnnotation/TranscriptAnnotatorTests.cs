using Genome;
using Intervals;
using Moq;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class TranscriptAnnotatorTests
    {
        [Fact]
        public void DecideAnnotationStatus_NoOverlap_ReturnNoAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 101),
                new Interval(5102, 6100), new AnnotationBehavior(true, false, false, true, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.NoAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Flanking_ReturnFlankingAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 100),
                new Interval(102, 305), new AnnotationBehavior(false, true, false, true, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.FlankingAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Reduced_TranscriptCompleteOverlap_ReturnSvCompleteOverlapAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), new AnnotationBehavior(false, true, true, false, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.CompleteOverlapAnnotation, observedStatus);
        }

        // the only thing that matters now is overlap w.r.t. the transcript (not gene)
        [Fact]
        public void DecideAnnotationStatus_Reduced_TranscriptCompleteOverlap_GenePartialOverlap_ReturnSvCompleteOverlapAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), new AnnotationBehavior(false, true, true, false, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.CompleteOverlapAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Reduced_TranscriptPartialOverlap_ReturnReducedAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 200),
                new Interval(102, 305), new AnnotationBehavior(false, true, true, false, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.ReducedAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Full_PartialOverlap_ReturnFullAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 105),
                new Interval(102, 305), new AnnotationBehavior(false, true, false, false, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.FullAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Full_CompleteOverlap_ReturnFullAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), new AnnotationBehavior(false, true, false, false, false));

            Assert.Equal(TranscriptAnnotationFactory.Status.FullAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_ROH_Return_RohAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), AnnotationBehavior.RohBehavior);

            Assert.Equal(TranscriptAnnotationFactory.Status.RohAnnotation, observedStatus);
        }

        [Fact]
        public void GetAnnotatedTranscripts_ReturnEmptyList()
        {
            var variant     = new Mock<IVariant>();
            var transcript1 = new Mock<ITranscript>();
            var transcript2 = new Mock<ITranscript>();

            var transcripts = new[] { transcript1.Object, transcript2.Object };

            variant.SetupGet(x => x.Behavior).Returns(new AnnotationBehavior(true, false, false, true, false));
            variant.SetupGet(x => x.Start).Returns(123456);
            variant.SetupGet(x => x.End).Returns(123456);

            transcript1.SetupGet(x => x.Id).Returns(CompactId.Convert("NR_046018.2"));
            transcript1.SetupGet(x => x.Start).Returns(108455);
            transcript1.SetupGet(x => x.End).Returns(118455);
            transcript1.SetupGet(x => x.Gene.Start).Returns(108455);
            transcript1.SetupGet(x => x.Gene.End).Returns(118455);

            transcript2.SetupGet(x => x.Id).Returns(CompactId.Convert("NR_106918.1"));
            transcript2.SetupGet(x => x.Start).Returns(128460);
            transcript2.SetupGet(x => x.End).Returns(129489);
            transcript2.SetupGet(x => x.Gene.Start).Returns(128460);
            transcript2.SetupGet(x => x.Gene.End).Returns(129489);

            var compressedSequence = new Mock<ISequence>();

            var observedAnnotatedTranscripts =
                TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant.Object, transcripts,
                    compressedSequence.Object, null, null);

            Assert.Empty(observedAnnotatedTranscripts);
        }

        [Fact]
        public void GetAnnotatedTranscripts_RohAnnotation_ReturnsCanonicalOnly()
        {
            var variant = new Mock<IVariant>();
            var transcript1 = new Mock<ITranscript>();
            var transcript2 = new Mock<ITranscript>();

            var transcripts = new[] { transcript1.Object, transcript2.Object };

            variant.SetupGet(x => x.Behavior).Returns(AnnotationBehavior.RohBehavior);
            variant.SetupGet(x => x.Start).Returns(10000);
            variant.SetupGet(x => x.End).Returns(20000);

            transcript1.SetupGet(x => x.Id).Returns(CompactId.Convert("NM_123.1"));
            transcript1.SetupGet(x => x.Start).Returns(11000);
            transcript1.SetupGet(x => x.End).Returns(15000);
            transcript1.SetupGet(x => x.IsCanonical).Returns(true);

            transcript2.SetupGet(x => x.Id).Returns(CompactId.Convert("NM_456.2"));
            transcript2.SetupGet(x => x.Start).Returns(11000);
            transcript2.SetupGet(x => x.End).Returns(15000);
            transcript2.SetupGet(x => x.IsCanonical).Returns(false);

            var observedAnnotatedTranscripts =
                TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant.Object, transcripts, null, null, null);

            Assert.Single(observedAnnotatedTranscripts);
            Assert.Equal("NM_123", observedAnnotatedTranscripts[0].Transcript.Id.WithVersion);
        }
    }
}
