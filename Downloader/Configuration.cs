using System;
using System.IO;
using Cloud;
using Cloud.Utilities;
using Genome;
using Microsoft.Extensions.Configuration;

namespace Downloader
{
    public static class Configuration
    {
        public static (string HostName, string CacheDir, string ReferencesDir, string ManifestGRCh37, string ManifestGRCh38) Load(
            string hostName, string manifestPrefix)
        {
            const string appSettingsFilename = "Downloader.appsettings.json";

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFilename)
                .Build();

            IConfigurationSection dataSource = config.GetSection("DataSource");

            if (string.IsNullOrEmpty(hostName))
            {
                hostName = dataSource["HostName"];
                if (string.IsNullOrEmpty(hostName))
                    throw new InvalidDataException($"Could not find the HostName entry in the {appSettingsFilename} file.");
                // this env variable will over-ride the configuration in cloud
                Environment.SetEnvironmentVariable(LambdaUrlHelper.UrlBaseEnvironmentVariableName, $"http://{hostName}/");
            }

            var    cloudConfiguration = new Cloud.Configuration();
            string cacheDir           = cloudConfiguration.CacheDirectory;
            if (string.IsNullOrEmpty(cacheDir))
                throw new InvalidDataException($"Could not find the CacheDirectory entry in the Cloud.appsettings.json file.");

            string referencesDir = cloudConfiguration.ReferencesDirectory;
            if (string.IsNullOrEmpty(referencesDir))
                throw new InvalidDataException($"Could not find the ReferencesDirectory entry in the Cloud.appsettings.json file.");

            string manifestGRCh37 ;
            string manifestGRCh38 ;
            
            if (string.IsNullOrEmpty(manifestPrefix))
            {
                manifestGRCh37 = LambdaUtilities.GetManifestUrl(dataSource["ManifestGRCh37"], GenomeAssembly.GRCh37);
                if (string.IsNullOrEmpty(manifestGRCh37))
                    throw new InvalidDataException($"Could not find the ManifestGRCh37 entry in the {appSettingsFilename} file.");

                manifestGRCh38 = LambdaUtilities.GetManifestUrl(dataSource["ManifestGRCh38"], GenomeAssembly.GRCh38);
                if (string.IsNullOrEmpty(manifestGRCh38))
                    throw new InvalidDataException($"Could not find the ManifestGRCh38 entry in the {appSettingsFilename} file.");
            }
            else
            {
                manifestGRCh37 = LambdaUtilities.GetManifestUrl($"{manifestPrefix}", GenomeAssembly.GRCh37);
                manifestGRCh38 = LambdaUtilities.GetManifestUrl($"{manifestPrefix}", GenomeAssembly.GRCh38);
            }

            return (hostName, '/' + cacheDir, '/' + referencesDir, manifestGRCh37, manifestGRCh38);
        }
    }
}
