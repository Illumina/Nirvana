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

        public override int GetHashCode(ITranscriptRegion obj)
        {
            unchecked
            {
                var hashCode = (int)obj.Type;
                hashCode = (hashCode * 397) ^ obj.Id.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Start;
                hashCode = (hashCode * 397) ^ obj.End;
                hashCode = (hashCode * 397) ^ obj.CdnaStart;
                hashCode = (hashCode * 397) ^ obj.CdnaEnd;
                return hashCode;
            }
        }
    }
}
