using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.VariantCreator
{
	public static class RepeatExpansionCreator
	{
		private static readonly AnnotationBehavior RepeatExpansionBehavior = new AnnotationBehavior(false, false, true, false, false, true);
		public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele, IInfoData infoData)
		{
			start++;//for the padding base
			if (infoData.RefRepeatCount == 0) return null;

		    (int number, bool foundError) = altAllele.Trim('<', '>').Substring(3).OptimizedParseInt32();
		    if (foundError) return null;

            int repeatCount = number;

			var svType = repeatCount == infoData.RefRepeatCount ? VariantType.short_tandem_repeat_variation: 
				repeatCount > infoData.RefRepeatCount
					? VariantType.short_tandem_repeat_expansion
					: VariantType.short_tandem_repeat_contraction;
			
			int end    = infoData.End ?? 0;
			string vid = GetVid(chromosome.EnsemblName, start, end, infoData.RepeatUnit, repeatCount);

			return new Variant(chromosome, start, end, refAllele, altAllele, svType, vid, false, false, false, null, null, RepeatExpansionBehavior);
		}

		private static string GetVid(string ensemblName, int start, int end, string repeatUnit, int repeatCount)
		{
			return $"{ensemblName}:{start}:{end}:{repeatUnit}:{repeatCount}";
		}
	}
}