using System.Collections.Generic;
using CommandLine.Builders;
using ErrorHandling;
using Xunit;

namespace UnitTests.CommandLine.Builders
{
    public sealed class TopLevelAppBuilderTests
    {
        private readonly Dictionary<string, TopLevelOption> _ops;

        public TopLevelAppBuilderTests()
        {
            _ops = new Dictionary<string, TopLevelOption>
            {
                ["combine"] = new TopLevelOption("combine cache directories", EmptyMethod)
            };
        }

        private static ExitCodes EmptyMethod(string command, string[] args) => ExitCodes.Success;

        [Fact]
        public void Parse_UnsupportedOption()
        {
            var validator = new TopLevelAppBuilder(new[] {"--if", "-"}, _ops).Parse();
            Assert.True(validator.Data.Errors.Count > 0);

            var exitCode = validator
                .ShowBanner("banner")
                .ShowHelpMenu("help")
                .ShowErrors()
                .Execute();

            Assert.Equal(ExitCodes.UnknownCommandLineOption, exitCode);
        }

        [Fact]
        public void Parse_ShowHelpMenu()
        {
            var validator = new TopLevelAppBuilder(null, _ops).Parse();
            Assert.True(validator.Data.ShowHelpMenu);

            var exitCode = validator
                .ShowBanner("banner")
                .ShowHelpMenu("help")
                .ShowErrors()
                .Execute();

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void Parse_Nominal()
        {
            var exitCode = new TopLevelAppBuilder(new[] { "combine", "dummy" }, _ops)
                .Parse()
                .ShowBanner("banner")
                .ShowHelpMenu("help")
                .ShowErrors()
                .Execute();

            Assert.Equal(ExitCodes.Success, exitCode);
        }
    }
}
