using Cloud.Utilities;
using Genome;

namespace Cloud
{
    public static class NirvanaHelper
    {
        public const string UrlBaseEnvironmentVariableName = "NirvanaDataUrlBase";

        public static readonly string S3Url                = LambdaUtilities.GetEnvironmentVariable(UrlBaseEnvironmentVariableName);
        public static readonly string S3CacheFolder        = S3Url + "ab0cf104f39708eabd07b8cb67e149ba-Cache/26/";
        public static readonly string S3UgaPath            = S3CacheFolder + "UGA.tsv.gz";
        public const string DefaultCacheSource             = "Both";
        public static readonly string S3RefPrefix          = S3Url + "d95867deadfe690e40f42068d6b59df8-References/5/Homo_sapiens.";

        public const string RefSuffix                      = ".Nirvana.dat";
        public const string JsonSuffix                     = ".json.gz";
        public const string JsonIndexSuffix                = ".jsi";

        public const string SuccessMessage                 = "Success";

        public static string GetS3RefLocation(GenomeAssembly genomeAssembly) => S3RefPrefix + genomeAssembly + RefSuffix;
    }
}