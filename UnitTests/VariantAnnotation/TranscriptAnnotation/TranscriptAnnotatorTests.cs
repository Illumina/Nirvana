using System.Collections.Generic;
using Moq;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.TranscriptAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class TranscriptAnnotatorTests
    {
        [Fact]
        public void NonOverlap_transcript_get_no_annotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 101),
                new Interval(5102, 6100), new AnnotationBehavior(true, false, false, true, false, false), new Interval(5102, 6100));

            Assert.Equal(TranscriptAnnotationFactory.Status.NoAnnotation, observedStatus);
        }

        [Fact]
        public void When_reducedTranscriptAnnotation_and_gene_is_completely_overlapped_with_variant_get_SvCompleteOverlapAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), new AnnotationBehavior(false, true, true, false, false, false), new Interval(102, 305));

            Assert.Equal(TranscriptAnnotationFactory.Status.SvCompleteOverlapAnnotation, observedStatus);
        }

        [Fact]
        public void When_reducedTranscriptAnnotation_and_transcript_completely_overlapped_variant_but_gene_partial_overlap_get_reducedAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), new AnnotationBehavior(false, true, true, false, false, false), new Interval(102, 503));

            Assert.Equal(TranscriptAnnotationFactory.Status.ReducedAnnotation, observedStatus);
        }
        [Fact]
        public void When_not_reducedTranscriptAnnotation_completely_overlapped_variant_get_full_annotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), new AnnotationBehavior(false, true, false, false, false, false), new Interval(102, 305));

            Assert.Equal(TranscriptAnnotationFactory.Status.FullAnnotation, observedStatus);
        }

        [Fact]
        public void When_reducedTranscriptAnnotation_partially_overlapped_variant_and_gene_partial_overlapped_get_reduced_annotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 200),
                new Interval(102, 305), new AnnotationBehavior(false, true, true, false, false, false), new Interval(102, 305));

            Assert.Equal(TranscriptAnnotationFactory.Status.ReducedAnnotation, observedStatus);
        }

        [Fact]
        public void When_not_reducedTranscriptAnnotation_partially_overlapped_variant_get_full_annotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 105),
                new Interval(102, 305), new AnnotationBehavior(false, true, false, false, false, false), new Interval(102, 305));

            Assert.Equal(TranscriptAnnotationFactory.Status.FullAnnotation, observedStatus);
        }

        [Fact]
        public void When_needFlankingTranscript_flankingTranscript_get_flankingAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 100),
                new Interval(102, 305), new AnnotationBehavior(false, true, false, true, false, false), new Interval(102, 305));

            Assert.Equal(TranscriptAnnotationFactory.Status.FlankingAnnotation, observedStatus);
        }

        [Fact]
        public void Annotate_return_null_when_no_flanking_over_transcript()
        {
            var variant = new Mock<IVariant>();
            var transcript1 = new Mock<ITranscript>();
            var transcript2 = new Mock<ITranscript>();

            var transcripts = new[] { transcript1.Object, transcript2.Object };

            variant.SetupGet(x => x.Behavior).Returns(new AnnotationBehavior(true, false, false, true, false, false));
            variant.SetupGet(x => x.Start).Returns(123456);
            variant.SetupGet(x => x.End).Returns(123456);

            transcript1.SetupGet(x => x.Start).Returns(108455);
            transcript1.SetupGet(x => x.End).Returns(118455);

            transcript1.SetupGet(x => x.Gene.Start).Returns(108455);
            transcript1.SetupGet(x => x.Gene.End).Returns(118455);

            transcript2.SetupGet(x => x.Start).Returns(128460);
            transcript2.SetupGet(x => x.End).Returns(129489);

            transcript2.SetupGet(x => x.Gene.Start).Returns(128460);
            transcript2.SetupGet(x => x.Gene.End).Returns(129489);

            var compressedSequence = new Mock<ISequence>();
            var observedAnnotatedTranscripts = new List<IAnnotatedTranscript>();

            TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant.Object, transcripts, compressedSequence.Object, observedAnnotatedTranscripts, null, null, null, null);

            Assert.Empty(observedAnnotatedTranscripts);
        }
    }
}
