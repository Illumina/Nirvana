using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.CompressedSequence
{
    public interface ICompressedSequence
    {
        int NumBases { get; }
        void Set(int numBases, byte[] buffer, IntervalTree<MaskedEntry> maskedIntervalTree);
        string Substring(int offset, int length);
    }
}
