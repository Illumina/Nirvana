using System.IO;
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
            }

            string cacheDir = dataSource["CacheDirectory"];
            if (string.IsNullOrEmpty(cacheDir))
                throw new InvalidDataException($"Could not find the CacheDirectory entry in the {appSettingsFilename} file.");

            string referencesDir = dataSource["ReferencesDirectory"];
            if (string.IsNullOrEmpty(referencesDir))
                throw new InvalidDataException($"Could not find the ReferencesDirectory entry in the {appSettingsFilename} file.");

            string manifestGRCh37;
            string manifestGRCh38;
            
            if (string.IsNullOrEmpty(manifestPrefix))
            {
                manifestGRCh37 = dataSource["ManifestGRCh37"];
                if (string.IsNullOrEmpty(manifestGRCh37))
                    throw new InvalidDataException($"Could not find the ManifestGRCh37 entry in the {appSettingsFilename} file.");

                manifestGRCh38 = dataSource["ManifestGRCh38"];
                if (string.IsNullOrEmpty(manifestGRCh38))
                    throw new InvalidDataException($"Could not find the ManifestGRCh38 entry in the {appSettingsFilename} file.");
            }
            else
            {
                manifestGRCh37 = $"{manifestPrefix}_GRCh37.txt";
                manifestGRCh38 = $"{manifestPrefix}_GRCh38.txt";
            }

            return (hostName, '/' + cacheDir, '/' + referencesDir, manifestGRCh37, manifestGRCh38);
        }
    }
}
