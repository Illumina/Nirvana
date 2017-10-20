namespace VariantAnnotation.Interface.Positions
{
    public interface ISample
    {
        double? VariantFrequency { get; }
        int? TotalDepth { get; }
        int[] AlleleDepths { get; }
        string Genotype { get; }
        int? GenotypeQuality { get; }
        bool FailedFilter { get; }
        int? CopyNumber { get; }
        bool IsLossOfHeterozygosity { get; }
        float? DeNovoQuality { get; }
        bool IsEmpty { get; }
        int[] SplitReadCounts { get; }
        int[] PairEndReadCounts { get; }
		string RepeatNumbers { get; }
		string RepeatNumberSpans { get; }
    }
}