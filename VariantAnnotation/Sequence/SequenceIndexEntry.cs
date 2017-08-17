using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Sequence
{
    public sealed class SequenceIndexEntry
    {
        public int NumBases;
        public long FileOffset;
        public int SequenceOffset;
        public IIntervalSearch<MaskedEntry> MaskedEntries = new NullIntervalSearch<MaskedEntry>();
    }
}