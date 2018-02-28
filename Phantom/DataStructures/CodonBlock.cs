using Phantom.Interfaces;
using VariantAnnotation.Interface.Sequence;

namespace Phantom.DataStructures
{
    public class CodonBlock : ICodonBlock
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public int StartPhase { get; }
        public bool IsSpliced { get; }
        public int? MidPositionInSplicedCodon { get; }

        public CodonBlock(int start, int end, bool isSpliced, int? midPositionInSplicedCodon, IChromosome chromosome, int startPhase = 0)
        {
            Chromosome = chromosome;
            Start = start;
            End = end;
            StartPhase = startPhase;
            IsSpliced = isSpliced;
            MidPositionInSplicedCodon = midPositionInSplicedCodon;
        }
    }
}