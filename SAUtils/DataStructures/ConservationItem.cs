using System;
using Genome;

namespace SAUtils.DataStructures
{
    public sealed class PhylopItem
    {
        public Chromosome Chromosome { get; }
        public int Position { get; }
        public double Score { get; }

        public PhylopItem(Chromosome chromosome, int position, double score)
        {
            Chromosome = chromosome;
            Position   = position;
            Score      = Math.Round(score,1, MidpointRounding.AwayFromZero);
        }
        
    }
}