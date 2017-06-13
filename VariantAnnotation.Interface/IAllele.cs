namespace VariantAnnotation.Interface
{
    public interface IAllele
    {
        int Start { get; }
        int End { get; }
        VariantType NirvanaVariantType { get; }
        string VariantId { get; }
        string ReferenceAllele { get; }
        string AlternateAllele { get; }
        string SuppAltAllele { get; }
        int GenotypeIndex { get; }
        string ConservationScore { get; }
        bool IsStructuralVariant { get; }
	    bool IsRecomposedVariant { get; }
		bool IsRepeatExpansion { get; }
		int RefRepeatCount { get; }
		string RepeatUnit { get; }
    }
}
