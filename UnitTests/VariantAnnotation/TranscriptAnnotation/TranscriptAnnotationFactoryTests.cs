using System.Collections.Generic;
using Cache.Data;
using Genome;
using Intervals;
using Moq;
using UnitTests.MockedData;
using UnitTests.TestUtilities;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class TranscriptAnnotationFactoryTests
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
        public void GetAnnotatedTranscripts_ReturnEmptyList()
        {
            var variant = new Mock<IVariant>();

            var transcript1 = new Transcript(ChromosomeUtilities.Chr1, 108455, 118455, string.Empty, BioType.mRNA, true,
                Source.Ensembl, Genes.MED8, TranscriptRegions.NM_001025366_2, string.Empty, null);

            var transcript2 = new Transcript(ChromosomeUtilities.Chr1, 128460, 129489, string.Empty, BioType.mRNA, true,
                Source.Ensembl, Genes.MED8, TranscriptRegions.NM_001025366_2, string.Empty, null);

            List<Transcript> transcripts = new() {transcript1, transcript2};

            variant.SetupGet(x => x.Behavior).Returns(new AnnotationBehavior(true, false, false, true, false));
            variant.SetupGet(x => x.Start).Returns(123456);
            variant.SetupGet(x => x.End).Returns(123456);

            var compressedSequence = new Mock<ISequence>();

            var observedAnnotatedTranscripts =
                TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant.Object, transcripts,
                    compressedSequence.Object, null);
            Assert.Empty(observedAnnotatedTranscripts);
        }
    }
}
