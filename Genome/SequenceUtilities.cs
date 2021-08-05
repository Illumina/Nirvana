using System;
using System.Buffers;

namespace Genome
{
    public static class SequenceUtilities
    {
        private static readonly char[] ReverseComplementLookupTable;

        static SequenceUtilities()
        {
            // initialize the reverse complement code
            const string forwardBases = "ABCDGHKMRTVYabcdghkmrtvy";
            const string reverseBases = "TVGHCDMKYABRTVGHCDMKYABR";
            ReverseComplementLookupTable = new char[256];

            for (var i = 0; i < 256; i++) ReverseComplementLookupTable[i] = 'N';
            for (var i = 0; i < forwardBases.Length; i++)
            {
                ReverseComplementLookupTable[forwardBases[i]] = reverseBases[i];
            }
        }

        public static unsafe string GetReverseComplement(string bases)
        {
            if (bases == null) return null;
            if (bases == string.Empty) return string.Empty;

            ArrayPool<char> charPool = ArrayPool<char>.Shared;
            int             numBases = bases.Length;

            char[] reverseChars = charPool.Rent(numBases);

            fixed (char* pBases = bases)
            fixed (char* pReverseChars = reverseChars)
            {
                char* pIn  = pBases;
                char* pOut = pReverseChars + numBases - 1;

                for (var i = 0; i < numBases; i++)
                {
                    *pOut = ReverseComplementLookupTable[*pIn];
                    pOut--;
                    pIn++;
                }
            }

            var reverseString = new string(reverseChars, 0, numBases);
            charPool.Return(reverseChars);

            return reverseString;
        }

        public static bool HasNonCanonicalBase(string bases)
        {
            if (bases == null) return false;
            ReadOnlySpan<char> baseSpan = bases.AsSpan();

            foreach (char b in baseSpan)
            {
                if (b == 'A' || b == 'C' || b == 'G' || b == 'T' || b == '-') continue;
                return true;
            }

            return false;
        }
    }
}