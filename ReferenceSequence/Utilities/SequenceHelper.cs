using System.Collections.Generic;
using Genome;
using IO;
using ReferenceSequence.IO;

namespace ReferenceSequence.Utilities
{
    public static class SequenceHelper
    {
        public static (IDictionary<ushort, Chromosome> refIndexToChromosome, IDictionary<string, Chromosome>
            refNameToChromosome, int numRefSeqs) GetDictionaries(string referencePath)
        {
            IDictionary<ushort, Chromosome> refIndexToChromosome;
            IDictionary<string, Chromosome> refNameToChromosome;
            int numRefSeqs;

            using (var reader = new CompressedSequenceReader(PersistentStreamUtils.GetReadStream(referencePath)))
            {
                refIndexToChromosome = reader.RefIndexToChromosome;
                refNameToChromosome  = reader.RefNameToChromosome;
                numRefSeqs           = reader.NumRefSeqs;
            }

            return (refIndexToChromosome, refNameToChromosome, numRefSeqs);
        }
    }
}
