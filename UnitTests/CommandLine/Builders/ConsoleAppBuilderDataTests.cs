using CommandLine.Builders;
using CommandLine.NDesk.Options;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.CommandLine.Builders
{
    public sealed class ConsoleAppBuilderDataTests
    {
        [Fact]
        public void VersionProvider_Set()
        {
            var ops = new OptionSet { { "test=", "test", v => { } } };

            var data = new ConsoleAppBuilder(null, ops).UseVersionProvider(new VersionProvider())
                .Parse()
                .Data;

            Assert.True(data.VersionProvider is VersionProvider);
        }
    }

    public sealed class ConsoleAppValidatorTests
    {
        [Fact]
        public void ShowBanner_EnabledOutput()
        {
            var ops = new OptionSet { { "test=", "test", v => { } } };

            var banner = new ConsoleAppBuilder(null, ops).UseVersionProvider(new VersionProvider())
                .Parse()
                .ShowBanner("authors");

            Assert.True(banner is ConsoleAppBanner);
        }
    }
}