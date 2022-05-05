using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using Vcf.Info;

namespace Vcf.Sample
{
    public sealed class Sample : ISample
    {
        public int[]    AlleleDepths                 { get; }
        public float?   ArtifactAdjustedQualityScore { get; } // PEPE
        public int?     CopyNumber                   { get; }
        public string[] DiseaseAffectedStatuses      { get; } // SMN1
        public bool     FailedFilter                 { get; }
        public string   Genotype                     { get; }
        public int?     GenotypeQuality              { get; }
        public bool     IsDeNovo                     { get; }
        public double?  DeNovoQuality                { get; } //for legacy callers only
        public bool     IsEmpty                      { get; }
        public float?   LikelihoodRatioQualityScore  { get; } // PEPE
        public int[]    PairedEndReadCounts          { get; } // Manta
        public int[]    RepeatUnitCounts             { get; } // ExpansionHunter
        public int[]    SplitReadCounts              { get; } // Manta
        public int?     TotalDepth                   { get; }
        public double[] VariantFrequencies           { get; }
        public int?     MinorHaplotypeCopyNumber     { get; }
        public double?  SomaticQuality               { get; }
        public bool?    IsLossOfHeterozygosity       { get; }
        public string[] HeteroplasmyPercentile       { get; }
        public int?     BinCount                     { get; }
        
        public ICustomFields CustomFields { get; }

        public static readonly Sample EmptySample =
            new Sample(null, null, null, null,
                false, null, null, false, null, 
                null, null, null, null, 
                null, null, null, null, 
                null, null, null);

        public Sample(int[] alleleDepths, float? artifactAdjustedQualityScore, int? copyNumber,
            string[] diseaseAffectedStatuses, bool failedFilter, string genotype, int? genotypeQuality, bool isDeNovo, double? deNovoQuality,
            float? likelihoodRatioQualityScore, int[] pairedEndReadCounts, int[] repeatUnitCounts,
            int[] splitReadCounts, int? totalDepth, double[] variantFrequencies, int? minorHaplotypeCopyNumber, double? somaticQuality, 
            bool? isLossOfHeterozygosity, string[] heteroplasmyPercentile, int? binCount, ICustomFields customFields=null)
        {
            AlleleDepths                 = alleleDepths;
            ArtifactAdjustedQualityScore = artifactAdjustedQualityScore;
            CopyNumber                   = copyNumber;
            DiseaseAffectedStatuses      = diseaseAffectedStatuses;
            FailedFilter                 = failedFilter;
            Genotype                     = genotype;
            GenotypeQuality              = genotypeQuality;
            IsDeNovo                     = isDeNovo;
            DeNovoQuality                = deNovoQuality;
            LikelihoodRatioQualityScore  = likelihoodRatioQualityScore;
            PairedEndReadCounts          = pairedEndReadCounts;
            RepeatUnitCounts             = repeatUnitCounts;
            SplitReadCounts              = splitReadCounts;
            TotalDepth                   = totalDepth;
            VariantFrequencies           = variantFrequencies;
            IsLossOfHeterozygosity       = isLossOfHeterozygosity;
            HeteroplasmyPercentile       = heteroplasmyPercentile;
            MinorHaplotypeCopyNumber     = minorHaplotypeCopyNumber;
            SomaticQuality               = somaticQuality;
            BinCount                     = binCount;
            CustomFields                 = customFields;

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
                      IsLossOfHeterozygosity       == null &&
                      MinorHaplotypeCopyNumber     == null &&
                      SomaticQuality               == null &&
                      HeteroplasmyPercentile       == null &&
                      DeNovoQuality                == null &&
                      BinCount                     == null &&
                      !FailedFilter                        &&
                      !IsDeNovo &&
                      (CustomFields == null || CustomFields.IsEmpty());
        }
    }
}