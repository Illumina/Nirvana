using Downloader;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.Downloader
{
    public sealed class ConfigurationTests
    {
        [Fact]
        public void Load_ExpectedResults()
        {
            (string hostName, string cacheDir, string referencesDir, string manifestGRCh37, string manifestGRCh38) = Configuration.Load(null, null);
            Assert.EndsWith("annotations.nirvana.illumina.com", hostName);
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
            var config = new global::Cloud.Configuration();
            (string _, string _, string _, string manifestGRCh37, string manifestGRCh38) = Configuration.Load(null, "Schema23");
            Assert.Equal($"http://annotations.nirvana.illumina.com/{config.ManifestDirectory}/{SaCommon.SchemaVersion}/Schema23_SA_GRCh37.txt", manifestGRCh37);
            Assert.Equal($"http://annotations.nirvana.illumina.com/{config.ManifestDirectory}/{SaCommon.SchemaVersion}/Schema23_SA_GRCh38.txt", manifestGRCh38);
        }
    }
}