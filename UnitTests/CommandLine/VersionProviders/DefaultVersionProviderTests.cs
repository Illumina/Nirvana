using CommandLine.VersionProviders;
using Xunit;

namespace UnitTests.CommandLine.VersionProviders
{
    public sealed class DefaultVersionProviderTests
    {
        [Fact]
        public void GetProgramVersion()
        {
            var programVersionCols = new DefaultVersionProvider().GetProgramVersion().Split(' ');
            Assert.Equal(2, programVersionCols.Length);
        }
    }
}
