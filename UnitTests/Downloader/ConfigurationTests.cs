using Downloader;
using Xunit;

namespace UnitTests.Downloader
{
    public sealed class ConfigurationTests
    {
        [Fact]
        public void Load_ExpectedResults()
        {
            (string hostName, string cacheDir, string referencesDir, string manifestGRCh37, string manifestGRCh38) = Configuration.Load(null, null);
            Assert.EndsWith("cloudfront.net", hostName);
            Assert.StartsWith("/", cacheDir);
            Assert.EndsWith("Cache", cacheDir);
            Assert.StartsWith("/", referencesDir);
            Assert.EndsWith("References", referencesDir);
            Assert.Contains("GRCh37", manifestGRCh37);
            Assert.Contains("GRCh38", manifestGRCh38);
        }

        [Fact]
        public void Load_OverrideHostName()
        {
            (string hostName, string _, string _, string _, string _) = Configuration.Load("www.illumina.com", null);
            Assert.Equal("www.illumina.com", hostName);
        }

        [Fact]
        public void Load_OverrideManifest()
        {
            (string _, string _, string _, string manifestGRCh37, string manifestGRCh38) = Configuration.Load(null, "Schema23");
            Assert.Equal("Schema23_GRCh37.txt", manifestGRCh37);
            Assert.Equal("Schema23_GRCh38.txt", manifestGRCh38);
        }
    }
}