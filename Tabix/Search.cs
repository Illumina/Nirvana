using System.Collections.Generic;
using System.IO;
using Compression.FileHandling;
using Genome;

namespace Tabix
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Search
    {
        private readonly Index _index;
        private readonly Stream _vcfStream;

        public Search(Index index, Stream vcfStream)
        {
            _index     = index;
            _vcfStream = vcfStream;
        }

        // ReSharper disable once UnusedMember.Global
        public bool HasVariants(string chromosomeName, int begin, int end)
        {
            var refSeq = _index.GetTabixReferenceSequence(chromosomeName);
            if (refSeq == null) return false;

            int adjBegin = SearchUtilities.AdjustBegin(begin);

            IEnumerable<int> bins = BinUtilities.OverlappingBinsWithVariants(adjBegin, end, refSeq.IdToChunks);

            var block = new BgzfBlock();
            foreach (int bin in bins)
            {
                refSeq.IdToChunks.TryGetValue(bin, out Interval[] chunks);
                if (HasVariantsInBin(refSeq.Chromosome, begin, end, block, chunks)) return true;
            }

            return false;
        }

        private bool HasVariantsInBin(Chromosome chromosome, int begin, int end, BgzfBlock block, Interval[] intervals)
        {
            (long minVirtualOffset, long maxVirtualOffset) = SearchUtilities.GetMinMaxVirtualFileOffset(intervals);

            long minOffset = VirtualPosition.From(minVirtualOffset).FileOffset;
            long maxOffset = VirtualPosition.From(maxVirtualOffset).FileOffset;

            return BgzfBlockVcfReader.FindVariantsInBlocks(_vcfStream, minOffset, maxOffset, block, chromosome, begin, end);
        }
    }
}
