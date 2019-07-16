namespace Tabix
{
    public static class SearchUtilities
    {
        public static long GetOffset(this Index index, string chromosomeName, int begin)
        {
            var refSeq = index.GetTabixReferenceSequence(chromosomeName);
            if (refSeq == null) return -1;

            // N.B. tabix assumes begin is 0-based and end is 1-based
            int end = begin;
            begin = AdjustBegin(begin);

            if (begin == 0) return refSeq.LinearFileOffsets.FirstNonZeroValue();

            ulong minOffset = GetMinOffset(refSeq, begin);
            ulong maxOffset = GetMaxOffset(refSeq, end);

            int bin = BinUtilities.ConvertPositionToBin(begin);

            if (refSeq.IdToChunks.TryGetValue(bin, out var chunks))
                return GetMinOverlapOffset(chunks, minOffset, maxOffset);

            int linearIndex = begin >> Constants.MinShift;
            if (linearIndex >= refSeq.LinearFileOffsets.Length) return -1;

            return (long)refSeq.LinearFileOffsets[linearIndex];
        }

        internal static int AdjustBegin(int begin)
        {
            // N.B. tabix assumes begin is 0-based and end is 1-based
            begin--;
            if (begin < 0) begin = 0;
            return begin;
        }

        internal static long FirstNonZeroValue(this ulong[] offsets)
        {
            foreach (ulong offset in offsets)
            {
                if (offset == 0) continue;
                return (long)offset;
            }

            return -1;
        }

        internal static ReferenceSequence GetTabixReferenceSequence(this Index index, string chromosomeName)
        {
            if (string.IsNullOrEmpty(chromosomeName)) return null;
            return !index.RefNameToTabixIndex.TryGetValue(chromosomeName, out ushort tabixIndex)
                ? null
                : index.ReferenceSequences[tabixIndex];
        }

        internal static long GetMinOverlapOffset(Interval[] chunks, ulong minOffset, ulong maxOffset)
        {
            if (chunks == null) return 0;

            ulong minOverlapOffset = ulong.MaxValue;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var chunk in chunks)
            {
                if (chunk.End > minOffset && chunk.Begin < maxOffset && chunk.Begin < minOverlapOffset)
                    minOverlapOffset = chunk.Begin;
            }

            return (long)minOverlapOffset;
        }

        internal static ulong GetMinOffset(ReferenceSequence refSeq, int begin)
        {
            int bin = BinUtilities.FirstBin(Constants.NumLevels) + (begin >> Constants.MinShift);

            do
            {
                if (refSeq.IdToChunks.ContainsKey(bin)) break;

                int firstBin = (BinUtilities.ParentBin(bin) << 3) + 1;

                if (bin > firstBin) bin--;
                else bin = BinUtilities.ParentBin(bin);

            } while (bin != 0);

            int bottomBin = BinUtilities.BottomBin(bin);

            return refSeq.LinearFileOffsets[bottomBin];
        }

        internal static ulong GetMaxOffset(ReferenceSequence refSeq, int end)
        {
            int bin = BinUtilities.FirstBin(Constants.NumLevels) + ((end - 1) >> Constants.MinShift) + 1;

            while (true)
            {
                while (bin % 8 == 1) bin = BinUtilities.ParentBin(bin);

                if (bin == 0) return ulong.MaxValue;
                if (refSeq.IdToChunks.TryGetValue(bin, out var chunks) && chunks.Length > 0) return chunks[0].Begin;

                bin++;
            }
        }

        internal static (long MinOffset, long MaxOffset) GetMinMaxVirtualFileOffset(Interval[] intervals)
        {
            int numIntervals = intervals.Length;

            var minBegin = (long)intervals[0].Begin;
            var minEnd   = (long)intervals[0].End;

            for (var i = 1; i < numIntervals; i++)
            {
                var interval = intervals[i];
                var begin    = (long)interval.Begin;
                var end      = (long)interval.End;

                if (begin < minBegin) minBegin = begin;
                if (end   > minEnd)   minEnd   = end;
            }

            return (minBegin, minEnd);
        }
    }
}
