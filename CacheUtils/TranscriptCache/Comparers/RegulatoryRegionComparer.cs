using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache.Comparers
{
    internal sealed class RegulatoryRegionComparer : EqualityComparer<IRegulatoryRegion>
    {
        public override bool Equals(IRegulatoryRegion x, IRegulatoryRegion y)
        {
            return x.Start             == y.Start             &&
                   x.End               == y.End               &&
                   x.Chromosome.Index  == y.Chromosome.Index  &&
                   x.Id.WithoutVersion == y.Id.WithoutVersion &&
                   x.Type              == y.Type;
        }

        public override int GetHashCode(IRegulatoryRegion obj)
        {
            unchecked
            {
                int hashCode = obj.Start;
                hashCode = (hashCode * 397) ^ obj.End;
                hashCode = (hashCode * 397) ^ obj.Chromosome.Index.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Id.WithoutVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)obj.Type;
                return hashCode;
            }
        }
    }
}
