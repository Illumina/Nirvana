using System.Collections.Generic;
using System.IO;
using Compression.FileHandling;
using Genome;

namespace Tabix
{
    public sealed class Search
    {
        private readonly Index _index;
        private readonly Stream _vcfStream;

        public Search(Index index, Stream vcfStream)
        {
            _index     = index;
            _vcfStream = vcfStream;
        }

        public bool HasVariants(string chromosomeName, int begin, int end)
        {
            var refSeq = _index.GetTabixReferenceSequence(chromosomeName);
            if (refSeq == null) return false;

            int adjBegin = SearchUtilities.AdjustBegin(begin);

            int beginBin = BinUtilities.ConvertPositionToBin(adjBegin);
            int endBin   = BinUtilities.ConvertPositionToBin(end);

            int binDiff = endBin - beginBin;

            // we can use the tabix index to investigate if any of the internal bins have variants
            if (binDiff >= 2)
            {
                bool hasInternalVariants = HasVariantsInAnyBins(refSeq.IdToChunks, beginBin + 1, endBin - 1);
                if (hasInternalVariants) return true;
            }

            // finally we have to check the remaining (edge) bins
            var block = new BgzfBlock();

            refSeq.IdToChunks.TryGetValue(beginBin, out var beginChunks);
            refSeq.IdToChunks.TryGetValue(endBin, out var endChunks);

            return HasVariantsInBin(refSeq.Chromosome, begin, end, block, beginChunks) || 
                   HasVariantsInBin(refSeq.Chromosome, begin, end, block, endChunks);
        }

        internal static bool HasVariantsInAnyBins(Dictionary<int, Interval[]> idToChunks, int beginBin,
            int endBin)
        {
            for (int bin = beginBin; bin <= endBin; bin++) if (idToChunks.ContainsKey(bin)) return true;
            return false;
        }

        private bool HasVariantsInBin(IChromosome chromosome, int begin, int end, BgzfBlock block, Interval[] intervals)
        {
            if (intervals == null) return false;
            (long minVirtualOffset, long maxVirtualOffset) = SearchUtilities.GetMinMaxVirtualFileOffset(intervals);

            long minOffset = VirtualPosition.From(minVirtualOffset).FileOffset;
            long maxOffset = VirtualPosition.From(maxVirtualOffset).FileOffset;

            return BgzfBlockVcfReader.FindVariantsInBlocks(_vcfStream, minOffset, maxOffset, block, chromosome, begin, end);
        }
    }
}
