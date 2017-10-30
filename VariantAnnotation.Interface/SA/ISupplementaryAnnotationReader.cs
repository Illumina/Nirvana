using System.Collections.Generic;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryAnnotationReader
    {
        IEnumerable<Interval<ISupplementaryInterval>> SmallVariantIntervals { get; }
        IEnumerable<Interval<ISupplementaryInterval>> SvIntervals { get; }
        IEnumerable<Interval<ISupplementaryInterval>> AllVariantIntervals { get; }
        ISupplementaryAnnotationHeader Header { get; }
        IEnumerable<(int, string)> GlobalMajorAlleleInRefMinors { get; }
        ISaPosition GetAnnotation(int position);
    }
}