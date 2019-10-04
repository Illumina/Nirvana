using System.Linq;
using Cloud;
using Genome;

namespace SingleAnnotationLambda
{
    public static class CacheUtilities
    {
        public const int DefaultVepVersion = 91;

        private static readonly int[] SupportedVepVersions = { 84, 91 };

        public static bool IsVepVersionSupported(int vepVersion) =>
            SupportedVepVersions.Any(supportedVepVersion => vepVersion == supportedVepVersion);

        public static string GetSupportedVersions() => string.Join(", ", SupportedVepVersions);

        private static string UrlCombine(string baseUrl, string relativeUrl) => baseUrl.TrimEnd('/') + '/' + relativeUrl.TrimStart('/');

        public static string GetCachePathPrefix(int vepVersion, GenomeAssembly genomeAssembly)
        {
            string cacheFolder = NirvanaHelper.S3Url + "ab0cf104f39708eabd07b8cb67e149ba-Cache/26/";
            string suffix      = $"{genomeAssembly}/{NirvanaHelper.DefaultCacheSource}";

            return UrlCombine(vepVersion == 84 ? $"{cacheFolder}VEP84/" : cacheFolder, suffix);
        }
    }
}
