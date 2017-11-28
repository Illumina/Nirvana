using System.Security.Cryptography;
using System.Text;
using CommonUtilities;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf.VariantCreator
{
	public static class SmallVariantCreator
	{
		private static readonly AnnotationBehavior SmallVariantBehavior = new AnnotationBehavior(true, false, false, true, false, false);


		public static IVariant Create(IChromosome chromosome, int start, string refAllele, string altAllele)
		{
			var trimmedVariant = BiDirectionalTrimmer.Trim(start, refAllele, altAllele);
			start     = trimmedVariant.Item1;
			refAllele = trimmedVariant.Item2;
			altAllele = trimmedVariant.Item3;
			var end   = start + refAllele.Length - 1;

			var variantType = GetVariantType(refAllele, altAllele);
			var vid = GetVid(chromosome.EnsemblName, start, end, altAllele, variantType);

			return new Variant(chromosome, start, end, refAllele, altAllele, variantType, vid, false, false, null, null, SmallVariantBehavior);
		}
		private static string GetVid(string ensemblName, int start, int end, string altAllele, VariantType type)
		{
            var referenceName = ensemblName;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case VariantType.SNV:
					return $"{referenceName}:{start}:{altAllele}";
				case VariantType.insertion:
					return $"{referenceName}:{start}:{end}:{GetInsertedAltAllele(altAllele)}";
				case VariantType.deletion:
					return $"{referenceName}:{start}:{end}";
				case VariantType.MNV:
				case VariantType.indel:
					return $"{referenceName}:{start}:{end}:{GetInsertedAltAllele(altAllele)}";
				default:
					return null;
			}
		}

		private static string GetInsertedAltAllele(string altAllele)
		{
			if (altAllele.Length <= 32) return altAllele;

			var md5Hash = MD5.Create();
			var md5Builder = new StringBuilder();

			var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(altAllele));

			md5Builder.Clear();
			foreach (var b in data) md5Builder.Append(b.ToString("x2"));
			return md5Builder.ToString();
		}

		private static VariantType GetVariantType(string refAllele, string altAllele)
		{
			var referenceAlleleLen = refAllele.Length;
			var alternateAlleleLen = altAllele.Length;
			if (alternateAlleleLen != referenceAlleleLen)
			{
				if (alternateAlleleLen == 0 && referenceAlleleLen > 0) return VariantType.deletion;
				if (alternateAlleleLen > 0 && referenceAlleleLen == 0) return VariantType.insertion;

				return VariantType.indel;
			}

			var variantType = alternateAlleleLen == 1 ? VariantType.SNV : VariantType.MNV;

			return variantType;
		}


	}
}