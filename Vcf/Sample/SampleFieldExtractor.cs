using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Vcf.Sample
{
    internal static class SampleFieldExtractor
    {
        internal static ISample[] ToSamples(this string[] vcfColumns, FormatIndices formatIndices, int numAltAlleles, bool isRepeatExpansion)
        {
            if (vcfColumns.Length < VcfCommon.MinNumColumnsSampleGenotypes) return null;

            int numSamples = vcfColumns.Length - VcfCommon.MinNumColumnsSampleGenotypes + 1;
            var samples    = new ISample[numSamples];

            formatIndices.Set(vcfColumns[VcfCommon.FormatIndex]);

            for (int index = VcfCommon.GenotypeIndex; index < vcfColumns.Length; index++)
            {
                samples[index - VcfCommon.GenotypeIndex] = ExtractSample(vcfColumns[index], formatIndices, numAltAlleles, isRepeatExpansion);
            }

            return samples;
        }

        internal static ISample ExtractSample(string sampleColumn, FormatIndices formatIndices, int numAltAlleles, bool isRepeatExpansion)
        {
            // sanity check: make sure we have a format column
            if (string.IsNullOrEmpty(sampleColumn)) return Sample.EmptySample;

            string[] sampleColumns = sampleColumn.OptimizedSplit(':', formatIndices.NumColumns);
            if (sampleColumns.Length == 1 && sampleColumns[0] == ".") return Sample.EmptySample;

            sampleColumns.NormalizeNulls();

            int[] alleleDepths                  = sampleColumns.GetString(formatIndices.AD).GetIntegers();
            float? artifactAdjustedQualityScore = sampleColumns.GetString(formatIndices.AQ).GetFloat();
            int? copyNumber                     = sampleColumns.GetString(formatIndices.CN).GetInteger();
            string[] diseaseAffectedStatuses    = sampleColumns.GetString(formatIndices.DST).GetStrings();
            bool failedFilter                   = sampleColumns.GetString(formatIndices.FT).GetFailedFilter();
            string genotype                     = sampleColumns.GetString(formatIndices.GT);
            int? genotypeQuality                = sampleColumns.GetString(formatIndices.GQ).GetInteger();
            bool isDeNovo                       = sampleColumns.GetString(formatIndices.DN).IsDeNovo();
            float? likelihoodRatioQualityScore  = sampleColumns.GetString(formatIndices.LQ).GetFloat();
            int[] pairedEndReadCounts           = sampleColumns.GetString(formatIndices.PR).GetIntegers();
            int[] repeatUnitCounts              = sampleColumns.GetString(formatIndices.REPCN).GetIntegers('/');
            int[] splitReadCounts               = sampleColumns.GetString(formatIndices.SR).GetIntegers();
            int? totalDepth                     = sampleColumns.GetString(formatIndices.DP).GetInteger();
            double? variantFrequency            = sampleColumns.GetString(formatIndices.VF).GetDouble();

            double[] variantFrequencies = VariantFrequency.GetVariantFrequencies(variantFrequency, alleleDepths, numAltAlleles);

            var sample = new Sample(alleleDepths, artifactAdjustedQualityScore, copyNumber, diseaseAffectedStatuses,
                failedFilter, genotype, genotypeQuality, isDeNovo, likelihoodRatioQualityScore, pairedEndReadCounts,
                repeatUnitCounts, splitReadCounts, totalDepth, variantFrequencies);

            return sample;
        }

        internal static void NormalizeNulls(this string[] cols)
        {
            for (var i = 0; i < cols.Length; i++)
            {
                string col = cols[i];
                if (col == null) continue;
                if (col.Length == 0 || col == ".") cols[i] = null;
            }
        }
    }
}