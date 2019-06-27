using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.VariantCreator
{
    public static class RepeatExpansionCreator
    {
        public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele, IInfoData infoData)
        {
            start++;//for the padding base
            if (infoData.RefRepeatCount == 0) return null;

            (int repeatCount, bool foundError) = altAllele.Trim('<', '>').Substring(3).OptimizedParseInt32();
            if (foundError) return null;

            if (infoData.RefRepeatCount != null)
            {
                var svType = GetRepeatExpansionType(infoData.RefRepeatCount.Value, repeatCount);

                int end = infoData.End ?? 0;
                string vid = GetVid(chromosome.EnsemblName, start, end, infoData.RepeatUnit, repeatCount);

                return new Variant(chromosome, start, end, refAllele, altAllele, svType, vid, false, false, false, null,
                    null, AnnotationBehavior.RepeatExpansionBehavior);
            }
            return null;
        }

        private static VariantType GetRepeatExpansionType(int refRepeatCount, int repeatCount)
        {
            if (refRepeatCount == repeatCount) return VariantType.short_tandem_repeat_variation;
            return repeatCount > refRepeatCount ? VariantType.short_tandem_repeat_expansion : VariantType.short_tandem_repeat_contraction;
        }

        private static string GetVid(string ensemblName, int start, int end, string repeatUnit, int repeatCount)
        {
            return $"{ensemblName}:{start}:{end}:{repeatUnit}:{repeatCount}";
        }
    }
}