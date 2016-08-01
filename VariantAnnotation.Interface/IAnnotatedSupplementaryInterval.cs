namespace VariantAnnotation.Interface
{
    public interface IAnnotatedSupplementaryInterval
    {
        ISupplementaryInterval SupplementaryInterval { get; }
        double? ReciprocalOverlap { get; }
    }
}