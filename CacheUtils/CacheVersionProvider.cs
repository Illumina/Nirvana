using CommandLine.Utilities;
using CommandLine.VersionProvider;
using VariantAnnotation.FileHandling.TranscriptCache;

namespace CacheUtils
{
    public class CacheVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"Nirvana {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion() => $"Cache version: {CacheConstants.DataVersion}";
    }
}
