using CommonUtilities;
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

            jsonObject.AddBoolValue("isEmpty", sample.IsEmpty);
            jsonObject.AddStringValue("genotype", sample.Genotype);
	        jsonObject.AddStringValue("repeatNumbers", sample.RepeatNumbers);
	        jsonObject.AddStringValue("repeatNumberSpans", sample.RepeatNumberSpans);
			jsonObject.AddDoubleValues("variantFrequencies", sample.VariantFrequencies);
            jsonObject.AddIntValue("totalDepth", sample.TotalDepth);
            jsonObject.AddIntValue("genotypeQuality", sample.GenotypeQuality);
            jsonObject.AddIntValue("copyNumber", sample.CopyNumber);
            jsonObject.AddIntValues("alleleDepths", sample.AlleleDepths);
            jsonObject.AddBoolValue("failedFilter", sample.FailedFilter);
            jsonObject.AddIntValues("splitReadCounts", sample.SplitReadCounts);
            jsonObject.AddIntValues("pairedEndReadCounts", sample.PairEndReadCounts);
            jsonObject.AddBoolValue("lossOfHeterozygosity", sample.IsLossOfHeterozygosity);
            jsonObject.AddDoubleValue("deNovoQuality", sample.DeNovoQuality, "0.#");

            jsonObject.AddIntValues("mpileupAlleleDepths",             sample.MpileupAlleleDepths);
            jsonObject.AddStringValue("silentCarrierHaplotype",        sample.SilentCarrierHaplotype);
            jsonObject.AddIntValues("paralogousEntrezGeneIds",         sample.ParalogousEntrezGeneIds);
            jsonObject.AddIntValues("paralogousGeneCopyNumbers",       sample.ParalogousGeneCopyNumbers);
            jsonObject.AddStringValues("diseaseClassificationSources", sample.DiseaseClassificationSources);
            jsonObject.AddStringValues("diseaseIds",                   sample.DiseaseIds);
            jsonObject.AddStringValues("diseaseAffectedStatuses",      sample.DiseaseAffectedStatus);
            jsonObject.AddIntValues("proteinAlteringVariantPositions", sample.ProteinAlteringVariantPositions);
            jsonObject.AddBoolValue("isCompoundHetCompatible",         sample.IsCompoundHetCompatible);

            jsonObject.AddDoubleValue("artifactAdjustedQualityScore", sample.ArtifactAdjustedQualityScore, "0.#");
            jsonObject.AddDoubleValue("likelihoodRatioQualityScore",  sample.LikelihoodRatioQualityScore, "0.#");

            sb.Append(JsonObject.CloseBrace);
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}