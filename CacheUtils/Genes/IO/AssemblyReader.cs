using System.Collections.Generic;
using System.IO;
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
                var line = reader.ReadLine();
                if (line == null) break;

                if (line.StartsWith("#")) continue;

                var cols      = line.Split('\t');
                var accession = cols[AccessionIndex];
                var ucscName  = cols[UcscIndex];

                if (!refNameToChromosome.TryGetValue(ucscName, out IChromosome chromosome)) continue;
                accessionToChromosome[accession] = chromosome;
            }

            return accessionToChromosome;
        }
    }
}
