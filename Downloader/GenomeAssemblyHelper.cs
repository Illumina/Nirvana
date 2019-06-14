using System.Collections.Generic;
using ErrorHandling.Exceptions;
using Genome;

namespace Downloader
{
    public static class GenomeAssemblyHelper
    {
        public static List<GenomeAssembly> GetGenomeAssemblies(string genomeAssembly)
        {
            genomeAssembly = genomeAssembly.ToLower();
            var genomeAssemblies = new List<GenomeAssembly>();

            switch (genomeAssembly.ToLower())
            {
                case "grch37":
                    genomeAssemblies.Add(GenomeAssembly.GRCh37);
                    break;
                case "grch38":
                    genomeAssemblies.Add(GenomeAssembly.GRCh38);
                    break;
                case "both":
                    genomeAssemblies.Add(GenomeAssembly.GRCh37);
                    genomeAssemblies.Add(GenomeAssembly.GRCh38);
                    break;
                default:
                    throw new UserErrorException($"Found an unknown genome assembly ({genomeAssembly}). Expected: GRCh37, GRCh38, or both");
            }

            return genomeAssemblies;
        }
    }
}
