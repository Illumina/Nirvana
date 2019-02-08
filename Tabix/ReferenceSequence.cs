using System.Collections.Generic;
using Genome;

namespace Tabix
{
    public sealed class ReferenceSequence
    {
        public readonly IChromosome Chromosome;
        public readonly Dictionary<int, Interval[]> IdToChunks;

        // for each 16 kbp interval
        public readonly ulong[] LinearFileOffsets;

        public ReferenceSequence(IChromosome chromosome, Dictionary<int, Interval[]> idToChunks, ulong[] linearFileOffsets)
        {
            Chromosome        = chromosome;
            IdToChunks        = idToChunks;
            LinearFileOffsets = linearFileOffsets;
        }
    }
}
