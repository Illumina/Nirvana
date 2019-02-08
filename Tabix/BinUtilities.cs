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
    }
}
