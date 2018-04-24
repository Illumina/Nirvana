﻿using OptimizedCore;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf.VariantCreator
{
	public static class CnvCreator
	{
		private static readonly AnnotationBehavior CnvBehavior = new AnnotationBehavior(false, true, true, false, true, true);

	    private const string CnvTag = "<CNV>";

		private static readonly AnnotationBehavior VerbosedCnvBehavior = new AnnotationBehavior(false, true, true, false, true, true, true);

		public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele, IInfoData infoData, bool enableVerboseTranscript)
		{
			start++;

            // CNV caller's can use <DUP> to indicate a copy number increase where the exact copy number is unknown
		    if (altAllele == "<DUP>")
		    {
		        int dupEnd = infoData.End ?? start;
                string dupVid = $"{chromosome.EnsemblName}:{start}:{dupEnd}:DUP";
                return new Variant(chromosome, start, dupEnd, refAllele, altAllele, VariantType.copy_number_gain, dupVid, false, false, false, null, null, enableVerboseTranscript ? VerbosedCnvBehavior : CnvBehavior);
            }

		    var copyNumber = GetCopyNumber(altAllele);
            var svType     = GetType(copyNumber);
            int end        = infoData.End ?? start;
            string vid     = GetVid(chromosome.EnsemblName, start, end, copyNumber);

            return new Variant(chromosome, start, end, refAllele, altAllele, svType, vid, false, false, false, null, null, enableVerboseTranscript ? VerbosedCnvBehavior : CnvBehavior);
		}

	    private static int? GetCopyNumber(string altAllele)
	    {
	        if (altAllele == CnvTag) return null;

	        (int number, bool foundError) = altAllele.Trim('<', '>').Substring(2).OptimizedParseInt32();
	        return foundError ? null : (int?)number;
        }

	    private static string GetVid(string ensemblName, int start, int end, int? copyNumber)
	    {
	        return copyNumber==null ? $"{ensemblName}:{start}:{end}:CNV" : $"{ensemblName}:{start}:{end}:{copyNumber}";
	    }

		private static VariantType GetType(int? copyNumber)
		{
			if (copyNumber == null || copyNumber==1) return VariantType.copy_number_variation;

			return copyNumber > 1
				? VariantType.copy_number_gain
				: VariantType.copy_number_loss;
		}

	}
}