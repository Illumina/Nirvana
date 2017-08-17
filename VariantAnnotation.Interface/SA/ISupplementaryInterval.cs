using System.IO;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryInterval : IInterval
    {
        string KeyName { get; }
        string ReferenceName { get; }
        string JsonString { get; }
        ReportFor ReportingFor { get; }
        void Write(BinaryWriter writer);
        double? GetReciprocalOverlap(IInterval variant);
    }

    public enum ReportFor
    {
        None,
        AllVariants,
        SmallVariants,
        StructuralVariants
    }
}