using Genome;

namespace VariantAnnotation.GeneFusions.Calling
{
    public sealed record BreakPoint(Chromosome Chromosome, int Position, bool OnReverseStrand);
}