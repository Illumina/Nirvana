using System.Collections.Generic;
using Cache.Data;
using Intervals;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class ReducedTranscriptAnnotator
    {
        public static AnnotatedTranscript GetAnnotatedTranscript(Transcript transcript, IVariant variant, HashSet<Transcript> geneFusionCandidates)
        {
            var annotation   = AnnotateTranscript(transcript, variant, geneFusionCandidates);
            var consequences = GetConsequences(transcript, variant, annotation.GeneFusion != null);

            return new AnnotatedTranscript(transcript, null, null, null, null, annotation.Position, null, null,
                consequences, annotation.GeneFusion, false);
        }

        public static AnnotatedTranscript GetCompleteOverlapTranscript(Transcript transcript) =>
            new AnnotatedTranscript(transcript, null, null, null, null, null, null, null, null, null, true);

        private static (IMappedPosition Position, IGeneFusionAnnotation GeneFusion)
            AnnotateTranscript(Transcript transcript, IVariant variant, HashSet<Transcript> geneFusionCandidates)
        {
            var position = GetMappedPosition(transcript.TranscriptRegions, variant);
            var geneFusionAnnotation = GeneFusionUtilities.GetGeneFusionAnnotation(variant.BreakEnds, transcript, geneFusionCandidates);
            return (position, geneFusionAnnotation);
        }

        private static IMappedPosition GetMappedPosition(TranscriptRegion[] regions, IInterval variant)
        {
            (int startIndex, _) = MappedPositionUtilities.FindRegion(regions, variant.Start);
            (int endIndex, _)   = MappedPositionUtilities.FindRegion(regions, variant.End);

            (int exonStart, int exonEnd, int intronStart, int intronEnd) =
                regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(-1, -1, -1, -1, -1, -1, -1, -1, exonStart, exonEnd, intronStart, intronEnd,
                startIndex, endIndex);
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