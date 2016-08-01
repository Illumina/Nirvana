using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;

namespace CreateCompressedReference
{
    public class TwoBitSequence
    {
        public byte[] Buffer;
        public int NumBufferBytes;
        public readonly IntervalTree<MaskedEntry> MaskedIntervalTree;

        public TwoBitSequence()
        {
            MaskedIntervalTree = new IntervalTree<MaskedEntry>();
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
            MaskedIntervalTree.Clear();
        }
    }
}
