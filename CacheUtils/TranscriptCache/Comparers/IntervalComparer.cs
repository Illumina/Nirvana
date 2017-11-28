using System.Collections.Generic;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.TranscriptCache.Comparers
{
    internal sealed class IntervalComparer : EqualityComparer<IInterval>
    {
        public override bool Equals(IInterval x, IInterval y) => x.Start == y.Start && x.End == y.End;

        public override int GetHashCode(IInterval x)
        {
            unchecked
            {
                return (x.Start * 397) ^ x.End;
            }
        }
    }
}
