using System;
using Genome;

namespace SAUtils.DataStructures
{
    public sealed class PhylopItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; }
        public double Score { get; }

        public PhylopItem(IChromosome chromosome, int position, double score)
        {
            Chromosome = chromosome;
            Position   = position;
            Score      = Math.Round(score,1, MidpointRounding.AwayFromZero);
        }
        
    }
}