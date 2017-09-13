using System.Collections.Generic;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Providers
{
    public interface ISequenceProvider : IAnnotationProvider
    {
        ushort NumRefSeqs { get; }
        ISequence Sequence { get; }
        IDictionary<string, IChromosome> GetChromosomeDictionary();
	    IDictionary<ushort, IChromosome> GetChromosomeIndexDictionary();
        void LoadChromosome(IChromosome chromosome);

    }
}