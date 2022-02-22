using System.Collections.Generic;
using Cache.Data;
using Genome;
using Intervals;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.Interface.Intervals;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class TranscriptAnnotationFactory
    {
        public static List<AnnotatedTranscript> GetAnnotatedTranscripts(IVariant variant, List<Transcript> transcripts,
            ISequence compressedSequence, HashSet<Transcript> geneFusionCandidates)
        {
            var annotatedTranscripts = new List<AnnotatedTranscript>();

            foreach (var transcript in transcripts)
            {
                var annotationStatus = DecideAnnotationStatus(variant, transcript, variant.Behavior);

                var annotatedTranscript = GetAnnotatedTranscript(variant, compressedSequence, transcript,
                    annotationStatus, geneFusionCandidates);

                if (annotatedTranscript != null) annotatedTranscripts.Add(annotatedTranscript);
            }

            return annotatedTranscripts;
        }

        private static AnnotatedTranscript GetAnnotatedTranscript(IVariant variant, ISequence compressedSequence,
            Transcript transcript, Status annotationStatus, HashSet<Transcript> geneFusionCandidates)
        {
            AnnotatedTranscript annotatedTranscript = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (annotationStatus)
            {
                case Status.FlankingAnnotation:
                    annotatedTranscript =
                        FlankingTranscriptAnnotator.GetAnnotatedTranscript(variant.End, transcript);
                    break;
                case Status.ReducedAnnotation:
                    annotatedTranscript =
                        ReducedTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, geneFusionCandidates);
                    break;
                case Status.CompleteOverlapAnnotation:
                    annotatedTranscript = ReducedTranscriptAnnotator.GetCompleteOverlapTranscript(transcript);
                    break;
                case Status.FullAnnotation:
                    var aminoAcids = variant.Chromosome.UcscName == "chrM"
                        ? AminoAcidCommon.MitochondrialAminoAcids
                        : AminoAcidCommon.StandardAminoAcids;
                    annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant,
                        compressedSequence, aminoAcids);
                    break;
            }

            return annotatedTranscript;
        }

        internal static Status DecideAnnotationStatus(IInterval variant, IInterval transcript,
            AnnotationBehavior behavior)
        {
            var overlapsTranscript = variant.Overlaps(transcript);

            if (!behavior.ReducedTranscriptAnnotation)
            {
                // handle small variants
                if (overlapsTranscript) return Status.FullAnnotation;
                if (behavior.NeedFlankingTranscript && variant.Overlaps(transcript, OverlapBehavior.FlankingLength))
                    return Status.FlankingAnnotation;
            }
            else
            {
                // handle large variants
                if (variant.Contains(transcript)) return Status.CompleteOverlapAnnotation;
                if (overlapsTranscript) return Status.ReducedAnnotation;
            }

            return Status.NoAnnotation;
        }

        public enum Status
        {
            NoAnnotation,
            CompleteOverlapAnnotation,
            FlankingAnnotation,
            ReducedAnnotation,
            FullAnnotation
        }
    }
}