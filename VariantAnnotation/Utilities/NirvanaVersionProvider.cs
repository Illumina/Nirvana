using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.TranscriptCache;

namespace VariantAnnotation.Utilities
{
    public sealed class NirvanaVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"Nirvana {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion() => 
            $"Cache version: {CacheConstants.DataVersion}, Supplementary annotation version: {SupplementaryAnnotationCommon.DataVersion}, Reference version: {CompressedSequenceCommon.HeaderVersion}";
    }
}
