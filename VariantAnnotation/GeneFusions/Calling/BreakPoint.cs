using Genome;

namespace VariantAnnotation.GeneFusions.Calling
{
    public sealed record BreakPoint(IChromosome Chromosome, int Position, bool OnReverseStrand);
}