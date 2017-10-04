using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Piano
{
    public class PianoAnnotationUtils
    {
        internal const int FlankingLength = 5000;
        private static readonly AminoAcids AminoAcidsProvider = new AminoAcids(false);
        private static readonly AminoAcids MitoAminoAcidsProvider = new AminoAcids(true);

        public static void GetAnnotatedTranscripts(IVariant variant, ITranscript[] transcriptCandidates,
            ISequence compressedSequence, IList<IAnnotatedTranscript> annotatedTranscripts)
        {
            foreach (var transcript in transcriptCandidates)
            {
                if (NeedAnnotate(variant, transcript, variant.Behavior, transcript.Gene))
                {
                    var annotatedTranscript = GetAnnotatedTranscript(variant, compressedSequence, transcript);
                    if (annotatedTranscript != null) annotatedTranscripts.Add(annotatedTranscript);
                }


            }

        }


        private static IAnnotatedTranscript GetAnnotatedTranscript(IVariant variant, ISequence compressedSequence, ITranscript transcript)
        {
            var acidsProvider = variant.Chromosome.UcscName == "chrM"
                        ? MitoAminoAcidsProvider
                        : AminoAcidsProvider;
            IAnnotatedTranscript annotatedTranscript =
                PianoTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, compressedSequence,acidsProvider);

            return annotatedTranscript;
        }

        internal static bool NeedAnnotate(IInterval variant, IInterval transcript,
            AnnotationBehavior behavior, IInterval gene)
        {

            if (transcript.Overlaps(variant) && !behavior.ReducedTranscriptAnnotation) return true;

            return false;
        }

       
    }
}