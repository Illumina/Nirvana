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
            string suffix = $"{genomeAssembly}/{LambdaUrlHelper.DefaultCacheSource}";

            //LambdaUrlHelper.GetBaseUrl() + 
            switch (vepVersion)
            {
                case 84:
                    return UrlCombine($"{LambdaUrlHelper.GetBaseUrl()+LambdaUrlHelper.S3CacheFolderBase}/26/VEP84/", suffix);
                default:
                    return UrlCombine($"{LambdaUrlHelper.GetBaseUrl()+LambdaUrlHelper.S3CacheFolder}", suffix);
            }
            
        }
    }
}
