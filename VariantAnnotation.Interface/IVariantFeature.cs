using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IVariantFeature
    {
        ushort ReferenceIndex { get; }
        bool IsStructuralVariant { get; }
        IAllele FirstAlternateAllele { get; }
        int OverlapReferenceBegin { get; }
        int OverlapReferenceEnd { get; }
        void AddSupplementaryIntervals(List<ISupplementaryInterval> overlappingSupplementaryIntervals);
        void AddCustomAnnotation(List<ISupplementaryAnnotationReader> saReaders);
        void AddCustomIntervals(List<ICustomInterval> intervals);
        void SetSupplementaryAnnotation(ISupplementaryAnnotationReader saReader);
    }
}
