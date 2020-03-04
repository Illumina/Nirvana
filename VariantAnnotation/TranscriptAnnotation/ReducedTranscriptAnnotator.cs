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
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant variant)
        {
            bool completeOverlap = variant.Contains(transcript);
            var  mappedPosition  = completeOverlap ? null : GetMappedPosition(transcript.TranscriptRegions, variant);

            List<ConsequenceTag> consequences = GetConsequences(transcript, variant);

            return new AnnotatedTranscript(transcript, null, null, null, null, mappedPosition, null, null, null, null,
                consequences, completeOverlap);
        }

        private static IMappedPosition GetMappedPosition(ITranscriptRegion[] regions, IInterval variant)
        {
            (int startIndex, _) = MappedPositionUtilities.FindRegion(regions, variant.Start);
            (int endIndex, _)   = MappedPositionUtilities.FindRegion(regions, variant.End);

            (int exonStart, int exonEnd, int intronStart, int intronEnd) = regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(-1, -1, -1, -1, -1, -1, exonStart, exonEnd, intronStart, intronEnd, startIndex,
                endIndex);
        }

        private static List<ConsequenceTag> GetConsequences(IInterval transcript, IVariant variant)
        {
            var featureEffect = new FeatureVariantEffects(transcript, variant.Type, variant, true);
            var consequence   = new Consequences(null, featureEffect);
            consequence.DetermineStructuralVariantEffect(variant);
            return consequence.GetConsequences();
        }
    }
}