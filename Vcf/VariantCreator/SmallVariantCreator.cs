using Genome;
using VariantAnnotation.Interface.IO;
using Variants;

namespace Vcf.VariantCreator
{
    public static class SmallVariantCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele,
            bool isDecomposed, bool isRecomposed, string[] linkedVids, string vid, bool isRefMinor)
        {
            int end         = start + refAllele.Length - 1;
            var variantType = GetVariantType(refAllele, altAllele);

            var annotationBehavior = variantType == VariantType.non_informative_allele
                ? AnnotationBehavior.MinimalAnnotationBehavior
                : AnnotationBehavior.SmallVariantBehavior;

            return new Variant(chromosome, start, end, refAllele, altAllele, variantType, vid, isRefMinor, isDecomposed,
                isRecomposed, linkedVids, null, annotationBehavior);
        }

        public static VariantType GetVariantType(string refAllele, string altAllele)
        {
            if (VcfCommon.IsNonInformativeAltAllele(altAllele)) return VariantType.non_informative_allele;

            int referenceAlleleLen = refAllele.Length;
            int alternateAlleleLen = altAllele.Length;

            if (alternateAlleleLen != referenceAlleleLen)
            {
                if (alternateAlleleLen == 0 && referenceAlleleLen > 0) return VariantType.deletion;
                if (alternateAlleleLen > 0 && referenceAlleleLen == 0) return VariantType.insertion;

                return VariantType.indel;
            }

            var variantType = alternateAlleleLen == 1 ? VariantType.SNV : VariantType.MNV;

            return variantType;
        }
    }
}