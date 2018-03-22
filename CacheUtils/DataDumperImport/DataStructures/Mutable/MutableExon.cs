using System;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.DataDumperImport.DataStructures.Mutable
{
    public sealed class MutableExon : IEquatable<MutableExon>, IInterval
    {
        private readonly IChromosome _chromosome;
        public readonly int Phase;
        public int Start { get; }
        public int End { get; }

        public MutableExon(IChromosome chromosome, int start, int end, int phase)
        {
            _chromosome = chromosome;
            Start       = start;
            End         = end;
            Phase       = phase;
        }

        public bool Equals(MutableExon other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _chromosome.Index == other._chromosome.Index && Start == other.Start && End == other.End &&
                   Phase == other.Phase;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _chromosome.Index.GetHashCode();
                hashCode = (hashCode * 397) ^ Start;
                hashCode = (hashCode * 397) ^ End;
                hashCode = (hashCode * 397) ^ Phase.GetHashCode();
                return hashCode;
            }
        }
    }
}
