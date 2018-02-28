namespace VariantAnnotation.Interface.Positions
{
    public interface IVariant : ISimpleVariant
    {
        string VariantId { get; }
        bool IsRefMinor { get; }
        bool IsRecomposed { get; }
        bool IsDecomposed { get; }
        string[] LinkedVids { get; }
		IBreakEnd[] BreakEnds { get; }
        AnnotationBehavior Behavior { get; }
    }
}