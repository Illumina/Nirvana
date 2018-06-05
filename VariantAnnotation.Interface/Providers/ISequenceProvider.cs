using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface ISequenceProvider : IAnnotationProvider
    {
        ISequence Sequence { get; }
        IDictionary<string, IChromosome> RefNameToChromosome { get; }
	    IDictionary<ushort, IChromosome> RefIndexToChromosome { get; }
        void LoadChromosome(IChromosome chromosome);
    }
}