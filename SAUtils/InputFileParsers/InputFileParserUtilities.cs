using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace SAUtils.InputFileParsers
{
    public static class InputFileParserUtilities
    {
        public static List<string> ChromosomeWhiteList = new List<string>();

        public static bool IsDesiredChromosome(string chromosome, IChromosomeRenamer renamer)
        {
            if (ChromosomeWhiteList == null) return true;
            if (ChromosomeWhiteList.Count == 0) return true;
            return ChromosomeWhiteList.Contains(renamer.GetEnsemblReferenceName(chromosome));
        }
    }
}
