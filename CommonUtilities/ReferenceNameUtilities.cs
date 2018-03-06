using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Sequence;

namespace CommonUtilities
{
    public static class ReferenceNameUtilities
    {
        public static IChromosome GetChromosome(IDictionary<string, IChromosome> refNameToChromosome,
            string referenceName)
        {
            if (referenceName == null) return new EmptyChromosome(string.Empty);
            return !refNameToChromosome.TryGetValue(referenceName, out IChromosome chromosome)
                ? new EmptyChromosome(referenceName)
                : chromosome;
        }

        public static IChromosome GetChromosome(IDictionary<ushort, IChromosome> refIndexToChromosome, ushort referenceIndex)
        {
            if (!refIndexToChromosome.TryGetValue(referenceIndex, out IChromosome chromosome))
            {
                throw new InvalidDataException($"Unable to find the reference index ({referenceIndex}) in the refIndexToChromosome dictionary.");
            }

            return chromosome;
        }

        public static bool IsEmpty(this IChromosome chromosome) => chromosome.Index == ushort.MaxValue;
    }
}
