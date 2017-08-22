using System.Collections.Generic;

namespace VariantAnnotation.Interface.Sequence
{
    public interface ISequenceReader
    {
        Dictionary<string, IChromosome> GetRefernceDictionary();
        ISequence GetSequence();
        void LoadChromosome(IChromosome chromosome);
    }
}