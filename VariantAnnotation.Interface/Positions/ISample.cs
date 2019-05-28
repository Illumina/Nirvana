namespace VariantAnnotation.Interface.Positions
{
    public interface ISample
    {
        int[] AlleleDepths { get; }
        float? ArtifactAdjustedQualityScore { get; } // PEPE
        int? CopyNumber { get; }
        string[] DiseaseAffectedStatuses { get; } // SMN1
        bool FailedFilter { get; }
        string Genotype { get; }
        int? GenotypeQuality { get; }
        bool IsDeNovo { get; }
        bool IsEmpty { get; }
        float? LikelihoodRatioQualityScore { get; } // PEPE
        int[] PairedEndReadCounts { get; } // Manta
        int[] RepeatUnitCounts { get; } // ExpansionHunter
        int[] SplitReadCounts { get; } // Manta
        int? TotalDepth { get; }
        double[] VariantFrequencies { get; }
    }
}