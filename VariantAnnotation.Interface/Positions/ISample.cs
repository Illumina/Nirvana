namespace VariantAnnotation.Interface.Positions
{
    public interface ISample
    {
        double[] VariantFrequencies { get; }
        int? TotalDepth { get; }
        int[] AlleleDepths { get; }
        string Genotype { get; }
        int? GenotypeQuality { get; }
        bool FailedFilter { get; }
        int? CopyNumber { get; }
        bool IsLossOfHeterozygosity { get; }
        float? DeNovoQuality { get; }
        int[] SplitReadCounts { get; }
        int[] PairEndReadCounts { get; }
		string RepeatNumbers { get; }
		string RepeatNumberSpans { get; }

        // SMN1
        int[] MpileupAlleleDepths { get; }
        string SilentCarrierHaplotype { get; }
        int[] ParalogousEntrezGeneIds { get; }
        int[] ParalogousGeneCopyNumbers { get; }
        string[] DiseaseClassificationSources { get; }
        string[] DiseaseIds { get; }
        string[] DiseaseAffectedStatus { get; }
        int[] ProteinAlteringVariantPositions { get; }
        bool IsCompoundHetCompatible { get; }

        // PEPE
        float? ArtifactAdjustedQualityScore { get; }
        float? LikelihoodRatioQualityScore { get; }

        bool IsEmpty { get; }
    }
}