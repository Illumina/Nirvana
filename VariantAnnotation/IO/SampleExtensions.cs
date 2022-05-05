using OptimizedCore;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.IO
{
    public static class SampleExtensions
    {
        public static string GetJsonString(this ISample sample)
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddBoolValue("isEmpty",                        sample.IsEmpty);
            jsonObject.AddStringValue("genotype",                  sample.Genotype);
            jsonObject.AddDoubleValues("variantFrequencies",          sample.VariantFrequencies);
            jsonObject.AddIntValue("totalDepth",                      sample.TotalDepth);
            jsonObject.AddIntValue("genotypeQuality",                 sample.GenotypeQuality);
            jsonObject.AddIntValue("copyNumber",                      sample.CopyNumber);
            jsonObject.AddIntValue("minorHaplotypeCopyNumber",        sample.MinorHaplotypeCopyNumber);
            jsonObject.AddIntValues("repeatUnitCounts",               sample.RepeatUnitCounts);
            jsonObject.AddIntValues("alleleDepths",                   sample.AlleleDepths);
            jsonObject.AddBoolValue("failedFilter",                   sample.FailedFilter);
            jsonObject.AddIntValues("splitReadCounts",                sample.SplitReadCounts);
            jsonObject.AddIntValues("pairedEndReadCounts",            sample.PairedEndReadCounts);
            jsonObject.AddBoolValue("isDeNovo",                       sample.IsDeNovo);
            jsonObject.AddDoubleValue("deNovoQuality",                sample.DeNovoQuality);
            jsonObject.AddStringValues("diseaseAffectedStatuses",     sample.DiseaseAffectedStatuses);
            jsonObject.AddDoubleValue("artifactAdjustedQualityScore", sample.ArtifactAdjustedQualityScore, "0.#");
            jsonObject.AddDoubleValue("likelihoodRatioQualityScore",  sample.LikelihoodRatioQualityScore, "0.#");
            if (sample.IsLossOfHeterozygosity.HasValue)
                jsonObject.AddBoolValue("lossOfHeterozygosity", sample.IsLossOfHeterozygosity.Value);
            jsonObject.AddDoubleValue("somaticQuality",               sample.SomaticQuality, "0.#");
            jsonObject.AddStringValues("heteroplasmyPercentile",      sample.HeteroplasmyPercentile, false);
            jsonObject.AddIntValue("binCount",                      sample.BinCount);
            
            if(sample.CustomFields != null && !sample.CustomFields.IsEmpty())
                jsonObject.AddObjectValue("vcfSampleInfo", sample.CustomFields);
            
            sb.Append(JsonObject.CloseBrace);
            return StringBuilderPool.GetStringAndReturn(sb);
        }
    }
}