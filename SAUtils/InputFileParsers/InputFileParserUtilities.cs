using System.Collections.Generic;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers
{
    public static class InputFileParserUtilities
    {
        public static List<string> ChromosomeWhiteList = new List<string>();
        public static readonly HashSet<string> ProcessedReferences = new HashSet<string>();

        public static bool IsDesiredChromosome(string chromosome, ChromosomeRenamer renamer)
        {
            if (ChromosomeWhiteList == null) return true;
            if (ChromosomeWhiteList.Count == 0) return true;
            return ChromosomeWhiteList.Contains(renamer.GetEnsemblReferenceName(chromosome));
        }
    }
}
