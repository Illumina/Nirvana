using System;
using Genome;
using Variants;

namespace Vcf.VariantCreator
{
    public static class ReferenceVariantCreator
    {
        private static string GetVid(string ensemblName, int start, int end, string refAllele, VariantType variantType)
        {
            string referenceName = ensemblName;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (variantType)
            {
                case VariantType.SNV:
                    return $"{referenceName}:{start}:{refAllele}";
                case VariantType.reference:
                    return $"{referenceName}:{start}:{end}:{refAllele}";
                default:
                    throw new NotImplementedException($"Unknown variantType ({variantType}) for computing vid");
            }
        }

        private static VariantType DetermineVariantType(bool isRefMinor) => isRefMinor ? VariantType.SNV : VariantType.reference;


        public static IVariant Create(IChromosome chromosome, int start, int end, string refallele, string altAllele,
            string refMinorGlobalMajorAllele)
        {
            bool isRefMinor = end == start && refMinorGlobalMajorAllele != null;
            var variantType = DetermineVariantType(isRefMinor);
            string vid      = GetVid(chromosome.EnsemblName, start, end, refallele, variantType);

            return isRefMinor
                ? new Variant(chromosome, start, end, refMinorGlobalMajorAllele, refallele, variantType, vid,
                    true, false, false, null, null, AnnotationBehavior.RefVariantBehavior)
                : new Variant(chromosome, start, end, refallele, altAllele, variantType, vid, false, false, false, null,
                    null, null);
        }
    }
}