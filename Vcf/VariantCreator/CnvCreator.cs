using Genome;
using VariantAnnotation.Pools;
using Variants;

namespace Vcf.VariantCreator
{
    public static class CnvCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, int end, string refAllele, string altAllele, string vid)
        {
            var variantType = GetVariantType(altAllele);
            return VariantPool.Get(chromosome, start + 1, end, refAllele, altAllele, variantType, vid, false, false, false,
                null, AnnotationBehavior.StructuralVariants, true);
        }

        // For old style allelic CNV calls (e.g. <CN1>, <CN4>, etc.),
        // do not try to determine the overall copy number gain or loss
        // - for allele-specific you'll probably introduce inconsistency
        // - for normal <CNV>, you'll probably get type wrong for MT, sex chromosomes, etc.
        private static VariantType GetVariantType(string altAllele)
        {
            if (altAllele == "<DEL>") return VariantType.copy_number_loss;
            return altAllele == "<DUP>" ? VariantType.copy_number_gain : VariantType.copy_number_variation;
        }
    }
}