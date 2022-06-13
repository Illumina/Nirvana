using Cloud.Utilities;

namespace Cloud;
using Microsoft.Extensions.Configuration;

public sealed class Configuration
{
    public readonly IConfigurationRoot    Config;
    public readonly IConfigurationSection DataSources;
    public          string                CacheDirectory      => DataSources["CacheDirectory"];
    public          string                ReferencesDirectory => DataSources["ReferencesDirectory"];
    public          string                ManifestDirectory   => DataSources["ManifestDirectory"];
    public          string                NirvanaBaseUrl      => DataSources["BaseUrl"];
    public Configuration()
    {
        const string appSettingsFilename = "Cloud.appsettings.json";

        Config = new ConfigurationBuilder()
            .AddJsonFile(appSettingsFilename)
            .Build();

        DataSources = Config.GetSection("DataSource");

    }

    
}