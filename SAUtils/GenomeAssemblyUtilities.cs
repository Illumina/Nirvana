using System;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils
{
    public static class GenomeAssemblyUtilities
    {
        public static GenomeAssembly Convert(string genomeAssembly)
        {
            GenomeAssembly ret;

            switch (String.IsNullOrEmpty(genomeAssembly) ? String.Empty : genomeAssembly.ToLower())
            {
                case "grch37":
                    ret = GenomeAssembly.GRCh37;
                    break;
                case "grch38":
                    ret = GenomeAssembly.GRCh38;
                    break;
                case "hg19":
                    ret = GenomeAssembly.hg19;
                    break;
                case "rcrs":
                    ret = GenomeAssembly.rCRS;
                    break;
                case "":
                    ret = GenomeAssembly.Unknown;
                    break;
                default:
                    throw new UserErrorException($"Unknown genome assembly was specified: {genomeAssembly}");
            }

            return ret;
        }
    }
}