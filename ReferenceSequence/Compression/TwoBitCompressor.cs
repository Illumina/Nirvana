using System.Collections.Generic;
using Intervals;

namespace ReferenceSequence.Compression
{
    internal static class TwoBitCompressor
    {
        private static readonly byte[] ConvertBaseToNumber = new byte[256];
        private const string Bases = "GCTA";

        static TwoBitCompressor()
        {
            for (var index = 0; index < 256; ++index)
                ConvertBaseToNumber[index] = 10;

            for (var index = 0; index < Bases.Length; ++index)
            {
                ConvertBaseToNumber[Bases[index]] = (byte)index;
                ConvertBaseToNumber[char.ToLower(Bases[index])] = (byte)index;
            }
        }

        private static int GetNumBufferBytes(int numBases) => (int)(numBases / 4.0 + 1.0);

        public static (byte[] Buffer, Interval[] MaskedEntries) Compress(string bases)
        {
            int numBufferBases = GetNumBufferBytes(bases.Length);
            var buffer         = new byte[numBufferBases];

            byte num1  = 0;
            var index1 = 0;
            var num2   = 0;

            foreach (char index2 in bases)
            {
                byte num3 = ConvertBaseToNumber[index2];
                if (num3 == 10) num3 = 0;
                num1 = (byte)((uint)num1 << 2 | num3);
                ++num2;

                if (num2 != 4) continue;

                buffer[index1] = num1;
                num1 = 0;
                num2 = 0;
                ++index1;
            }

            if (num2 != 0) buffer[index1] = (byte)((uint)num1 << (4 - num2) * 2);

            var maskedEntries = new List<Interval>();

            for (var index2 = 0; index2 < bases.Length; ++index2)
            {
                if (bases[index2] != 'N') continue;

                int begin = index2;
                int end   = index2;

                for (++index2; index2 < bases.Length && bases[index2] == 'N'; ++index2) end = index2;

                maskedEntries.Add(new Interval(begin, end));
            }

            return (buffer, maskedEntries.ToArray());
        }
    }
}