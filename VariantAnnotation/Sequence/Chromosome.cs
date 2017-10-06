using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Sequence
{
    public sealed class Chromosome : IChromosome
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public ushort Index { get; }

        public override int GetHashCode()
        {
            return Index;
        }

        public bool Equals(Chromosome obj)
        {
            return Index == obj.Index;
        }

        public Chromosome(string ucscName, string ensemblName, ushort index)
        {
            UcscName    = ucscName;
            EnsemblName = ensemblName;
            Index       = index;
        }
    }
}