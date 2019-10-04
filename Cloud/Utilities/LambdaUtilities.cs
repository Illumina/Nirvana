using System;
using System.IO;
using Genome;

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

        public static string GetManifestUrl(string version, GenomeAssembly genomeAssembly)
        {
            if (string.IsNullOrEmpty(version)) version = "latest";

            switch (version)
            {
                case "latest":
                    return $"{NirvanaHelper.S3Url}latest_SA_{genomeAssembly}.txt";
                case "release":
                    return $"{NirvanaHelper.S3Url}DRAGEN_3.4_{genomeAssembly}.txt";
                default:
                    return $"{NirvanaHelper.S3Url}{version}_SA_{genomeAssembly}.txt";
            }
        }
    }
}
