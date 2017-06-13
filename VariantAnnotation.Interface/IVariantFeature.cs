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
        void AddSupplementaryIntervals(List<IInterimInterval> overlappingSupplementaryIntervals);

		void SetSupplementaryAnnotation(ISupplementaryAnnotationReader saReader);

        string ReferenceName { get; }
        bool IsRefMinor { get; }
        bool IsReference { get; }
        bool IsRefNoCall { get; }
		bool IsRepeatExpansion { get; }
    }
}
