using System;
using IO;

namespace Genome
{
    public interface IChromosome : IEquatable<IChromosome>
    {
        string UcscName { get; }
        string EnsemblName { get; }
        string RefSeqAccession { get; }
        string GenBankAccession { get; }
        int Length { get; }
        int FlankingLength { get; }
        ushort Index { get; }
        void Write(ExtendedBinaryWriter writer);
    }
}