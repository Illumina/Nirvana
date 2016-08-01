using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
	public static class VID
	{
		/// <summary>
		/// constructs a VID based on the supplied feature
		/// </summary>
		public static string Create(string referenceName, VariantAlternateAllele altAllele)
		{
			return GetVid(referenceName, altAllele.NirvanaVariantType, altAllele.ReferenceBegin, altAllele.ReferenceEnd, altAllele.AlternateAllele, altAllele.CopyNumber, altAllele.BreakEnd?.ToString(),altAllele.IsStructuralVariant);

		}

	    private static string GetVid(string referenceName, VariantType variantType, int refBegin, int refEnd, string altAllele, string copyNumber = null, string breakEnd = null,bool isStructuralVariant = false)
		{
			referenceName = AnnotationLoader.Instance.ChromosomeRenamer.GetEnsemblReferenceName(referenceName);
            string vid;
			switch (variantType)
			{
				case VariantType.SNV:
					vid = $"{referenceName}:{refBegin}:{altAllele}";
					break;

				case VariantType.insertion:
					vid = isStructuralVariant ? $"{referenceName}:{refBegin}:{refEnd}:INS" : $"{referenceName}:{refBegin}:{refEnd}:{(altAllele.Length > 32 ? SupplementaryAnnotation.GetMd5HashString(altAllele) : altAllele)}";
					break;

				case VariantType.deletion:
					vid = $"{referenceName}:{refBegin}:{refEnd}";
					break;

				case VariantType.MNV:
				case VariantType.indel:
					vid =
						$"{referenceName}:{refBegin}:{refEnd}:{(altAllele.Length > 32 ? SupplementaryAnnotation.GetMd5HashString(altAllele) : altAllele)}";
					break;

				case VariantType.duplication:
					vid = $"{referenceName}:{refBegin}:{refEnd}:DUP";
					break;

				case VariantType.tandem_duplication:
					vid = $"{referenceName}:{refBegin}:{refEnd}:TDUP";
					break;

				case VariantType.translocation_breakend:
					vid = breakEnd;
					break;

				case VariantType.inversion:
					vid = $"{referenceName}:{refBegin}:{refEnd}:Inverse";
					break;

				case VariantType.mobile_element_insertion:
					vid = $"{referenceName}:{refBegin}:{refEnd}:MEI";
					break;

				case VariantType.copy_number_gain:
				case VariantType.copy_number_loss:
				case VariantType.copy_number_variation:
					vid = $"{referenceName}:{refBegin}:{refEnd}:{copyNumber}";
					break;

				case VariantType.reference_no_call:
					vid = $"{referenceName}:{refBegin}:{refEnd}:NC";
					break;

				default:
					vid = $"{referenceName}:{refBegin}:{refEnd}";
					break;
			}
			return vid;
		}
	}
}
