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

        /// <summary>
        /// assumes begin is 0-based, and end is 1-based
        /// </summary>
        internal static int[] ConvertRegionToBinList(int begin, int end)
        {
            if (begin >= end) return null;

            int numSpanBits = Constants.MinShift + (Constants.NumLevels << 1) + Constants.NumLevels;
            int spanLength = 1 << numSpanBits;

            if (end >= spanLength) end = spanLength;

            var bins = new List<int>();
            var firstBinOnLevel = 0;
            --end;

            for (var level = 0; level <= Constants.NumLevels; level++)
            {
                int binBegin = firstBinOnLevel + (begin >> numSpanBits);
                int binEnd = firstBinOnLevel + (end >> numSpanBits);

                for (int bin = binBegin; bin <= binEnd; bin++) bins.Add(bin);

                numSpanBits -= 3;
                firstBinOnLevel += 1 << ((level << 1) + level);
            }

            return bins.ToArray();
        }
    }
}
