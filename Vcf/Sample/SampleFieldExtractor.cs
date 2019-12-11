using System;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using Vcf.Sample.Legacy;

namespace Vcf.Sample
{
    internal static class SampleFieldExtractor
    {
        internal static ISample[] ToSamples(this string[] vcfColumns, FormatIndices formatIndices, int numAltAlleles)
        {
            if (vcfColumns.Length < VcfCommon.MinNumColumnsSampleGenotypes) return null;

            int numSamples = vcfColumns.Length - VcfCommon.MinNumColumnsSampleGenotypes + 1;
            var samples    = new ISample[numSamples];

            formatIndices.Set(vcfColumns[VcfCommon.FormatIndex]);

            var legacySampleExtractor = IsLegacyVariantCaller(formatIndices) ? new LegacySampleFieldExtractor(vcfColumns, formatIndices) : null;

            for (int index = VcfCommon.GenotypeIndex; index < vcfColumns.Length; index++)
            {
                samples[index - VcfCommon.GenotypeIndex] = ExtractSample(vcfColumns[index], formatIndices, numAltAlleles, legacySampleExtractor);
            }

            return samples;
        }

        internal static ISample ExtractSample(string sampleColumn, FormatIndices formatIndices, int numAltAlleles,
            LegacySampleFieldExtractor legacyExtractor = null)
        {
            // sanity check: make sure we have a format column
            if (string.IsNullOrEmpty(sampleColumn)) return Sample.EmptySample;

            string[] sampleColumns = sampleColumn.OptimizedSplit(':', formatIndices.NumColumns);
            if (sampleColumns.Length == 1 && sampleColumns[0] == ".") return Sample.EmptySample;

            sampleColumns.NormalizeNulls();

            if (legacyExtractor != null)
            {
                return legacyExtractor.ExtractSample(sampleColumn);
            }

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
            int? minorHaplotypeCopyNumber       = sampleColumns.GetString(formatIndices.MCN).GetInteger();
            double? somaticQuality              = sampleColumns.GetString(formatIndices.SQ).GetDouble();

            double[] variantFrequencies = VariantFrequency.GetVariantFrequencies(variantFrequency, alleleDepths, numAltAlleles);

            var isLoh = GetLoh(copyNumber, minorHaplotypeCopyNumber, genotype);

            var sample = new Sample(alleleDepths, artifactAdjustedQualityScore, copyNumber, diseaseAffectedStatuses,
                failedFilter, genotype, genotypeQuality, isDeNovo, likelihoodRatioQualityScore, pairedEndReadCounts,
                repeatUnitCounts, splitReadCounts, totalDepth, variantFrequencies, minorHaplotypeCopyNumber, somaticQuality, isLoh);

            return sample;
        }

        private static bool? GetLoh(int? copyNumber, int? minorHaplotypeCopyNumber, string genotype)
        {
            if (!minorHaplotypeCopyNumber.HasValue || !copyNumber.HasValue) return null;

            return (genotype == "1/2" || genotype == "1|2") && minorHaplotypeCopyNumber == 0 && copyNumber >= 2;
        }

        private static bool IsLegacyVariantCaller(FormatIndices formatIndices)
        {
            return formatIndices.TAR != null ||
                   formatIndices.TIR != null ||
                   formatIndices.AU != null ||
                   formatIndices.GU != null ||
                   formatIndices.CU != null ||
                   formatIndices.TU != null ||
                   formatIndices.GQX != null ||
                   formatIndices.DPI != null ||
                   formatIndices.MCC != null;

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