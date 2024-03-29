﻿using System.Collections.Generic;
using ErrorHandling.Exceptions;

namespace Genome
{
    public static class GenomeAssemblyHelper
    {
        public static readonly HashSet<GenomeAssembly> AutosomeAndAllosomeAssemblies =
            new HashSet<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38, GenomeAssembly.hg19,GenomeAssembly.SARSCoV2 };

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
                case "sarscov2":
                    ret = GenomeAssembly.SARSCoV2;
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