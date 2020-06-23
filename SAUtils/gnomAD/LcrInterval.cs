using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.gnomAD
{
    public class LcrInterval:ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public string GetJsonString() => string.Empty;

        public LcrInterval(IChromosome chromosome, int start, int end)
        {
            Chromosome = chromosome;
            Start = start;
            End = end;
        }
    }
}