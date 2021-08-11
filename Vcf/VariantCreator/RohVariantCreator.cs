using Genome;
using VariantAnnotation.Pools;
using Variants;

namespace Vcf.VariantCreator
{
    public static class RohVariantCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            string variantId) => VariantPool.Get(chromosome, start + 1, end, refAllele, altAllele,
            VariantType.run_of_homozygosity, variantId, false, false, false, null,
            AnnotationBehavior.RunsOfHomozygosity, true);

    }
}