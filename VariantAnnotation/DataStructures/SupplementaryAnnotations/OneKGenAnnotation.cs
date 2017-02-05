using System.Globalization;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public sealed class OneKGenAnnotation:ISupplementaryAnnotation
	{
		#region members
		public string AncestralAllele;

		public int? OneKgAllAn;
		public int? OneKgAfrAn;
		public int? OneKgAmrAn;
		public int? OneKgEasAn;
		public int? OneKgEurAn;
		public int? OneKgSasAn;

		public int? OneKgAllAc;
		public int? OneKgAfrAc;
		public int? OneKgAmrAc;
		public int? OneKgEasAc;
		public int? OneKgEurAc;
		public int? OneKgSasAc;
		#endregion
		public bool HasConflicts { get; private set; }

		public void Read(ExtendedBinaryReader reader)
		{
			AncestralAllele = reader.ReadAsciiString();

			OneKgAllAn = reader.ReadOptNullableInt32();
			OneKgAfrAn = reader.ReadOptNullableInt32();
			OneKgAmrAn = reader.ReadOptNullableInt32();
			OneKgEasAn = reader.ReadOptNullableInt32();
			OneKgEurAn = reader.ReadOptNullableInt32();
			OneKgSasAn = reader.ReadOptNullableInt32();

			OneKgAllAc = reader.ReadOptNullableInt32();
			OneKgAfrAc = reader.ReadOptNullableInt32();
			OneKgAmrAc = reader.ReadOptNullableInt32();
			OneKgEasAc = reader.ReadOptNullableInt32();
			OneKgEurAc = reader.ReadOptNullableInt32();
			OneKgSasAc = reader.ReadOptNullableInt32();
		}

		public void AddAnnotationToVariant(IAnnotatedAlternateAllele jsonVariant)
		{
			jsonVariant.AncestralAllele = AncestralAllele;

			jsonVariant.OneKgAlleleNumberAfrican = OneKgAfrAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberAmerican = OneKgAmrAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberAll = OneKgAllAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberEastAsian = OneKgEasAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberEuropean = OneKgEurAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberSouthAsian = OneKgSasAn?.ToString(CultureInfo.InvariantCulture);

			jsonVariant.OneKgAlleleCountAfrican = OneKgAfrAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountAmerican = OneKgAmrAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountAll = OneKgAllAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountEastAsian = OneKgEasAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountEuropean = OneKgEurAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountSouthAsian = OneKgSasAc?.ToString(CultureInfo.InvariantCulture);

			jsonVariant.AlleleFrequencyAll = ComputeFrequency(OneKgAllAn, OneKgAllAc);
			jsonVariant.AlleleFrequencyAfrican = ComputeFrequency(OneKgAfrAn, OneKgAfrAc);
			jsonVariant.AlleleFrequencyAdMixedAmerican = ComputeFrequency(OneKgAmrAn, OneKgAmrAc);
			jsonVariant.AlleleFrequencyEastAsian = ComputeFrequency(OneKgEasAn, OneKgEasAc);
			jsonVariant.AlleleFrequencyEuropean = ComputeFrequency(OneKgEurAn, OneKgEurAc);
			jsonVariant.AlleleFrequencySouthAsian = ComputeFrequency(OneKgSasAn, OneKgSasAc);
		}

		private static string ComputeFrequency(int? alleleNumber, int? alleleCount)
		{
			return alleleNumber != null && alleleNumber.Value > 0 && alleleCount != null
				? ((double)alleleCount / alleleNumber.Value).ToString(JsonCommon.FrequencyRoundingFormat)
				: null;
		}


		public void Write(ExtendedBinaryWriter writer)
		{
			writer.WriteOptAscii(AncestralAllele);

			writer.WriteOpt(OneKgAllAn);
			writer.WriteOpt(OneKgAfrAn);
			writer.WriteOpt(OneKgAmrAn);
			writer.WriteOpt(OneKgEasAn);
			writer.WriteOpt(OneKgEurAn);
			writer.WriteOpt(OneKgSasAn);

			writer.WriteOpt(OneKgAllAc);
			writer.WriteOpt(OneKgAfrAc);
			writer.WriteOpt(OneKgAmrAc);
			writer.WriteOpt(OneKgEasAc);
			writer.WriteOpt(OneKgEurAc);
			writer.WriteOpt(OneKgSasAc);
		}

		public void MergeAnnotations(ISupplementaryAnnotation other)
		{
			var otherAnnotation = other as OneKGenAnnotation;

			if (otherAnnotation?.OneKgAllAn == null)
				return;

			if (OneKgAllAc == null)
			{
				AncestralAllele = otherAnnotation.AncestralAllele;

				OneKgAllAn = otherAnnotation.OneKgAllAn;
				OneKgAfrAn = otherAnnotation.OneKgAfrAn;
				OneKgAmrAn = otherAnnotation.OneKgAmrAn;
				OneKgEurAn = otherAnnotation.OneKgEurAn;
				OneKgEasAn = otherAnnotation.OneKgEasAn;
				OneKgSasAn = otherAnnotation.OneKgSasAn;

				OneKgAllAc = otherAnnotation.OneKgAllAc;
				OneKgAfrAc = otherAnnotation.OneKgAfrAc;
				OneKgAmrAc = otherAnnotation.OneKgAmrAc;
				OneKgEurAc = otherAnnotation.OneKgEurAc;
				OneKgEasAc = otherAnnotation.OneKgEasAc;
				OneKgSasAc = otherAnnotation.OneKgSasAc;

			}
			else
			{
				HasConflicts = true;
				Clear();
			}
		}

		public void Clear()
		{
			AncestralAllele = null;

			OneKgAllAn = null;
			OneKgAfrAn = null;
			OneKgAmrAn = null;
			OneKgEurAn = null;
			OneKgEasAn = null;
			OneKgSasAn = null;

			OneKgAllAc = null;
			OneKgAfrAc = null;
			OneKgAmrAc = null;
			OneKgEurAc = null;
			OneKgEasAc = null;
			OneKgSasAc = null;
		}
	}
}