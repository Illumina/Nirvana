using System;

namespace Genome
{
    public sealed class Chromosome : IChromosome, IComparable<IChromosome>
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public string RefSeqAccession { get; }
        public string GenBankAccession { get; }
        public int Length { get; }
        public ushort Index { get; }

        public const ushort UnknownReferenceIndex = ushort.MaxValue;

        public Chromosome(string ucscName, string ensemblName, string refSeqAccession, string genBankAccession,
            int length, ushort index)
        {
            UcscName         = ucscName;
            EnsemblName      = ensemblName;
            RefSeqAccession  = refSeqAccession;
            GenBankAccession = genBankAccession;
            Length           = length;
            Index            = index;
        }

        public bool Equals(IChromosome other) => Index == other.Index && Length == other.Length;

        public int CompareTo(IChromosome other) => Index == other.Index ? Length.CompareTo(other.Length) : Index.CompareTo(other.Index);
    }
}