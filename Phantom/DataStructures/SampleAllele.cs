namespace Phantom.DataStructures
{
    public struct SampleAllele
    {
        public int SampleIndex { get; }
        public byte AlleleIndex { get; }

        public SampleAllele(int sampleIndex, byte alleleIndex)
        {
            SampleIndex = sampleIndex;
            AlleleIndex = alleleIndex;
        }
    }
}
