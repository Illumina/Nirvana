namespace Genome
{
    public sealed record ChromosomeInterval(Chromosome Chromosome, int Start, int End) : IChromosomeInterval;
}