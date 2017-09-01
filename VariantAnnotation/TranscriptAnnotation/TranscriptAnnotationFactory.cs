using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class TranscriptAnnotationFactory
    {
        internal const int FlankingLength = 5000;
        internal static AminoAcids AminoAcidsProvider = new AminoAcids(false);
        internal static AminoAcids MitoAminoAcidsProvider = new AminoAcids(true);

        public static void GetAnnotatedTranscripts(IVariant variant, ITranscript[] transcriptCandidates,
            ISequence compressedSequence, IList<IAnnotatedTranscript> annotatedTranscripts,
            ISet<string> overlappingGenes, IList<IOverlappingTranscript> overlappingTranscripts,
            IPredictionCache siftCache, IPredictionCache polyphenCache,
            IEnumerable<ITranscript> geneFusionCandidates = null)
        {
            foreach (var transcript in transcriptCandidates)
            {
                var annotationStatus = DecideAnnotationStatus(variant, transcript, variant.Behavior, transcript.Gene);

                if (annotationStatus != Status.NoAnnotation && variant.Behavior.ReportOverlappingGenes)
                    overlappingGenes.Add(transcript.Gene.Symbol);

                if (variant.Behavior.NeedVerboseTranscripts)
                    AddOverlappingTranscript(annotationStatus, transcript,variant,overlappingTranscripts );

                var annotatedTranscript = GetAnnotatedTranscript(variant, compressedSequence, transcript, annotationStatus, siftCache,polyphenCache,geneFusionCandidates);

                if (annotatedTranscript != null) annotatedTranscripts.Add(annotatedTranscript);
            }

        }

        private static void AddOverlappingTranscript(Status annotationStatus, ITranscript transcript,IInterval variant, IList<IOverlappingTranscript> overlappingTranscripts)
        {
            if ( annotationStatus == Status.SvCompleteOverlapAnnotation)
                overlappingTranscripts.Add(new OverlappingTranscript(transcript.Id,transcript.Gene.Symbol,transcript.Version,transcript.IsCanonical,false));
            if (annotationStatus == Status.ReducedAnnotation)
            {
                var partialOverlap = !variant.Contains(transcript);
                overlappingTranscripts.Add(new OverlappingTranscript(transcript.Id, transcript.Gene.Symbol, transcript.Version, transcript.IsCanonical, partialOverlap));
            }
               
        }

        private static IAnnotatedTranscript GetAnnotatedTranscript(IVariant variant, ISequence compressedSequence, ITranscript transcript, Status annotationStatus,
            IPredictionCache siftCache, IPredictionCache polyphenCache, IEnumerable<ITranscript> geneFusionCandidates)
        {
            IAnnotatedTranscript annotatedTranscript = null;
            switch (annotationStatus)
            {
                case Status.FlankingAnnotation:
                    annotatedTranscript =
                        FlankingTranscriptAnnotator.GetAnnotatedTranscript(variant.End, transcript);
                    break;
                case Status.ReducedAnnotation:
                    annotatedTranscript = ReducedTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant,geneFusionCandidates);
                    break;
                case Status.FullAnnotation:
                    var aminoAcidsPrider = variant.Chromosome.UcscName == "chrM"
                        ? MitoAminoAcidsProvider
                        : AminoAcidsProvider;
                    annotatedTranscript =
                        FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, compressedSequence,siftCache,polyphenCache,aminoAcidsPrider);
                    break;
                default:
                    break;
            }

            return annotatedTranscript;
        }

        internal static Status DecideAnnotationStatus(IInterval variant, IInterval transcript,
            AnnotationBehavior behavior,IInterval gene)
        {

            if (variant.Contains(gene) && behavior.ReducedTranscriptAnnotation)  return Status.SvCompleteOverlapAnnotation;
            if (variant.Contains(gene) && !behavior.ReducedTranscriptAnnotation) return Status.FullAnnotation;
            if (!variant.Contains(gene)&& variant.Overlaps(transcript) && behavior.ReducedTranscriptAnnotation)  return Status.ReducedAnnotation;
            if (!variant.Contains(gene) && variant.Overlaps(transcript) && !behavior.ReducedTranscriptAnnotation) return Status.FullAnnotation;
            if (!variant.Overlaps(transcript) && variant.Overlaps(transcript, FlankingLength) && behavior.NeedFlankingTranscript)
                return Status.FlankingAnnotation;

            return Status.NoAnnotation;
        }

        public enum Status
        {
            NoAnnotation,
            SvCompleteOverlapAnnotation,
            FlankingAnnotation,
            ReducedAnnotation,
            FullAnnotation
        }
    }
}