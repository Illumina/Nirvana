using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace CacheUtils
{
    public class CacheVersionProvider : IVersionProvider
    {
        public string GetProgramVersion() => $"Nirvana {CommandLineUtilities.InformationalVersion}";

        public string GetDataVersion() => $"Cache version: {NirvanaDatabaseCommon.DataVersion}";
    }
}
