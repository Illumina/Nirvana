using System.Collections.Generic;
using Intervals;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class ReducedTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant variant, ITranscript[] geneFusionCandidates)
        {
            var annotation   = AnnotateTranscript(transcript, variant, geneFusionCandidates);
            var consequences = GetConsequences(transcript, variant, annotation.GeneFusion != null);

            return new AnnotatedTranscript(transcript, null, null, null, null, annotation.Position, null, null, null,
                null, consequences, annotation.GeneFusion, false);
        }

        public static IAnnotatedTranscript GetCompleteOverlapTranscript(ITranscript transcript) =>
            new AnnotatedTranscript(transcript, null, null, null, null, null, null, null, null, null, null, null, true);

        private static (IMappedPosition Position, IGeneFusionAnnotation GeneFusion)
            AnnotateTranscript(ITranscript transcript, IVariant variant, ITranscript[] geneFusionCandidates)
        {
            var position = GetMappedPosition(transcript.TranscriptRegions, variant);
            var geneFusionAnnotation = GeneFusionUtilities.GetGeneFusionAnnotation(variant.BreakEnds, transcript, geneFusionCandidates);
            return (position, geneFusionAnnotation);
        }

        private static IMappedPosition GetMappedPosition(ITranscriptRegion[] regions, IInterval variant)
        {
            var (startIndex, _) = MappedPositionUtilities.FindRegion(regions, variant.Start);
            var (endIndex, _)   = MappedPositionUtilities.FindRegion(regions, variant.End);

            var (exonStart, exonEnd, intronStart, intronEnd) = regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(-1, -1, -1, -1, -1, -1, exonStart, exonEnd, intronStart, intronEnd, startIndex,
                endIndex);
        }

        private static IEnumerable<ConsequenceTag> GetConsequences(IInterval transcript, ISimpleVariant variant,
            bool hasGeneFusionAnnotation)
        {
            var featureEffect = new FeatureVariantEffects(transcript, variant.Type, variant, true);
            var consequence   = new Consequences(null, featureEffect);
            consequence.DetermineStructuralVariantEffect(variant.Type, hasGeneFusionAnnotation);
            return consequence.GetConsequences();
        }
    }
}