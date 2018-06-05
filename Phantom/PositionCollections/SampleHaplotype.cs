namespace Phantom.PositionCollections
{
    public struct SampleHaplotype
    {
        public int SampleIndex { get; }
        public byte HaplotypeIndex { get; }

        public SampleHaplotype(int sampleIndex, byte haplotypeIndex)
        {
            SampleIndex = sampleIndex;
            HaplotypeIndex = haplotypeIndex;
        }
    }
}
