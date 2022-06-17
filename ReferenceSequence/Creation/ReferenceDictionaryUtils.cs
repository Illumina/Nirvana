using System.Collections.Generic;
using System.IO;
using Genome;

namespace ReferenceSequence.Creation
{
    internal static class ReferenceDictionaryUtils
    {
        internal static Dictionary<string, Chromosome> GetRefNameToChromosome(IEnumerable<Chromosome> chromosomes)
        {
            var refNameToChromosome = new Dictionary<string, Chromosome>();

            foreach (var chromosome in chromosomes)
            {
                bool isUcscEmpty             = string.IsNullOrEmpty(chromosome.UcscName);
                bool isEnsemblEmpty          = string.IsNullOrEmpty(chromosome.EnsemblName);
                bool isRefSeqAccessionEmpty  = string.IsNullOrEmpty(chromosome.RefSeqAccession);
                bool isGenBankAccessionEmpty = string.IsNullOrEmpty(chromosome.GenBankAccession);

                if (isUcscEmpty && isEnsemblEmpty && isRefSeqAccessionEmpty && isGenBankAccessionEmpty)
                    throw new InvalidDataException("Expected at least one chromosome field to be non-empty.");

                if (!isUcscEmpty)             refNameToChromosome[chromosome.UcscName]         = chromosome;
                if (!isEnsemblEmpty)          refNameToChromosome[chromosome.EnsemblName]      = chromosome;
                if (!isRefSeqAccessionEmpty)  refNameToChromosome[chromosome.RefSeqAccession]  = chromosome;
                if (!isGenBankAccessionEmpty) refNameToChromosome[chromosome.GenBankAccession] = chromosome;
            }

            return refNameToChromosome;
        }
    }
}
