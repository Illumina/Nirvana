using System;

namespace VariantAnnotation.Interface.Sequence
{
    public static class GenomeAssemblyHelper
    {
        public static GenomeAssembly Convert(string genomeAssembly)
        {
            GenomeAssembly ret;

            switch (string.IsNullOrEmpty(genomeAssembly) ? string.Empty : genomeAssembly.ToLower())
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
                    throw new ArgumentException($"Unknown genome assembly was specified: {genomeAssembly}");
            }

            return ret;
        }
    }
}