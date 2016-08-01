using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace VariantAnnotation.Utilities
{
    public class NirvanaVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"Nirvana {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion() => 
            $"Cache version: {NirvanaDatabaseCommon.DataVersion}, Supplementary annotation version: {SupplementaryAnnotationCommon.DataVersion}, Reference version: {CompressedSequenceCommon.HeaderVersion}";
    }
}
