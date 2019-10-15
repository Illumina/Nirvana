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

            return new RepeatExpansion(chromosome, start, end, refAllele, altAllele, vid, repeatCount, refRepeatCount);
        }
    }
}