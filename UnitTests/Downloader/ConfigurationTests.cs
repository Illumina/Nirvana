using Downloader;
using Xunit;

namespace UnitTests.Downloader
{
    public sealed class ConfigurationTests
    {
        [Fact]
        public void Load_Nominal()
        {
            (string hostName, string cacheDir, string referencesDir, string manifestGRCh37, string manifestGRCh38) = Configuration.Load();
            Assert.EndsWith("cloudfront.net", hostName);
            Assert.StartsWith("/", cacheDir);
            Assert.EndsWith("Cache", cacheDir);
            Assert.StartsWith("/", referencesDir);
            Assert.EndsWith("References", referencesDir);
            Assert.Contains("GRCh37", manifestGRCh37);
            Assert.Contains("GRCh38", manifestGRCh38);
        }
    }
}
