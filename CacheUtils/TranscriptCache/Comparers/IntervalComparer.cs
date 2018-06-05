using System.Collections.Generic;
using Intervals;

namespace CacheUtils.TranscriptCache.Comparers
{
    internal sealed class IntervalComparer : EqualityComparer<IInterval>
    {
        public override bool Equals(IInterval x, IInterval y) => x.Start == y.Start && x.End == y.End;

        public override int GetHashCode(IInterval obj)
        {
            unchecked
            {
                return (obj.Start * 397) ^ obj.End;
            }
        }
    }
}
