using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.Interface.Intervals
{
    public interface IAnnotatedSupplementaryInterval
    {
        ISupplementaryInterval SupplementaryInterval { get; }
        double? ReciprocalOverlap { get; }
    }
}