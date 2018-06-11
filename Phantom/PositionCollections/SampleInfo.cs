namespace Phantom.PositionCollections
{
    public class SampleInfo
    {
        public string[,][] Values;
        public int NumPositions;
        public int NumSamples;

        public SampleInfo(string[,][] values)
        {
            Values = values;
            NumPositions = values.GetLength(0);
            NumSamples = values.GetLength(1);
        }
    }
}