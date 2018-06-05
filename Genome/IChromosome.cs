using System;

namespace Genome
{
    public interface IChromosome : IEquatable<IChromosome>
    {
        string UcscName { get; }
        string EnsemblName { get; }
        ushort Index { get; }
    }
}