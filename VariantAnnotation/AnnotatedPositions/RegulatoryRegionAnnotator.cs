using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class RegulatoryRegionAnnotator
    {
        public static IAnnotatedRegulatoryRegion Annotate(IVariant variant, IRegulatoryRegion regulatoryRegion)
        {
            var featureEffect = new FeatureVariantEffects(regulatoryRegion, variant.Type, variant.Start, variant.End,
                variant.Behavior.StructuralVariantConsequence);

            var consequence = new Consequences(null, featureEffect);
            consequence.DetermineRegulatoryVariantEffects();
            return new AnnotatedRegulatoryRegion(regulatoryRegion, consequence.GetConsequences());
        }
    }
}