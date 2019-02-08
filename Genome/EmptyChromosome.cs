namespace Genome
{
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

        public bool Equals(IChromosome other) => UcscName == other.UcscName && Index == other.Index;

        public override int GetHashCode() => UcscName.GetHashCode();
    }
}