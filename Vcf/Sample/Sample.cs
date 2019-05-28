using VariantAnnotation.Interface.Positions;

namespace Vcf.Sample
{
    public sealed class Sample : ISample
    {
        public int[] AlleleDepths { get; }
        public float? ArtifactAdjustedQualityScore { get; } // PEPE
        public int? CopyNumber { get; }
        public string[] DiseaseAffectedStatuses { get; } // SMN1
        public bool FailedFilter { get; }
        public string Genotype { get; }
        public int? GenotypeQuality { get; }
        public bool IsDeNovo { get; }
        public bool IsEmpty { get; }
        public float? LikelihoodRatioQualityScore { get; } // PEPE
        public int[] PairedEndReadCounts { get; } // Manta
        public int[] RepeatUnitCounts { get; } // ExpansionHunter
        public int[] SplitReadCounts { get; } // Manta
        public int? TotalDepth { get; }
        public double[] VariantFrequencies { get; }

        public static readonly Sample EmptySample =
            new Sample(null, null, null, null, false, null, null, false, null, null, null, null, null, null);

        public Sample(int[] alleleDepths, float? artifactAdjustedQualityScore, int? copyNumber,
            string[] diseaseAffectedStatuses, bool failedFilter, string genotype, int? genotypeQuality, bool isDeNovo,
            float? likelihoodRatioQualityScore, int[] pairedEndReadCounts, int[] repeatUnitCounts,
            int[] splitReadCounts, int? totalDepth, double[] variantFrequencies)
        {
            AlleleDepths                 = alleleDepths;
            ArtifactAdjustedQualityScore = artifactAdjustedQualityScore;
            CopyNumber                   = copyNumber;
            DiseaseAffectedStatuses      = diseaseAffectedStatuses;
            FailedFilter                 = failedFilter;
            Genotype                     = genotype;
            GenotypeQuality              = genotypeQuality;
            IsDeNovo                     = isDeNovo;
            LikelihoodRatioQualityScore  = likelihoodRatioQualityScore;
            PairedEndReadCounts          = pairedEndReadCounts;
            RepeatUnitCounts             = repeatUnitCounts;
            SplitReadCounts              = splitReadCounts;
            TotalDepth                   = totalDepth;
            VariantFrequencies           = variantFrequencies;


            IsEmpty = AlleleDepths                 == null &&
                      ArtifactAdjustedQualityScore == null &&
                      CopyNumber                   == null &&
                      DiseaseAffectedStatuses      == null &&
                      Genotype                     == null &&
                      GenotypeQuality              == null &&
                      LikelihoodRatioQualityScore  == null &&
                      PairedEndReadCounts          == null &&
                      RepeatUnitCounts             == null &&
                      SplitReadCounts              == null &&
                      TotalDepth                   == null &&
                      VariantFrequencies           == null &&
                      !FailedFilter                        &&
                      !IsDeNovo;
        }
    }
}