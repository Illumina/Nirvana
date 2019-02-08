using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.CommandLine.Builders
{
    public sealed class ValidationExtensionsTests
    {
        private static ExitCodes Execute(IConsoleAppValidator validator)
        {
            return validator
                .DisableOutput()
                .ShowBanner("authors")
                .ShowHelpMenu("description", "example")
                .ShowErrors()
                .Execute(() => ExitCodes.Success);
        }

        [Fact]
        public void CheckEachDirectoryContainsFiles_ContainsFiles_SuccessExitCode()
        {
            string tempDir = Path.GetTempPath();

            const string suffix = ".txt";
            string randomPath = RandomPath.GetRandomPath() + suffix;
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
            string tempDir = Path.GetTempPath();
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
            string randomPath = RandomPath.GetRandomPath();
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
            string randomPath = RandomPath.GetRandomPath() + ".anavrin";

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
        public void CheckDirectoryExists_MissingDirectory_PathNotFoundExitCode()
        {
            var ops = new OptionSet { { "if=", "if", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if", "-" }, ops)
                .Parse()
                .CheckDirectoryExists("-", "test", "--if"));

            Assert.Equal(ExitCodes.PathNotFound, exitCode);
        }

        [Fact]
        public void CheckDirectoryExists_EmptyPath_MissingCommandLineOptionExitCode()
        {
            var ops = new OptionSet { { "if=", "if", v => { } } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if", "-" }, ops)
                .Parse()
                .CheckDirectoryExists(null, "test", "--if"));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void CheckEachFilenameExists_MissingFile_MissingCommandLineOptionExitCode()
        {
            var ops = new OptionSet { { "if=", "if", v => { } } };
            var filenames = new List<string> { "bob", null };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--if", "-" }, ops)
                .Parse()
                .CheckEachFilenameExists(filenames, "test", "--if"));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
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
        public void HasRequiredDate_Exists_SuccessExitCode()
        {
            string observedDate = default(string);
            const string expectedDate = "2018-03-14";

            var ops = new OptionSet { { "date=", "date", v => observedDate = v } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--date", expectedDate }, ops)
                .Parse()
                .HasRequiredDate(observedDate, "date", "--date"));

            Assert.Equal(expectedDate, observedDate);
            Assert.Equal(ExitCodes.Success, exitCode);
        }

        [Fact]
        public void HasRequiredDate_Exists_BadFormat()
        {
            string observedDate = default(string);
            var ops = new OptionSet { { "date=", "date", v => observedDate = v } };

            var validator = new ConsoleAppBuilder(new[] { "--date", "garbage" }, ops)
                .Parse()
                .HasRequiredDate(observedDate, "date", "--date");

            Assert.True(validator.Data.Errors.Count > 0);
        }

        [Fact]
        public void HasRequiredDate_DoesNotExist_MissingCommandLineExitCode()
        {
            string observedDate = default(string);

            var ops = new OptionSet { { "date=", "date", v => observedDate = v } };

            var exitCode = Execute(new ConsoleAppBuilder(new[] { "--bar", "bar" }, ops)
                .Parse()
                .HasRequiredDate(observedDate, "date", "--date"));

            Assert.Equal(ExitCodes.MissingCommandLineOption, exitCode);
        }

        [Fact]
        public void CheckNonZero_True()
        {
            const string expectedDate = "2018-03-14";
            var ops = new OptionSet { { "date=", "date", v => { } } };

            var validator = new ConsoleAppBuilder(new[] { "--date", expectedDate }, ops)
                .Parse()
                .CheckNonZero(3, "date");

            Assert.Equal(ExitCodes.Success, validator.Data.ExitCode);
            Assert.Empty(validator.Data.Errors);
        }

        [Fact]
        public void CheckNonZero_False()
        {
            var ops = new OptionSet { { "date=", "date", v => { } } };

            var validator = new ConsoleAppBuilder(new[] { "--date", "2018-03-14" }, ops)
                .Parse()
                .CheckNonZero(0, "date");

            Assert.NotEqual(ExitCodes.Success, validator.Data.ExitCode);
            Assert.True(validator.Data.Errors.Count > 0);
        }

        [Fact]
        public void CheckOutputFilenameSuffix_True()
        {
            var ops = new OptionSet { { "date=", "date", v => { } } };

            var validator = new ConsoleAppBuilder(new[] {"--date", "2018-03-14" }, ops)
                .Parse()
                .CheckOutputFilenameSuffix("test.json", ".json", "temp");

            Assert.Equal(ExitCodes.Success, validator.Data.ExitCode);
            Assert.Empty(validator.Data.Errors);
        }

        [Fact]
        public void CheckOutputFilenameSuffix_False()
        {
            var ops = new OptionSet { { "date=", "date", v => { } } };

            var validator = new ConsoleAppBuilder(new[] { "--date", "2018-03-14" }, ops)
                .Parse()
                .CheckOutputFilenameSuffix("test.json", ".gz", "temp");

            Assert.NotEqual(ExitCodes.Success, validator.Data.ExitCode);
            Assert.True(validator.Data.Errors.Count > 0);
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
            var observedFlag = false;
            var ops = new OptionSet { { "test=", "test", v => { } } };

            Execute(new ConsoleAppBuilder(new[] { "--test", "test" }, ops)
                .Parse()
                .Enable(true, () => { observedFlag = true; }));

            Assert.True(observedFlag);
        }

        [Fact]
        public void Enable_False_SkipMethod()
        {
            var observedFlag = false;
            var ops = new OptionSet { { "test=", "test", v => { } } };

            Execute(new ConsoleAppBuilder(new[] { "--test", "test" }, ops)
                .Parse()
                .Enable(false, () => { observedFlag = true; }));

            Assert.False(observedFlag);
        }
    }
}
