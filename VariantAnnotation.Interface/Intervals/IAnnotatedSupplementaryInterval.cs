using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.Interface.Intervals
{
    public interface IAnnotatedSupplementaryInterval
    {
        ISuppIntervalItem SupplementaryInterval { get; }
        double? ReciprocalOverlap { get; }
    }
}