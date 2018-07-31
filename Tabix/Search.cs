using Genome;

namespace Tabix
{
    public static class Search
    {
        /// <summary>
        /// This method returns the minimum offset for all variants that start from the begin position (Nirvana behavior)
        /// </summary>
        public static long GetOffset(this Index index, IChromosome chromosome, int begin)
        {
            if (chromosome.IsEmpty() || chromosome.Index >= index.ReferenceSequences.Length) return 0;

            // N.B. tabix assumes begin is 0-based and end is 1-based
            int end = begin;
            begin--;

            if (begin < 0) begin = 0;

            var refSeq = index.ReferenceSequences[chromosome.Index];
            if (begin == 0) return (long)refSeq.LinearFileOffsets[0];

            ulong minOffset = GetMinOffset(refSeq, begin);
            ulong maxOffset = GetMaxOffset(refSeq, end);

            int bin = BinUtilities.ConvertPositionToBin(begin);

            long minOverlapOffset = GetMinOverlapOffset(refSeq.IdToChunks[bin], minOffset, maxOffset);
            return minOverlapOffset;
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
    }
}
