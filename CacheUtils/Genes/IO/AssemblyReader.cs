using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Genes.IO
{
    public static class AssemblyReader
    {
        private const int AccessionIndex = 6;
        private const int UcscIndex      = 9;

        public static IDictionary<string, IChromosome> GetAccessionToChromosome(StreamReader reader,
            IDictionary<string, IChromosome> refNameToChromosome)
        {
            var accessionToChromosome = new Dictionary<string, IChromosome>();

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                if (line.OptimizedStartsWith('#')) continue;

                var cols         = line.OptimizedSplit('\t');
                string accession = cols[AccessionIndex];
                string ucscName  = cols[UcscIndex];

                if (!refNameToChromosome.TryGetValue(ucscName, out IChromosome chromosome)) continue;
                accessionToChromosome[accession] = chromosome;
            }

            return accessionToChromosome;
        }
    }
}
