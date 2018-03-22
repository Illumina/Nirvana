using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Sequence
{
    public sealed class Chromosome : IChromosome
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public ushort Index { get; }

        public Chromosome(string ucscName, string ensemblName, ushort index)
        {
            UcscName    = ucscName;
            EnsemblName = ensemblName;
            Index       = index;
        }

        public bool Equals(IChromosome other) => Index == other.Index;

        public override int GetHashCode() => Index.GetHashCode();
    }
}