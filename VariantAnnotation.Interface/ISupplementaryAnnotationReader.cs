using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ISupplementaryAnnotationReader
    {
        IEnumerable<Interval<IInterimInterval>> SmallVariantIntervals { get; }
        IEnumerable<Interval<IInterimInterval>> SvIntervals { get; }
        IEnumerable<Interval<IInterimInterval>> AllVariantIntervals { get; }
        ISupplementaryAnnotationHeader Header { get; }
        bool IsRefMinor(int position);
        ISaPosition GetAnnotation(int position);
    }
}
