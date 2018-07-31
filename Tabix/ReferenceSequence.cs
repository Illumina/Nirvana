using System.Collections.Generic;

namespace Tabix
{
    public sealed class ReferenceSequence
    {
        public readonly string Chromosome;
        public readonly Dictionary<int, Interval[]> IdToChunks;

        // for each 16 kbp interval
        public readonly ulong[] LinearFileOffsets;

        public ReferenceSequence(string chromosome, Dictionary<int, Interval[]> idToChunks, ulong[] linearFileOffsets)
        {
            Chromosome        = chromosome;
            IdToChunks        = idToChunks;
            LinearFileOffsets = linearFileOffsets;
        }
    }
}
