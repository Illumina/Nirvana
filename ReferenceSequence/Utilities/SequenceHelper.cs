using System.Collections.Generic;
using Genome;
using IO;
using ReferenceSequence.IO;

namespace ReferenceSequence.Utilities
{
    public static class SequenceHelper
    {
        public static (IDictionary<ushort, IChromosome> refIndexToChromosome, IDictionary<string, IChromosome>
            refNameToChromosome, int numRefSeqs) GetDictionaries(string referencePath)
        {
            IDictionary<ushort, IChromosome> refIndexToChromosome;
            IDictionary<string, IChromosome> refNameToChromosome;
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
