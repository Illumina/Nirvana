using VariantAnnotation.Interface.Positions;

namespace Vcf.Sample
{
    public sealed class Sample : ISample
    {
        public double[] VariantFrequencies { get; }
        public int? TotalDepth { get; }
        public int[] AlleleDepths { get; }
        public string Genotype { get; }
        public int? GenotypeQuality { get; }
        public bool FailedFilter { get; }
        public int? CopyNumber { get; }
        public bool IsLossOfHeterozygosity { get; }
        public float? DeNovoQuality { get; }
        public int[] SplitReadCounts { get; }
        public int[] PairEndReadCounts { get; }
	    public string RepeatNumbers { get; }
	    public string RepeatNumberSpans { get; }

        // SMN1
        public int[] MpileupAlleleDepths { get; }
        public string SilentCarrierHaplotype { get; }
        public int[] ParalogousEntrezGeneIds { get; }
        public int[] ParalogousGeneCopyNumbers { get; }
        public string[] DiseaseClassificationSources { get; }
        public string[] DiseaseIds { get; }
        public string[] DiseaseAffectedStatus { get; }
        public int[] ProteinAlteringVariantPositions { get; }
        public bool IsCompoundHetCompatible { get; }

        // PEPE
        public float? ArtifactAdjustedQualityScore { get; }
        public float? LikelihoodRatioQualityScore { get; }

        public bool IsEmpty { get; }

        public static readonly Sample EmptySample = new Sample(null, null, null, null, null, false, null, false, null,
            null, null, null, null, null, null, null, null, null, null, null, null, false, null, null);

        public Sample(string genotype, int? genotypeQuality, double[] variantFrequencies, int? totalDepth,
            int[] alleleDepths, bool failedFilter, int? copyNumber, bool isLossOfHeterozygosity, float? deNovoQuality,
            int[] splitReadCounts, int[] pairEndReadCounts, string repeatNumbers, string repeatNumberSpan,
            int[] mpileupAlleleDepths, string silentCarrierHaplotype, int[] paralagousEntrezGeneIds,
            int[] paralogousGeneCopyNumbers, string[] diseaseClassificationSources, string[] diseaseIds,
            string[] diseaseAffectedStatus, int[] proteinAlteringVariantPositions, bool isCompoundHetCompatible,
            float? artifactAdjustedQualityScore, float? likelihoodRatioQualityScore)
        {
            Genotype                        = genotype;
            GenotypeQuality                 = genotypeQuality;
            VariantFrequencies              = variantFrequencies;
            TotalDepth                      = totalDepth;
            AlleleDepths                    = alleleDepths;
            FailedFilter                    = failedFilter;
            CopyNumber                      = copyNumber;
            IsLossOfHeterozygosity          = isLossOfHeterozygosity;
            DeNovoQuality                   = deNovoQuality;
            SplitReadCounts                 = splitReadCounts;
            PairEndReadCounts               = pairEndReadCounts;
            RepeatNumbers                   = repeatNumbers;
            RepeatNumberSpans               = repeatNumberSpan;
            MpileupAlleleDepths             = mpileupAlleleDepths;
            SilentCarrierHaplotype          = silentCarrierHaplotype;
            ParalogousEntrezGeneIds         = paralagousEntrezGeneIds;
            ParalogousGeneCopyNumbers       = paralogousGeneCopyNumbers;
            DiseaseClassificationSources    = diseaseClassificationSources;
            DiseaseIds                      = diseaseIds;
            DiseaseAffectedStatus           = diseaseAffectedStatus;
            ProteinAlteringVariantPositions = proteinAlteringVariantPositions;
            IsCompoundHetCompatible         = isCompoundHetCompatible;
            ArtifactAdjustedQualityScore    = artifactAdjustedQualityScore;
            LikelihoodRatioQualityScore     = likelihoodRatioQualityScore;

            IsEmpty = Genotype == null && GenotypeQuality == null && VariantFrequencies == null && TotalDepth == null &&
                      AlleleDepths == null && !FailedFilter && CopyNumber == null && !IsLossOfHeterozygosity &&
                      DeNovoQuality == null && SplitReadCounts == null && PairEndReadCounts == null &&
                      RepeatNumbers == null && RepeatNumberSpans == null && MpileupAlleleDepths == null &&
                      SilentCarrierHaplotype == null && ParalogousEntrezGeneIds == null &&
                      ParalogousGeneCopyNumbers == null && DiseaseClassificationSources == null && DiseaseIds == null &&
                      DiseaseAffectedStatus == null && ProteinAlteringVariantPositions == null &&
                      !IsCompoundHetCompatible && ArtifactAdjustedQualityScore == null &&
                      LikelihoodRatioQualityScore == null;
        }
    }
}