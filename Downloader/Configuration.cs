using System.IO;
using Microsoft.Extensions.Configuration;

namespace Downloader
{
    public static class Configuration
    {
        public static (string HostName, string CacheDir, string ReferencesDir, string ManifestGRCh37, string ManifestGRCh38) Load()
        {
            const string appSettingsFilename = "Downloader.appsettings.json";

            var config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFilename)
                .Build();

            var dataSource = config.GetSection("DataSource");

            string hostname = dataSource["HostName"];
            if (string.IsNullOrEmpty(hostname)) throw new InvalidDataException($"Could not find the HostName entry in the {appSettingsFilename} file.");

            string cacheDir = dataSource["CacheDirectory"];
            if (string.IsNullOrEmpty(cacheDir)) throw new InvalidDataException($"Could not find the CacheDirectory entry in the {appSettingsFilename} file.");

            string referencesDir = dataSource["ReferencesDirectory"];
            if (string.IsNullOrEmpty(referencesDir)) throw new InvalidDataException($"Could not find the ReferencesDirectory entry in the {appSettingsFilename} file.");

            string manifestGRCh37 = dataSource["ManifestGRCh37"];
            if (string.IsNullOrEmpty(manifestGRCh37)) throw new InvalidDataException($"Could not find the ManifestGRCh37 entry in the {appSettingsFilename} file.");

            string manifestGRCh38 = dataSource["ManifestGRCh38"];
            if (string.IsNullOrEmpty(manifestGRCh38)) throw new InvalidDataException($"Could not find the ManifestGRCh38 entry in the {appSettingsFilename} file.");

            return (hostname, '/' + cacheDir, '/' + referencesDir, manifestGRCh37, manifestGRCh38);
        }
    }
}
