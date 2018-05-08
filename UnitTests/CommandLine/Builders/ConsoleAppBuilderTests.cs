using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using Xunit;

namespace UnitTests.CommandLine.Builders
{
    public sealed class ConsoleAppBuilderTests
    {
        [Fact]
        public void Parse_UnsupportedOption()
        {
            var ops = new OptionSet { { "test=", "test", v => { } } };

            var data = new ConsoleAppBuilder(new[] { "--if", "-" }, ops)
                .Parse()
                .Data;

            Assert.Single(data.Errors);
            Assert.Equal(2, data.UnsupportedOps.Count);
        }

        [Fact]
        public void Parse_Version()
        {
            var ops = new OptionSet { { "test=", "test", v => { } } };

            var validator = new ConsoleAppBuilder(new[] {"--version"}, ops)
                .Parse();

            Assert.True(validator.Data.ShowVersion);

            var exitCode = validator
                .CheckInputFilenameExists("dummy", "vcf", "--in")
                .ShowBanner("authors")
                .ShowHelpMenu("description", "example")
                .ShowErrors()
                .Execute(() => ExitCodes.Success);

            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void Parse_HelpMenu()
        {
            var ops = new OptionSet { { "test=", "test", v => { } } };

            var validator = new ConsoleAppBuilder(new[] { "--help" }, ops)
                .Parse();

            Assert.True(validator.Data.ShowHelpMenu);

            var exitCode = validator
                .CheckInputFilenameExists("dummy", "vcf", "--in")
                .ShowBanner("authors")
                .ShowHelpMenu("description", "example")
                .ShowErrors()
                .Execute(() => ExitCodes.Success);

            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void Parse_ShowOutput()
        {
            var ops = new OptionSet { { "test=", "test", v => { } } };

            var exitCode = new ConsoleAppBuilder(new[] { "--test", "test" }, ops)
                .Parse()
                .ShowBanner("authors")
                .ShowHelpMenu("description", "example")
                .ShowErrors()
                .Execute(() => ExitCodes.Success);

            Assert.Equal(ExitCodes.Success, exitCode);
        }
    }
}
