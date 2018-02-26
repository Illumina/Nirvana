using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf.VariantCreator
{
	public static class CnvCreator
	{
		private static readonly AnnotationBehavior CnvBehavior = new AnnotationBehavior(false, true, true, false, true, true);

	    private const string CnvTag = "<CNV>";

		private static readonly AnnotationBehavior VerbosedCnvBehavior = new AnnotationBehavior(false, true, true, false, true, true, true);
		public static IVariant Create(IChromosome chromosome, string id, int start, string refAllele, string altAllele, IInfoData infoData, int? sampleCopyNumber, bool enableVerboseTranscript)
		{
			start++;
            // CNV caller's can use <DUP> to indicate a copy number increase where the exact copy number is unknown
		    if (altAllele == "<DUP>")
		    {
		        var dupEnd = infoData.End ?? start;
                var dupVid = $"{chromosome.EnsemblName}:{start}:{dupEnd}:DUP";
                return new Variant(chromosome, start, dupEnd, refAllele, altAllele, VariantType.copy_number_gain, dupVid, false, false, null, null, enableVerboseTranscript ? VerbosedCnvBehavior : CnvBehavior);
            }

		    var copyNumber = GetCopyNumber(altAllele, infoData, sampleCopyNumber);

		    var svType     = EvaluateCopyNumberType(copyNumber, id, chromosome.UcscName);
			var end        = infoData.End??start;
			var vid        = GetVid(chromosome.EnsemblName, start, end, copyNumber);

			return new Variant(chromosome, start, end, refAllele, altAllele, svType, vid, false, false, null, null, enableVerboseTranscript ? VerbosedCnvBehavior : CnvBehavior);
		}

	    private static int? GetCopyNumber(string altAllele, IInfoData infoData, int? sampleCopyNumber)
	    {
	        int? copyNumber;
	        //if info copy number is null, check the sample copy number
	        if (altAllele == CnvTag)
	        {
	            copyNumber = infoData.CopyNumber ?? sampleCopyNumber;
	        }
	        else
	        {
	            copyNumber = int.Parse(altAllele.Trim('<', '>').Substring(2));
	        }

	        return copyNumber;
	    }

	    private static string GetVid(string ensemblName, int start, int end, int? copyNumber)
		{
			return $"{ensemblName}:{start}:{end}:{copyNumber}";
		}

		private static VariantType EvaluateCopyNumberType(int? copyNumber, string id, string ucscName)
		{
			if (copyNumber == null) return VariantType.copy_number_variation;

			if (!string.IsNullOrEmpty(id) && id.StartsWith("Canvas:"))
			{
				var canvasInfo = id.Split(':');
				if (canvasInfo[1].Equals("GAIN")) return VariantType.copy_number_gain;
				if (canvasInfo[1].Equals("LOSS")) return VariantType.copy_number_loss;
				if (canvasInfo[1].Equals("REF")) return VariantType.copy_number_variation;
			}

			var baseCopyNumber = ucscName == "chrY" ? 1 : 2;

			if (copyNumber < baseCopyNumber)
			{
				return VariantType.copy_number_loss;
			}

			return copyNumber > baseCopyNumber
				? VariantType.copy_number_gain
				: VariantType.copy_number_variation;
		}

	}
}