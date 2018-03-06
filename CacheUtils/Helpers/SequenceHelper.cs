using System.Collections.Generic;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using VariantAnnotation.Utilities;

namespace CacheUtils.Helpers
{
    public static class SequenceHelper
    {
        private static CompressedSequenceReader GetSequenceReader(string referencePath) =>
            new CompressedSequenceReader(FileUtilities.GetReadStream(referencePath));

        public static (IDictionary<ushort, IChromosome> refIndexToChromosome, IDictionary<string, IChromosome>
            refNameToChromosome, int numRefSeqs) GetDictionaries(string referencePath)
        {
            IDictionary<ushort, IChromosome> refIndexToChromosome;
            IDictionary<string, IChromosome> refNameToChromosome;
            int numRefSeqs;

            using (var reader = GetSequenceReader(referencePath))
            {
                refIndexToChromosome = reader.RefIndexToChromosome;
                refNameToChromosome  = reader.RefNameToChromosome;
                numRefSeqs           = reader.NumRefSeqs;
            }

            return (refIndexToChromosome, refNameToChromosome, numRefSeqs);
        }
    }
}
