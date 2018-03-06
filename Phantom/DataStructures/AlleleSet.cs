using VariantAnnotation.Interface.Sequence;

namespace Phantom.DataStructures
{
    public struct AlleleSet
    {
        public IChromosome Chromosome { get; }
        public int[] Starts { get; }
        public string[][] VariantArrays { get; }

        public AlleleSet(IChromosome chromosome, int[] starts, string[][] variantArrays)
        {
            Chromosome = chromosome;
            Starts = starts;
            VariantArrays = variantArrays;
        }
    }
}