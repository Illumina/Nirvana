using System;
using Cloud.Utilities;
using Genome;
using IO;
using ReferenceSequence;

namespace Cloud
{
    public static class LambdaUrlHelper
    {
        public const            ushort        SaSchemaVersion                = 22;
        public const            string        UrlBaseEnvironmentVariableName = "NirvanaDataUrlBase";
        private static readonly Configuration Config                         = new ();

        public static string S3CacheFolderBase    = Config.CacheDirectory;
        // public const string S3ManifestFolderBase = "a9f54ea6ac0548696c97a3ee64bc39ec2e71b84b-SaManifest";
        public static readonly string S3CacheFolder =
            $"{Config.CacheDirectory}/{CacheConstants.DataVersion}/";

        private static readonly string S3RefPrefix =
            $"{Config.ReferencesDirectory}/{ReferenceSequenceCommon.HeaderVersion}/Homo_sapiens.";

        
        private const string UgaFileName        = "UGA.tsv.gz";
        public const  string DefaultCacheSource = "Both";
        public const  string RefSuffix          = ".Nirvana.dat";
        public const  string JsonSuffix         = ".json.gz";
        public const  string JsonIndexSuffix    = ".jsi";
        public const  string SuccessMessage     = "Success";

        public static string GetBaseUrl()
        {
            var envBaseUrl = Environment.GetEnvironmentVariable(UrlBaseEnvironmentVariableName);
            
            return string.IsNullOrEmpty(envBaseUrl) ? Config.NirvanaBaseUrl: envBaseUrl;
        }

        public static string GetManifestBaseUrl() => GetBaseUrl() + Config.ManifestDirectory;
        
        public static string GetCacheFolder() => GetBaseUrl()     + S3CacheFolder;
        public static string GetUgaUrl() => GetCacheFolder() + UgaFileName;
        public static string GetRefPrefix() => GetBaseUrl()     + S3RefPrefix;

        public static string GetRefUrl(GenomeAssembly genomeAssembly) =>
            GetRefPrefix() + genomeAssembly + RefSuffix;
    }
}