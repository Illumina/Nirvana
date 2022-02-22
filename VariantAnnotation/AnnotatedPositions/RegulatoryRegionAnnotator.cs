using Cache.Data;
using VariantAnnotation.AnnotatedPositions.Consequence;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class RegulatoryRegionAnnotator
    {
        public static AnnotatedRegulatoryRegion Annotate(IVariant variant, RegulatoryRegion regulatoryRegion)
        {
            var featureEffect = new FeatureVariantEffects(regulatoryRegion, variant.Type, variant,
                variant.Behavior.StructuralVariantConsequence);

            var consequence = new Consequences(null, featureEffect);
            consequence.DetermineRegulatoryVariantEffects();
            return new AnnotatedRegulatoryRegion(regulatoryRegion, consequence.GetConsequences());
        }
    }
}