using System.Collections.Generic;
using Genome;

namespace Tabix
{
    public sealed class ReferenceIndex
    {
        public readonly Chromosome Chromosome;
        public readonly Dictionary<int, Interval[]> IdToChunks;

        // for each 16 kbp interval
        public readonly ulong[] LinearFileOffsets;

        public ReferenceIndex(Chromosome chromosome, Dictionary<int, Interval[]> idToChunks, ulong[] linearFileOffsets)
        {
            Chromosome        = chromosome;
            IdToChunks        = idToChunks;
            LinearFileOffsets = linearFileOffsets;
        }
    }
}
