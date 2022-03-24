using Genome;

namespace Phantom.PositionCollections
{
    public struct AlleleSet
    {
        public Chromosome Chromosome { get; }
        public int[] Starts { get; }
        public string[][] VariantArrays { get; }

        public AlleleSet(Chromosome chromosome, int[] starts, string[][] variantArrays)
        {
            Chromosome = chromosome;
            Starts = starts;
            VariantArrays = variantArrays;
        }
    }
}