using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.VariantCreator
{
	public static class CnvCreator
	{
		public static readonly AnnotationBehavior CnvBehavior = new AnnotationBehavior(false, true, true, false, true);
	    private const string CnvTag = "<CNV>";

		public static IVariant Create(Chromosome chromosome, int start, string refAllele, string altAllele, IInfoData infoData)
		{
			start++;

            // CNV caller's can use <DUP> to indicate a copy number increase where the exact copy number is unknown
		    if (altAllele == "<DUP>")
		    {
		        int dupEnd = infoData.End ?? start;
                string dupVid = $"{chromosome.EnsemblName}:{start}:{dupEnd}:DUP";
                return new Variant(chromosome, start, dupEnd, refAllele, altAllele, VariantType.copy_number_gain, dupVid, false, false, false, null, null, CnvBehavior);
            }

		    var copyNumber = GetCopyNumber(altAllele);
            var svType     = GetType(copyNumber);
            int end        = infoData.End ?? start;
            string vid     = GetVid(chromosome.EnsemblName, start, end, copyNumber);

            return new Variant(chromosome, start, end, refAllele, altAllele, svType, vid, false, false, false, null, null, CnvBehavior);
		}

	    private static int? GetCopyNumber(string altAllele)
	    {
	        if (altAllele == CnvTag) return null;

	        (int number, bool foundError) = altAllele.Trim('<', '>').Substring(2).OptimizedParseInt32();
	        return foundError ? null : number;
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