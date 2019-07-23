namespace Genome
{
    public sealed class GenomicRange
    {
        public GenomicPosition Start { get; }
        public GenomicPosition? End { get; }

        public GenomicRange(GenomicPosition start, GenomicPosition? end)
        {
            Start = start;
            End = end;
        }
    }
}