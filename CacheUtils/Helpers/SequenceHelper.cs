using System.Collections.Generic;
using Genome;
using IO;
using VariantAnnotation.Sequence;

namespace CacheUtils.Helpers
{
    public static class SequenceHelper
    {
        private static CompressedSequenceReader GetSequenceReader(string referencePath) =>
            new CompressedSequenceReader(StreamSourceUtils.GetStream(referencePath));

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
