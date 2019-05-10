using System;

namespace Genome
{
    public sealed class Chromosome : IChromosome, IComparable<IChromosome>
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public ushort Index { get; }

        public const ushort UnknownReferenceIndex = ushort.MaxValue;

        public Chromosome(string ucscName, string ensemblName, ushort index)
        {
            UcscName    = ucscName;
            EnsemblName = ensemblName;
            Index       = index;
        }

        public bool Equals(IChromosome other) => Index == other.Index;

        public override int GetHashCode() => Index.GetHashCode();

        public int CompareTo(IChromosome other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : Index.CompareTo(other.Index);
        }
    }
}