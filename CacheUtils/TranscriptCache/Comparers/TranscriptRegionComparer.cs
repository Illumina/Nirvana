using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache.Comparers
{
    internal sealed class TranscriptRegionComparer : EqualityComparer<ITranscriptRegion>
    {
        public override bool Equals(ITranscriptRegion x, ITranscriptRegion y)
        {
            if (ReferenceEquals(x, y)) return true;
            return x.Type == y.Type && x.Id == y.Id && x.Start == y.Start && x.End == y.End &&
                   x.CdnaStart == y.CdnaStart && x.CdnaEnd == y.CdnaEnd;
        }

        public override int GetHashCode(ITranscriptRegion x)
        {
            unchecked
            {
                var hashCode = (int)x.Type;
                hashCode = (hashCode * 397) ^ x.Id.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Start;
                hashCode = (hashCode * 397) ^ x.End;
                hashCode = (hashCode * 397) ^ x.CdnaStart;
                hashCode = (hashCode * 397) ^ x.CdnaEnd;
                return hashCode;
            }
        }
    }
}
