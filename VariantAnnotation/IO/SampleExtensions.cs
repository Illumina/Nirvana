using OptimizedCore;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.IO
{
    public static class SampleExtensions
    {
        public static string GetJsonString(this ISample sample)
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddBoolValue("isEmpty",                        sample.IsEmpty);
            jsonObject.AddStringValue("genotype",                     sample.Genotype);
            jsonObject.AddDoubleValues("variantFrequencies",          sample.VariantFrequencies);
            jsonObject.AddIntValue("totalDepth",                      sample.TotalDepth);
            jsonObject.AddIntValue("genotypeQuality",                 sample.GenotypeQuality);
            jsonObject.AddIntValue("copyNumber",                      sample.CopyNumber);
            jsonObject.AddIntValues("repeatUnitCounts",               sample.RepeatUnitCounts);
            jsonObject.AddIntValues("alleleDepths",                   sample.AlleleDepths);
            jsonObject.AddBoolValue("failedFilter",                   sample.FailedFilter);
            jsonObject.AddIntValues("splitReadCounts",                sample.SplitReadCounts);
            jsonObject.AddIntValues("pairedEndReadCounts",            sample.PairedEndReadCounts);
            jsonObject.AddBoolValue("isDeNovo",                       sample.IsDeNovo);
            jsonObject.AddStringValues("diseaseAffectedStatuses",     sample.DiseaseAffectedStatuses);
            jsonObject.AddDoubleValue("artifactAdjustedQualityScore", sample.ArtifactAdjustedQualityScore, "0.#");
            jsonObject.AddDoubleValue("likelihoodRatioQualityScore",  sample.LikelihoodRatioQualityScore, "0.#");

            sb.Append(JsonObject.CloseBrace);
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}