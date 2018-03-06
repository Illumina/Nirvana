using System;

namespace VariantAnnotation.Interface.Sequence
{
    public interface IChromosome : IEquatable<IChromosome>
    {
        string UcscName { get; }
        string EnsemblName { get; }
        ushort Index { get; }
    }

    public sealed class EmptyChromosome : IChromosome
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public ushort Index { get; }

        public EmptyChromosome(string chromosomeName)
        {
            UcscName    = chromosomeName;
            EnsemblName = chromosomeName;
            Index       = ushort.MaxValue;
        }

        public bool Equals(IChromosome other) => UcscName == other.UcscName;

        public override int GetHashCode() => UcscName.GetHashCode();
    }
}