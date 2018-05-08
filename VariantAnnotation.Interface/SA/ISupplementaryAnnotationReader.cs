using System;
using System.Collections.Generic;
using Intervals;

namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryAnnotationReader : IDisposable
    {
        IEnumerable<Interval<ISupplementaryInterval>> SmallVariantIntervals { get; }
        IEnumerable<Interval<ISupplementaryInterval>> SvIntervals { get; }
        IEnumerable<Interval<ISupplementaryInterval>> AllVariantIntervals { get; }
        ISupplementaryAnnotationHeader Header { get; }
        IEnumerable<(int, string)> GlobalMajorAlleleInRefMinors { get; }
        ISaPosition GetAnnotation(int position);
    }
}