using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf.VariantCreator
{
	public static class CnvCreator
	{
		private static readonly AnnotationBehavior CnvBehavior = new AnnotationBehavior(false, true, true, false, true, true);

		private static readonly AnnotationBehavior VerbosedCnvBehavior = new AnnotationBehavior(false, true, true, false, true, true, true);
		public static IVariant Create(IChromosome chromosome, string id, int start, string refAllele, IInfoData infoData, int? sampleCopyNumber, bool enableVerboseTranscript)
		{
			start++;
			//if info copy number is null, check the sample copy number
			var copyNumber = infoData.CopyNumber ?? sampleCopyNumber;
			var svType     = EvaluateCopyNumberType(copyNumber, id, chromosome.UcscName);
			var end        = infoData.End??start;
			var vid        = GetVid(chromosome.EnsemblName, start, end, copyNumber);

			return new Variant(chromosome, start, end, refAllele, "CNV", svType, vid, false, false, null, null, enableVerboseTranscript ? VerbosedCnvBehavior : CnvBehavior);
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