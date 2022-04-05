using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;

namespace ReferenceSequence.IO
{
    public static class AssemblyReader
    {
        private const int EnsemblIndex          = 0;
        private const int GenBankAccessionIndex = 4;
        private const int RefSeqAccessionIndex  = 6;
        private const int LengthIndex           = 8;
        private const int UcscIndex             = 9;

        public static List<Chromosome> GetChromosomes(Stream stream, Dictionary<string, Chromosome> oldRefNameToChromosome, int oldNumRefSeqs)
        {
            var nextRefIndex = (ushort)oldNumRefSeqs;
            var chromosomes  = new List<Chromosome>();

            using (var reader = new StreamReader(stream))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;

                    if (line.OptimizedStartsWith('#')) continue;

                    string[] cols = line.OptimizedSplit('\t');
                    string ensemblName      = cols[EnsemblIndex].Sanitize();
                    string genBankAccession = cols[GenBankAccessionIndex].Sanitize();
                    string refSeqAccession  = cols[RefSeqAccessionIndex].Sanitize();
                    int length              = int.Parse(cols[LengthIndex]);
                    string ucscName         = cols[UcscIndex].Sanitize();

                    ushort refIndex = GetRefIndex(oldRefNameToChromosome, ensemblName, ucscName, genBankAccession, refSeqAccession, ref nextRefIndex);
                    chromosomes.Add(new Chromosome(ucscName, ensemblName, refSeqAccession, genBankAccession, length, refIndex));
                }
            }

            return chromosomes.OrderBy(x => x.Index).ToList();
        }

        private static string Sanitize(this string s) => s == "na" ? null : s;

        private static ushort GetRefIndex(Dictionary<string, Chromosome> refNameToChromosome, string ensemblName, string ucscName, string genBankAccession, string refSeqAccession, ref ushort nextRefIndex)
        {
            if (!string.IsNullOrEmpty(ensemblName)      && refNameToChromosome.TryGetValue(ensemblName, out var chromosome))  return chromosome.Index;
            if (!string.IsNullOrEmpty(ucscName)         && refNameToChromosome.TryGetValue(ucscName, out chromosome))         return chromosome.Index;
            if (!string.IsNullOrEmpty(genBankAccession) && refNameToChromosome.TryGetValue(genBankAccession, out chromosome)) return chromosome.Index;
            if (!string.IsNullOrEmpty(refSeqAccession)  && refNameToChromosome.TryGetValue(refSeqAccession, out chromosome))  return chromosome.Index;

            ushort refIndex = nextRefIndex;
            nextRefIndex++;
            return refIndex;
        }
    }
}
