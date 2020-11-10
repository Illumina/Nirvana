using Genome;
using Variants;

namespace Vcf.VariantCreator
{
    public static class StructuralVariantCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, int end, string refAllele, string altAllele, string svType, string vid)
        {
            VariantType variantType = GetVariantType(altAllele, svType);
            AnnotationBehavior behavior = variantType == VariantType.translocation_breakend
                ? AnnotationBehavior.BreakendVariants
                : AnnotationBehavior.StructuralVariants;

            if (variantType != VariantType.translocation_breakend) start++;

            return new Variant(chromosome, start, end, refAllele, altAllele, variantType, vid, false, false, false,
                null, behavior, true);
        }
                
        public static VariantType GetVariantType(string altAllele, string svType)
        {
            switch (svType)
            {
                case "DEL":
                    return VariantType.deletion;
                case "INS":
                    return VariantType.insertion;
                case "DUP":
                    return altAllele == "<DUP:TANDEM>" ? VariantType.tandem_duplication : VariantType.duplication;
                case "INV":
                    return VariantType.inversion;
                case "TDUP":
                    return VariantType.tandem_duplication;
                case "BND":
                    return VariantType.translocation_breakend;
                case "CNV":
                    return VariantType.copy_number_variation;
                case "STR":
                    return VariantType.short_tandem_repeat_variation;
                case "ALU":
                    return VariantType.mobile_element_insertion;
                case "LINE1":
                    return VariantType.mobile_element_insertion;
                case "LOH":
                    return VariantType.copy_number_variation;
                case "SVA":
                    return VariantType.mobile_element_insertion;
                default:
                    return VariantType.unknown;
            }
        }
    }
}