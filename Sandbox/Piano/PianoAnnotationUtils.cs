using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Piano
{
    public static class PianoAnnotationUtils
    {
        private static readonly AminoAcids AminoAcidsProvider = new AminoAcids(false);
        private static readonly AminoAcids MitoAminoAcidsProvider = new AminoAcids(true);

        public static void GetAnnotatedTranscripts(IVariant variant, ITranscript[] transcriptCandidates,
            ISequence compressedSequence, IList<IAnnotatedTranscript> annotatedTranscripts)
        {
            foreach (var transcript in transcriptCandidates)
            {
                if (transcript.Overlaps(variant) && !variant.Behavior.ReducedTranscriptAnnotation)
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
            var annotatedTranscript =
                PianoTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, compressedSequence,acidsProvider);

            return annotatedTranscript;
        }


       
    }
}