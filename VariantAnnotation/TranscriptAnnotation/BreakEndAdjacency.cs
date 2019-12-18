using Genome;

namespace VariantAnnotation.TranscriptAnnotation
{
    public sealed class BreakEndAdjacency
    {
        public readonly BreakPoint Origin;
        public readonly BreakPoint Partner;

        public BreakEndAdjacency(BreakPoint origin, BreakPoint partner)
        {
            Origin  = origin;
            Partner = partner;
        }
    }

    public sealed class BreakPoint
    {
        public readonly IChromosome Chromosome;
        public readonly int Position;
        public readonly bool OnReverseStrand;

        public BreakPoint(IChromosome chromosome, int position, bool onReverseStrand)
        {
            Chromosome      = chromosome;
            Position        = position;
            OnReverseStrand = onReverseStrand;
        }
    }
}
