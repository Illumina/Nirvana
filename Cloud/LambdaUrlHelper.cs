using Cloud.Utilities;
using Genome;
using IO;
using ReferenceSequence;

namespace Cloud
{
    public static class LambdaUrlHelper
    {
        public const string UrlBaseEnvironmentVariableName = "NirvanaDataUrlBase";

        public const string S3CacheFolderBase = "ab0cf104f39708eabd07b8cb67e149ba-Cache";
        public static readonly string S3CacheFolder =
            $"{S3CacheFolderBase}/{CacheConstants.DataVersion}/";

        private static readonly string S3RefPrefix =
            $"d95867deadfe690e40f42068d6b59df8-References/{ReferenceSequenceCommon.HeaderVersion}/Homo_sapiens.";

        private const string UgaFileName        = "UGA.tsv.gz";
        public const  string DefaultCacheSource = "Both";
        public const  string RefSuffix          = ".Nirvana.dat";
        public const  string JsonSuffix         = ".json.gz";
        public const  string JsonIndexSuffix    = ".jsi";
        public const  string SuccessMessage     = "Success";

        public static string GetBaseUrl(string baseUrl = null) =>
            baseUrl ?? LambdaUtilities.GetEnvironmentVariable(UrlBaseEnvironmentVariableName);

        public static string GetCacheFolder(string baseUrl = null) => GetBaseUrl(baseUrl)     + S3CacheFolder;
        public static string GetUgaUrl(string      baseUrl = null) => GetCacheFolder(baseUrl) + UgaFileName;
        public static string GetRefPrefix(string   baseUrl = null) => GetBaseUrl(baseUrl)     + S3RefPrefix;

        public static string GetRefUrl(GenomeAssembly genomeAssembly, string baseUrl = null) =>
            GetRefPrefix(baseUrl) + genomeAssembly + RefSuffix;
    }
}