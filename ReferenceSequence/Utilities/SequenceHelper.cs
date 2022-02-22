using System.Collections.Generic;
using Genome;
using IO;
using ReferenceSequence.IO;

namespace ReferenceSequence.Utilities
{
    public static class SequenceHelper
    {
        public static Dictionary<string, Chromosome> GetRefNameToChromosome(string referencePath)
        {
            using var reader = new CompressedSequenceReader(PersistentStreamUtils.GetReadStream(referencePath));

            Dictionary<string, Chromosome> refNameToChromosome = reader.RefNameToChromosome;
            return refNameToChromosome;
        }
    }
}
