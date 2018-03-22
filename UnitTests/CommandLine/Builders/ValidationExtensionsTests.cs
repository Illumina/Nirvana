using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.CommandLine.Builders
{
    public sealed class ValidationExtensionsTests : RandomFileBase
    {
        private static ExitCodes Execute(IConsoleAppValidator validator)
        {
            return validator
                .DisableOutput(true)
                .ShowBanner("authors")
                .ShowHelpMenu("description", "example")
                .ShowErrors()
                .Execute(() => ExitCodes.Success);
        }

        [Fact]
        public void CheckEachDirectoryContainsFiles_ContainsFiles_SuccessExitCode()
        {
            var tempDir = Path.GetTempPath();

            const string suffix = ".txt";
            var randomPath = GetRandomPath() + suffix;
            File.Create(randomPath);

            var ops = new OptionSet { { "id=", "id", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--id", tempDir }, ops)
                .Parse()
                .CheckEachDirectoryContainsFiles(new[] { tempDir }, "test", "--id", $"*{suffix}"));

            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void CheckEachDirectoryContainsFiles_MissingFiles_FileNotFoundExitCode()
        {
            var tempDir = Path.GetTempPath();
            const string suffix = ".anavrin";

            var ops = new OptionSet { { "id=", "id", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--id", tempDir }, ops)
                .Parse()
                .CheckEachDirectoryContainsFiles(new[] { tempDir }, "test", "--id", $"*{suffix}"));

            Assert.Equal(ExitCodes.FileNotFound, exitCode);
        }

        [Fact]
        public void CheckEachDirectoryContainsFiles_MissingArguments_MissingCommandLineExitCode()
        {
            var ops = new OptionSet { { "id=", "id", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(null, ops)
                .Parse()
                .CheckEachDirectoryContainsFiles(null, "test", "--id", null));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void CheckInputFilenameExists_FileExists_SuccessExitCode()
        {
            var randomPath = GetRandomPath();
            File.Create(randomPath);

            var ops = new OptionSet { { "if=", "if", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if", randomPath }, ops)
                .Parse()
                .CheckInputFilenameExists(randomPath, "test", "--if"));

            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void CheckInputFilenameExists_MissingFiles_FileNotFoundExitCode()
        {
            var randomPath = GetRandomPath() + ".anavrin";

            var ops = new OptionSet { { "id=", "id", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if", randomPath }, ops)
                .Parse()
                .CheckInputFilenameExists(randomPath, "test", "--if"));

            Assert.Equal(ExitCodes.FileNotFound, exitCode);
        }

        [Fact]
        public void CheckInputFilenameExists_MissingArguments_MissingCommandLineExitCode()
        {
            var ops = new OptionSet { { "if=", "if", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(null, ops)
                .Parse()
                .CheckInputFilenameExists(null, "test", "--if"));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void CheckInputFilenameExists_EmptyPath_MissingCommandLineExitCode()
        {
            var ops = new OptionSet { { "if=", "if", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if" }, ops)
                .Parse()
                .CheckInputFilenameExists(null, "test", "--if"));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void CheckInputFilenameExists_IgnoredPath_SuccessExitCode()
        {
            var ops = new OptionSet { { "if=", "if", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if", "-" }, ops)
                .Parse()
                .CheckInputFilenameExists("-", "test", "--if",true, "-"));

            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void HasRequiredParameter_Exists_SuccessExitCode()
        {
            string observedString       = default(string);
            const string expectedString = "foo";

            var ops = new OptionSet { { "test=", "test", v => observedString = v } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--test", expectedString }, ops)
                .Parse()
                .HasRequiredParameter(observedString, "test", "--test"));

            Assert.Equal(expectedString, observedString);
            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void HasRequiredParameter_DoesNotExist_MissingCommandLineExitCode()
        {
            string testString           = default(string);
            const string expectedString = default(string);

            var ops = new OptionSet
            {
                {"test=", "test", v => testString = v},
                {"bar=", "bar", v => { } }
            };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--bar", "bar" }, ops)
                .Parse()
                .HasRequiredParameter(testString, "test", "--test"));

            Assert.Equal(expectedString, testString);
            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void HasRequiredParameter_MissingArguments_MissingCommandLineExitCode()
        {
            string observedString = default(string);
            var ops = new OptionSet { { "test=", "test", v => observedString = v } };

            var exitCode = Execute(new ConsoleAppBuilder(null, ops)
                .Parse()
                .HasRequiredParameter(observedString, "test", "--test"));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void Enable_True_ExecuteMethod()
        {
            bool observedFlag = false;
            var ops = new OptionSet { { "test=", "test", v => { } } };

            Execute(new ConsoleAppBuilder(new[] { "--test", "test" }, ops)
                .Parse()
                .Enable(true, () => { observedFlag = true; }));

            Assert.True(observedFlag);
        }

        [Fact]
        public void Enable_False_SkipMethod()
        {
            bool observedFlag = false;
            var ops = new OptionSet { { "test=", "test", v => { } } };

            Execute(new ConsoleAppBuilder(new[] { "--test", "test" }, ops)
                .Parse()
                .Enable(false, () => { observedFlag = true; }));

            Assert.False(observedFlag);
        }
    }
}
