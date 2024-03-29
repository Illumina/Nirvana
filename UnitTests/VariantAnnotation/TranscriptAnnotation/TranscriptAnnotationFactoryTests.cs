﻿using System.Collections.Generic;
using Genome;
using Intervals;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
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
                new Interval(5102, 6100), AnnotationBehavior.SmallVariants, Chromosome.ShortFlankingLength);

            Assert.Equal(TranscriptAnnotationFactory.Status.NoAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Flanking_ReturnFlankingAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 100),
                new Interval(102, 305), AnnotationBehavior.SmallVariants, Chromosome.ShortFlankingLength);

            Assert.Equal(TranscriptAnnotationFactory.Status.FlankingAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Reduced_TranscriptPartialOverlap_ReturnReducedAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 200),
                new Interval(102, 305), AnnotationBehavior.StructuralVariants, Chromosome.ShortFlankingLength);

            Assert.Equal(TranscriptAnnotationFactory.Status.ReducedAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Full_PartialOverlap_ReturnFullAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 105),
                new Interval(102, 305), AnnotationBehavior.SmallVariants, Chromosome.ShortFlankingLength);

            Assert.Equal(TranscriptAnnotationFactory.Status.FullAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_Full_CompleteOverlap_ReturnFullAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), AnnotationBehavior.SmallVariants, Chromosome.ShortFlankingLength);

            Assert.Equal(TranscriptAnnotationFactory.Status.FullAnnotation, observedStatus);
        }

        [Fact]
        public void DecideAnnotationStatus_ROH_Return_RohAnnotation()
        {
            var observedStatus = TranscriptAnnotationFactory.DecideAnnotationStatus(new Interval(100, 500),
                new Interval(102, 305), AnnotationBehavior.RunsOfHomozygosity, Chromosome.ShortFlankingLength);

            Assert.Equal(TranscriptAnnotationFactory.Status.RohAnnotation, observedStatus);
        }

        [Fact]
        public void GetAnnotatedTranscripts_ReturnEmptyList()
        {
            var variant     = new Mock<IVariant>();
            var transcript1 = new Mock<ITranscript>();
            var transcript2 = new Mock<ITranscript>();

            ITranscript[] transcripts = { transcript1.Object, transcript2.Object };

            var chromosome = ChromosomeUtilities.Chr1;
            variant.SetupGet(x => x.Behavior).Returns(AnnotationBehavior.SmallVariants);
            variant.SetupGet(x => x.Chromosome).Returns(chromosome);
            //variant.SetupGet(x => x.Chromosome.FlankingLength).Returns(Chromosome.ShortFlankingLength);
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

            IList<IAnnotatedTranscript> observedAnnotatedTranscripts =
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

            ITranscript[] transcripts = { transcript1.Object, transcript2.Object };

            variant.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            variant.SetupGet(x => x.Behavior).Returns(AnnotationBehavior.RunsOfHomozygosity);
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

            IList<IAnnotatedTranscript> observedAnnotatedTranscripts =
                TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant.Object, transcripts, null, null, null);

            Assert.Single(observedAnnotatedTranscripts);
            Assert.Equal("NM_123", observedAnnotatedTranscripts[0].Transcript.Id.WithVersion);
        }
    }
}
