using System.IO;
using ErrorHandling.DataStructures;
using ErrorHandling.Exceptions;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class AbstractCommandLineHandlerTests : RandomFileBase
    {
        #region members

        private readonly CommandLineExample _example;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public AbstractCommandLineHandlerTests()
        {
            var ops = new OptionSet
            {
                {
                    "id=",
                    "input {directory}",
                    v => ConfigurationSettingsExample.InputDirectory = v
                },
                {
                    "if=",
                    "input {filename}",
                    v => ConfigurationSettingsExample.InputFilename = v
                },
                {
                    "num=",
                    "number",
                    (int v) => ConfigurationSettingsExample.Number = v
                },
            };

            _example = new CommandLineExample("Tests the command-line parameters", ops, "command-line example", VariantAnnotation.DataStructures.Constants.Authors);
        }

        [Fact]
        public void CheckDirectoryExists()
        {
            var randomDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(randomDir);
            _example.CheckDirectoryExistsTest(randomDir);
            Directory.Delete(randomDir);
        }

        [Fact]
        public void CheckInputFilenameExists()
        {
            var randomPath = GetRandomPath();
            File.Create(randomPath);
            _example.CheckInputFilenameExistsTest(randomPath);
        }

        [Fact]
        public void HasRequiredParameter()
        {
            _example.HasRequiredParameterTest();
        }

        [Fact]
        public void HasRequiredDate()
        {
            _example.HasRequiredDateTest();
        }

        [Fact]
        public void HasOneOptionSelected()
        {
            _example.HasOneOptionSelectedTest();
        }

        [Fact]
        public void CheckNonZero()
        {
            _example.CheckNonZeroTest();
        }

        [Fact]
        public void HasOnlyOneOption()
        {
            _example.HasOnlyOneOptionTest();
        }

        [Fact]
        public void ExecuteNoArgs()
        {
            _example.ExecuteNoArgsTest();
        }

        [Fact]
        public void ExecuteUnsupportedOptions()
        {
            _example.ExecuteUnsupportedOptionsTest();
        }

        [Fact]
        public void ExecuteOptionException()
        {
            _example.ExecuteOptionExceptionTest();
        }

        [Fact]
        public void ExecuteVersion()
        {
            _example.ExecuteVersionTest();
        }

        [Fact]
        public void ExecuteHelp()
        {
            _example.ExecuteHelpTest();
        }

        [Fact]
        public void ExecuteParsingErrors()
        {
            _example.ExecuteParsingErrorsTest();
        }

        [Fact]
        public void ExecuteException()
        {
            _example.ExecuteExceptionTest();
        }

        [Fact]
        public void Execute()
        {
            _example.ExecuteTest();
        }

        [Fact]
        public void ConfigurationSettings()
        {
            _example.ConfigurationSettings();
        }
    }

    public class CommandLineExample : AbstractCommandLineHandler
    {
        private bool _throwException;
        private bool _executed;

        /// <summary>
        /// constructor
        /// </summary>
        public CommandLineExample(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine() { }

        private void AddParsingErrors()
        {
            CheckDirectoryExists("bob", "test", "--test");
            Assert.Equal((int)ExitCodes.PathNotFound, ExitCode);
        }

        /// <summary>
        /// tests the check directory functionality
        /// </summary>
        public void CheckDirectoryExistsTest(string directoryPath)
        {
            // exists
            CheckDirectoryExists(directoryPath, "test", "--test");
            Assert.Equal(0, ExitCode);

            // missing
            CheckDirectoryExists(null, "test", "--test");
            Assert.Equal((int)ExitCodes.MissingCommandLineOption, ExitCode);
            ExitCode = 0;

            // does not exist
            CheckDirectoryExists("bob", "test", "--test");
            Assert.Equal((int)ExitCodes.PathNotFound, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests the check input filename functionality
        /// </summary>
        public void CheckInputFilenameExistsTest(string randomPath)
        {
            // missing
            CheckInputFilenameExists(randomPath, "test", "--test");
            Assert.Equal(0, ExitCode);

            // missing
            CheckInputFilenameExists(null, "test", "--test");
            Assert.Equal((int)ExitCodes.MissingCommandLineOption, ExitCode);
            ExitCode = 0;

            // does not exist
            CheckInputFilenameExists("bob", "test", "--test");
            Assert.Equal((int)ExitCodes.FileNotFound, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests the has required parameter functionality
        /// </summary>
        public void HasRequiredParameterTest()
        {
            ushort test = 1;
            HasRequiredParameter(test, "test", "--test");
            Assert.Equal(0, ExitCode);

            test = 0;
            HasRequiredParameter(test, "test", "--test");
            Assert.Equal((int)ExitCodes.MissingCommandLineOption, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests the has required date functionality
        /// </summary>
        public void HasRequiredDateTest()
        {
            // missing
            HasRequiredDate(null, "test", "--test");
            Assert.Equal((int)ExitCodes.MissingCommandLineOption, ExitCode);
            ExitCode = 0;

            // correct
            HasRequiredDate("2015-12-10", "test", "--test");
            Assert.Equal(0, ExitCode);

            // incorrect
            HasRequiredDate("not-a-date", "test", "--test");
            Assert.Equal((int)ExitCodes.BadArguments, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests the has required date functionality
        /// </summary>
        public void HasOneOptionSelectedTest()
        {
            // false/false
            HasOneOptionSelected(false, "--test1", false, "--test2");
            Assert.Equal((int)ExitCodes.BadArguments, ExitCode);
            ExitCode = 0;

            // false/true
            HasOneOptionSelected(false, "--test1", true, "--test2");
            Assert.Equal(0, ExitCode);

            // true/false
            HasOneOptionSelected(true, "--test1", false, "--test2");
            Assert.Equal(0, ExitCode);

            // true/true
            HasOneOptionSelected(true, "--test1", true, "--test2");
            Assert.Equal((int)ExitCodes.BadArguments, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests the check non-zero functionality
        /// </summary>
        public void CheckNonZeroTest()
        {
            // zero
            CheckNonZero(0, "test");
            Assert.Equal((int)ExitCodes.MissingCommandLineOption, ExitCode);
            ExitCode = 0;

            // non-zero
            CheckNonZero(1, "test");
            Assert.Equal(0, ExitCode);
        }

        /// <summary>
        /// tests the has only one option functionality
        /// </summary>
        public void HasOnlyOneOptionTest()
        {
            // zero
            HasOnlyOneOption(0, "--test");
            Assert.Equal((int)ExitCodes.BadArguments, ExitCode);
            ExitCode = 0;

            // one
            HasOnlyOneOption(1, "--test");
            Assert.Equal(0, ExitCode);

            // more than one
            HasOnlyOneOption(2, "--test");
            Assert.Equal((int)ExitCodes.BadArguments, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests execute without any command-line arguments
        /// </summary>
        public void ExecuteNoArgsTest()
        {
            Execute(null);
            Assert.Equal((int)ExitCodes.MissingCommandLineOption, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests execute with unsupported command-line arguments
        /// </summary>
        public void ExecuteUnsupportedOptionsTest()
        {
            var args = "--out bob".Split(' ');
            Execute(args);
            Assert.Equal((int)ExitCodes.UnknownCommandLineOption, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests execute that triggers an exception in the command-line parser
        /// </summary>
        public void ExecuteOptionExceptionTest()
        {
            var args = "--num bob".Split(' ');
            Execute(args);
            Assert.Equal((int)ExitCodes.UnknownCommandLineOption, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// makes sure that our configuration settings are set properly
        /// </summary>
        public void ConfigurationSettings()
        {
            var args = "--id oscar --if charlie --num 123".Split(' ');
            Execute(args);
            Assert.Equal("oscar",   ConfigurationSettingsExample.InputDirectory);
            Assert.Equal("charlie", ConfigurationSettingsExample.InputFilename);
            Assert.Equal(123,       ConfigurationSettingsExample.Number);
            ExitCode = 0;
        }

        /// <summary>
        /// tests execute that outputs the version info
        /// </summary>
        public void ExecuteVersionTest()
        {
            var args = "--if bob --version".Split(' ');
            Execute(args);
            Assert.Equal(0, ExitCode);
        }

        /// <summary>
        /// tests execute that outputs the help info
        /// </summary>
        public void ExecuteHelpTest()
        {
            var args = "--if bob --help".Split(' ');
            Execute(args);
            Assert.Equal(0, ExitCode);
        }

        /// <summary>
        /// tests execute when parsing errors exist
        /// </summary>
        public void ExecuteParsingErrorsTest()
        {
            var args = "--if bob".Split(' ');
            AddParsingErrors();
            Execute(args);
            Assert.NotEqual(0, ExitCode);
            ExitCode = 0;
        }

        /// <summary>
        /// tests execute when parsing errors exist
        /// </summary>
        public void ExecuteExceptionTest()
        {
            var args = "--if bob".Split(' ');
            _throwException = true;
            Execute(args);

            Assert.Equal((int)ExitCodes.MissingCompressionLibrary, ExitCode);
            Assert.True(_executed);

            ExitCode = 0;
            _throwException = false;
            _executed = false;
        }

        /// <summary>
        /// tests execute when parsing errors exist
        /// </summary>
        public void ExecuteTest()
        {
            var args = "--if bob".Split(' ');
            Execute(args);

            Assert.Equal(0, ExitCode);
            Assert.True(_executed);

            _executed = false;
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            _executed = true;
            if (_throwException) throw new MissingCompressionLibraryException("The compression library is missing.");
        }
    }

    public static class ConfigurationSettingsExample
    {
        #region members

        public static string InputDirectory;
        public static string InputFilename;
        public static int Number;

        #endregion
    }
}
