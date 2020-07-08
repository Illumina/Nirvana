using Intervals;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class RegulatoryRegionAnnotator
    {
        public static IAnnotatedRegulatoryRegion Annotate(IVariant variant, IRegulatoryRegion regulatoryRegion)
        {
            OverlapType overlapType = Intervals.Utilities.GetOverlapType(regulatoryRegion.Start, regulatoryRegion.End,
                variant.Start, variant.End);
            EndpointOverlapType endpointOverlapType =
                Intervals.Utilities.GetEndpointOverlapType(regulatoryRegion.Start, regulatoryRegion.End, variant.Start, variant.End);
            var featureEffect = new FeatureVariantEffects(overlapType, endpointOverlapType, false, variant.Type, variant.IsStructuralVariant);
            
            var consequence = new Consequences(VariantType.unknown, null, featureEffect);
            consequence.DetermineRegulatoryVariantEffects();
            return new AnnotatedRegulatoryRegion(regulatoryRegion, consequence.GetConsequences());
        }
    }
}