using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface ISequenceProvider : IAnnotationProvider
    {
        ISequence Sequence { get; }
        Dictionary<string, Chromosome> RefNameToChromosome { get; }
        Dictionary<ushort, Chromosome> RefIndexToChromosome { get; }
        void LoadChromosome(Chromosome chromosome);
    }
}