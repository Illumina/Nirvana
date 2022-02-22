#nullable enable
using System.Collections.Generic;

namespace Genome;

public static class ReferenceNameUtilities
{
    public static Chromosome GetChromosome(Dictionary<string, Chromosome> refNameToChromosome,
        string? referenceName)
    {
        if (referenceName == null) return Chromosome.GetEmpty(string.Empty);
        return !refNameToChromosome.TryGetValue(referenceName, out Chromosome? chromosome)
            ? Chromosome.GetEmpty(referenceName)
            : chromosome;
    }
}