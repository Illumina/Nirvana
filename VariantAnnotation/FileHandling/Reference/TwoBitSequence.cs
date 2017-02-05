using System.Collections.Generic;

namespace VariantAnnotation.FileHandling.Reference
{
    internal class TwoBitSequence
    {
        public byte[] Buffer;
        public int NumBufferBytes;
        public readonly List<MaskedEntry> MaskedIntervals;

        public TwoBitSequence()
        {
            MaskedIntervals = new List<MaskedEntry>();
        }

        private static int GetNumBufferBytes(int numBases)
        {
            return (int)(numBases / 4.0 + 1.0);
        }

        public void Allocate(int numBases)
        {
            NumBufferBytes = GetNumBufferBytes(numBases);
            if (Buffer == null || Buffer.Length < NumBufferBytes)
                Buffer = new byte[NumBufferBytes];
            MaskedIntervals.Clear();
        }
    }
}
