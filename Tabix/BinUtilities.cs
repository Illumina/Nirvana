using System.Collections.Generic;

namespace Tabix
{
    internal static class BinUtilities
    {
        internal static int FirstBin(int bin) => ((1 << ((bin << 1) + bin)) - 1) / 7;
        internal static int ParentBin(int bin) => (bin - 1) >> 3;

        internal static int BottomBin(int bin)
        {
            var level = 0;
            for (int b = bin; b != 0; b = ParentBin(b)) level++;
            return (bin - FirstBin(level)) << (Constants.NumLevels - level) * 3;
        }

        /// <summary>
        /// assumes begin is 0-based
        /// </summary>
        internal static int ConvertPositionToBin(int begin) => 4681 + (begin >> Constants.MinShift);

        internal static IEnumerable<int> OverlappingBinsWithVariants(int begin, int end, Dictionary<int, Interval[]> idToChunks)
        {
            var overlappingBins = new List<int>();
            if (begin >= end) return overlappingBins;

            int shift = Constants.InitialShift;
            if (end >= Constants.MaxReferenceLength) end = Constants.MaxReferenceLength;

            var level = 0;
            var levelStartBin = 0;

            for (--end; level <= Constants.NumLevels; shift -= 3, levelStartBin += 1 << ((level << 1) + level), level++)
            {
                int beginBin = levelStartBin + (begin >> shift);
                int endBin   = levelStartBin + (end >> shift);

                for (int bin = beginBin; bin <= endBin; bin++)
                {
                    if (idToChunks.ContainsKey(bin)) overlappingBins.Add(bin);
                }
            }

            return overlappingBins;
        }
    }
}
