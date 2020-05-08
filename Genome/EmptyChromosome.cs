using IO;

namespace Genome
{
    public sealed class EmptyChromosome : IChromosome
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public string RefSeqAccession { get; }
        public string GenBankAccession { get; }
        public int Length { get; }
        public int FlankingLength => Chromosome.ShortFlankingLength;
        public ushort Index { get; }
        
        public EmptyChromosome(string chromosomeName)
        {
            UcscName         = chromosomeName;
            EnsemblName      = chromosomeName;
            RefSeqAccession  = chromosomeName;
            GenBankAccession = chromosomeName;
            Length           = 0;
            Index            = ushort.MaxValue;
        }
        
        public void Write(ExtendedBinaryWriter writer) => throw new System.NotImplementedException();

        public bool Equals(IChromosome other) => UcscName == other.UcscName && Index == other.Index;

        public override int GetHashCode() => UcscName.GetHashCode();
    }
}