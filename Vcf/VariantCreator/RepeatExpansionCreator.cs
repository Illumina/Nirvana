using Genome;
using OptimizedCore;
using Variants;

namespace Vcf.VariantCreator
{
    public static class RepeatExpansionCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, int end, string refAllele, string altAllele, int? refRepeatCount, string vid)
        {
            if (refRepeatCount == null || refRepeatCount == 0) return null;

            (int repeatCount, bool foundError) = altAllele.Trim('<', '>').Substring(3).OptimizedParseInt32();
            if (foundError) return null;

            start++;
            var variantType = GetRepeatExpansionType(refRepeatCount, repeatCount);

            return new Variant(chromosome, start, end, refAllele, altAllele, variantType, vid, false, false, false,
                null, null, AnnotationBehavior.RepeatExpansionBehavior);
        }

        private static VariantType GetRepeatExpansionType(int? refRepeatCount, int repeatCount)
        {
            if (refRepeatCount == null || refRepeatCount == repeatCount) return VariantType.short_tandem_repeat_variation;
            return repeatCount > refRepeatCount ? VariantType.short_tandem_repeat_expansion : VariantType.short_tandem_repeat_contraction;
        }
    }
}