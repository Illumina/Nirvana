using System.Globalization;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class ExacAnnotation : ISupplementaryAnnotation
    {
        #region members
        public int ExacCoverage;

        public int? ExacAllAn;
        public int? ExacAfrAn;
        public int? ExacAmrAn;
        public int? ExacEasAn;
        public int? ExacFinAn;
        public int? ExacNfeAn;
        public int? ExacOthAn;
        public int? ExacSasAn;

        public int? ExacAllAc;
        public int? ExacAfrAc;
        public int? ExacAmrAc;
        public int? ExacEasAc;
        public int? ExacFinAc;
        public int? ExacNfeAc;
        public int? ExacOthAc;
        public int? ExacSasAc;
        #endregion

        public bool HasConflicts { get; private set; }

        public void Read(ExtendedBinaryReader reader)
        {
            ExacCoverage = reader.ReadOptInt32();

            ExacAllAn = reader.ReadOptNullableInt32();
            ExacAfrAn = reader.ReadOptNullableInt32();
            ExacAmrAn = reader.ReadOptNullableInt32();
            ExacEasAn = reader.ReadOptNullableInt32();
            ExacFinAn = reader.ReadOptNullableInt32();
            ExacNfeAn = reader.ReadOptNullableInt32();
            ExacOthAn = reader.ReadOptNullableInt32();
            ExacSasAn = reader.ReadOptNullableInt32();

            ExacAllAc = reader.ReadOptNullableInt32();
            ExacAfrAc = reader.ReadOptNullableInt32();
            ExacAmrAc = reader.ReadOptNullableInt32();
            ExacEasAc = reader.ReadOptNullableInt32();
            ExacFinAc = reader.ReadOptNullableInt32();
            ExacNfeAc = reader.ReadOptNullableInt32();
            ExacOthAc = reader.ReadOptNullableInt32();
            ExacSasAc = reader.ReadOptNullableInt32();
        }



        public void AddAnnotationToVariant(IAnnotatedAlternateAllele jsonVariant)
        {
            jsonVariant.ExacCoverage = ExacCoverage > 0 ? ExacCoverage.ToString(CultureInfo.InvariantCulture) : null;
            jsonVariant.ExacAlleleNumberAfrican = ExacAfrAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberAmerican = ExacAmrAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberAll = ExacAllAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberEastAsian = ExacEasAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberFinish = ExacFinAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberNonFinish = ExacNfeAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberOther = ExacOthAn?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleNumberSouthAsian = ExacSasAn?.ToString(CultureInfo.InvariantCulture);

            jsonVariant.ExacAlleleCountAfrican = ExacAfrAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountAmerican = ExacAmrAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountAll = ExacAllAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountEastAsian = ExacEasAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountFinish = ExacFinAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountNonFinish = ExacNfeAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountOther = ExacOthAc?.ToString(CultureInfo.InvariantCulture);
            jsonVariant.ExacAlleleCountSouthAsian = ExacSasAc?.ToString(CultureInfo.InvariantCulture);

            jsonVariant.ExacAlleleFrequencyAfrican = ComputeFrequency(ExacAfrAn, ExacAfrAc);
            jsonVariant.ExacAlleleFrequencyAmerican = ComputeFrequency(ExacAmrAn, ExacAmrAc);
            jsonVariant.ExacAlleleFrequencyAll = ComputeFrequency(ExacAllAn, ExacAllAc);
            jsonVariant.ExacAlleleFrequencyEastAsian = ComputeFrequency(ExacEasAn, ExacEasAc);
            jsonVariant.ExacAlleleFrequencyFinish = ComputeFrequency(ExacFinAn, ExacFinAc);
            jsonVariant.ExacAlleleFrequencyNonFinish = ComputeFrequency(ExacNfeAn, ExacNfeAc);
            jsonVariant.ExacAlleleFrequencyOther = ComputeFrequency(ExacOthAn, ExacOthAc);
            jsonVariant.ExacAlleleFrequencySouthAsian = ComputeFrequency(ExacSasAn, ExacSasAc);
        }

        private static string ComputeFrequency(int? alleleNumber, int? alleleCount)
        {
            return alleleNumber != null && alleleNumber.Value > 0 && alleleCount != null
                ? ((double)alleleCount / alleleNumber.Value).ToString(JsonCommon.FrequencyRoundingFormat)
                : null;
        }


        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(ExacCoverage);

            writer.WriteOpt(ExacAllAn);
            writer.WriteOpt(ExacAfrAn);
            writer.WriteOpt(ExacAmrAn);
            writer.WriteOpt(ExacEasAn);
            writer.WriteOpt(ExacFinAn);
            writer.WriteOpt(ExacNfeAn);
            writer.WriteOpt(ExacOthAn);
            writer.WriteOpt(ExacSasAn);

            writer.WriteOpt(ExacAllAc);
            writer.WriteOpt(ExacAfrAc);
            writer.WriteOpt(ExacAmrAc);
            writer.WriteOpt(ExacEasAc);
            writer.WriteOpt(ExacFinAc);
            writer.WriteOpt(ExacNfeAc);
            writer.WriteOpt(ExacOthAc);
            writer.WriteOpt(ExacSasAc);
        }


        public void Clear()
        {
            ExacCoverage = 0;

            ExacAllAn = null;
            ExacAfrAn = null;
            ExacAmrAn = null;
            ExacEasAn = null;
            ExacFinAn = null;
            ExacNfeAn = null;
            ExacOthAn = null;
            ExacSasAn = null;

            ExacAllAc = null;
            ExacAfrAc = null;
            ExacAmrAc = null;
            ExacEasAc = null;
            ExacFinAc = null;
            ExacNfeAc = null;
            ExacOthAc = null;
            ExacSasAc = null;
        }

        public void MergeAnnotations(ISupplementaryAnnotation other)
        {
            var otherAnnotation = other as ExacAnnotation;

            if (otherAnnotation?.ExacAllAn == null || otherAnnotation.ExacAllAn.Value == 0 || HasConflicts)
                return;

            if (ExacAllAn == null || ExacAllAn.Value == 0)
            {
                ExacCoverage = otherAnnotation.ExacCoverage;

                ExacAllAn = otherAnnotation.ExacAllAn;
                ExacAfrAn = otherAnnotation.ExacAfrAn;
                ExacAmrAn = otherAnnotation.ExacAmrAn;
                ExacEasAn = otherAnnotation.ExacEasAn;
                ExacFinAn = otherAnnotation.ExacFinAn;
                ExacNfeAn = otherAnnotation.ExacNfeAn;
                ExacOthAn = otherAnnotation.ExacOthAn;
                ExacSasAn = otherAnnotation.ExacSasAn;

                ExacAllAc = otherAnnotation.ExacAllAc;
                ExacAfrAc = otherAnnotation.ExacAfrAc;
                ExacAmrAc = otherAnnotation.ExacAmrAc;
                ExacEasAc = otherAnnotation.ExacEasAc;
                ExacFinAc = otherAnnotation.ExacFinAc;
                ExacNfeAc = otherAnnotation.ExacNfeAc;
                ExacOthAc = otherAnnotation.ExacOthAc;
                ExacSasAc = otherAnnotation.ExacSasAc;
            }
            else
            {
                // this is a conflict
                HasConflicts = true;
                Clear();
            }
        }
    }
}