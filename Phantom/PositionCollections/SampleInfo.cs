namespace Phantom.PositionCollections
{
    public sealed class SampleInfo
    {
        public readonly string[,][] Values;
        public readonly int NumPositions;
        public readonly int NumSamples;

        public SampleInfo(string[,][] values)
        {
            Values       = values;
            NumPositions = values.GetLength(0);
            NumSamples   = values.GetLength(1);
        }
    }
}