using CommandLine.VersionProviders;
using Xunit;

namespace UnitTests.CommandLine.VersionProviders
{
    public sealed class DefaultVersionProviderTests
    {
        [Fact]
        public void GetProgramVersion()
        {
            var versionProvider = new DefaultVersionProvider();
            Assert.Equal(string.Empty, versionProvider.DataVersion);
        }
    }
}
