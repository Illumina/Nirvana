using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace SAUtils
{
    public class SAVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"Nirvana {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion() => $"Supplementary annotation version: {SupplementaryAnnotationCommon.DataVersion}";
    }
}
