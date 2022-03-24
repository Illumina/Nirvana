using System.Collections.Generic;
using System.IO;

namespace Genome
{
    public static class ReferenceNameUtilities
    {
        public static Chromosome GetChromosome(IDictionary<string, Chromosome> refNameToChromosome,
            string referenceName)
        {
            if (referenceName == null) return Chromosome.GetEmptyChromosome(string.Empty);

            return !refNameToChromosome.TryGetValue(referenceName, out Chromosome chromosome)
                ? Chromosome.GetEmptyChromosome(referenceName)
                : chromosome;
        }

        public static Chromosome GetChromosome(IDictionary<ushort, Chromosome> refIndexToChromosome, ushort referenceIndex)
        {
            if (!refIndexToChromosome.TryGetValue(referenceIndex, out Chromosome chromosome))
            {
                throw new InvalidDataException($"Unable to find the reference index ({referenceIndex}) in the refIndexToChromosome dictionary.");
            }

            return chromosome;
        }

        public static bool IsEmpty(this Chromosome chromosome) => chromosome.Index == ushort.MaxValue;
    }
}
