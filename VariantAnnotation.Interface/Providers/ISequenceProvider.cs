using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface ISequenceProvider : IProvider
    {
        ISequence                      Sequence            { get; }
        Dictionary<string, Chromosome> RefNameToChromosome { get; }
        Chromosome[]                   Chromosomes         { get; }
        void LoadChromosome(Chromosome chromosome);
    }
}