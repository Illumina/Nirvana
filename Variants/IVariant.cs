namespace Variants
{
    public interface IVariant : ISimpleVariant
    {
        string VariantId { get; }
        bool IsRefMinor { get; }
        bool IsRecomposed { get; }
        bool IsDecomposed { get; }
        string[] LinkedVids { get; }
        AnnotationBehavior Behavior { get; }
        bool IsStructuralVariant { get; }
    }
}