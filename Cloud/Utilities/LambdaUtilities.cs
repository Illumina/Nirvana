using System;
using System.IO;
using Genome;
using IO;

namespace Cloud.Utilities
{
    public static class LambdaUtilities
    {
        public const string SuccessMessage = "Success";
        public const string SnsTopicKey    = "SnsTopicArn";

        public static void GarbageCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static string GetEnvironmentVariable(string key)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value)) throw new InvalidDataException($"Environment variable {key} is not set.");
            return value;
        }

        public static void DeleteTempOutput()
        {
            string[] files = Directory.GetFiles(Path.GetTempPath());
            if (files.Length == 0) return;
            foreach (string tempFile in files) File.Delete(tempFile);
        }

        public static string GetManifestUrl(string version, GenomeAssembly genomeAssembly, int saSchemaVersion, string baseUrl = null)
        {
            if (string.IsNullOrEmpty(version)) version = "latest";
            string s3BaseUrl = LambdaUrlHelper.GetManifestBaseUrl(baseUrl)+$"/{saSchemaVersion}/";
            switch (version)
            {
                case "latest":
                    return $"{s3BaseUrl}latest_SA_{genomeAssembly}.txt";
                case "release":
                    return $"{s3BaseUrl}DRAGEN_3.4_{genomeAssembly}.txt";
                case "none":
                    return null;
                default:
                    return $"{s3BaseUrl}{version}_SA_{genomeAssembly}.txt";
            }
        }

        
        public static string GetCachePathPrefix(GenomeAssembly genomeAssembly, string baseUrl=null)
        {
            return LambdaUrlHelper.GetCacheFolder(baseUrl).UrlCombine(genomeAssembly.ToString())
                .UrlCombine(LambdaUrlHelper.DefaultCacheSource);
        }
    }
}
