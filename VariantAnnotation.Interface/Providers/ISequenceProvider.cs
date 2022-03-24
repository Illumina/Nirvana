using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface ISequenceProvider : IAnnotationProvider
    {
        ISequence Sequence { get; }
        IDictionary<string, Chromosome> RefNameToChromosome { get; }
        IDictionary<ushort, Chromosome> RefIndexToChromosome { get; }
        void LoadChromosome(Chromosome chromosome);
    }
}