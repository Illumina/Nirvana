using System;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf.VariantCreator
{

	public static class ReferenceVariantCreator 
    {
	    private static readonly AnnotationBehavior RefVariantBehavior = new AnnotationBehavior(true, false, false, true, false, false);

	    private static string GetVid(string ensemblName, int start, int end, string refAllele, VariantType variantType)
        {
            var referenceName = ensemblName;
            switch (variantType)
            {
                case VariantType.SNV:
                    return $"{referenceName}:{start}:{refAllele}";
                case VariantType.reference:
                    return $"{referenceName}:{start}:{end}:{refAllele}";
                default:
                    throw new NotImplementedException($"unknown variantType ({variantType}) for computing vid");
            }
        }

        private static VariantType DetermineVariantType(bool isRefMinor)
        {
            return isRefMinor ? VariantType.SNV : VariantType.reference;
        }

        
        public static IVariant Create(IChromosome chromosome, int start, int end, string refallele, string altAllele, bool isRefMinor)
        {
            var annotationBehavior = end != start || !isRefMinor ? null: RefVariantBehavior;

            var variantType = DetermineVariantType(isRefMinor);
	        var vid = GetVid(chromosome.EnsemblName, start, end, refallele, variantType);

			return new Variant(chromosome, start, end, refallele, altAllele, variantType, vid, isRefMinor, false, null, null, annotationBehavior);
        }
    }
}