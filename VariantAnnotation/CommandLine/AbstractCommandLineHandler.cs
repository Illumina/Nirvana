using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ErrorHandling.DataStructures;
using ErrorHandling.Utilities;
using NDesk.Options;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.CommandLine
{
    public abstract class AbstractCommandLineHandler
    {
        #region members

        public int ExitCode;
        private bool _showHelpMenu;
        private bool _showVersion;

        private readonly OptionSet _commandLineOps;
        private readonly string _commandLineExample;
        private readonly string _programDescription;
        private readonly string _programAuthors;

        protected readonly StringBuilder ErrorBuilder;
        protected readonly string ErrorSpacer;

        private readonly IVersionProvider _versionProvider;

        protected long PeakMemoryUsageBytes;
        protected TimeSpan WallTimeSpan;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        protected AbstractCommandLineHandler(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null)
        {
            _programDescription = programDescription;
            _programAuthors     = programAuthors;
            _commandLineOps     = ops;
            _commandLineExample = commandLineExample;

            // add the help and version options
            ops.Add("help|h", "displays the help menu", v => _showHelpMenu = v != null);
            ops.Add("version|v", "displays the version", v => _showVersion = v != null);

            // use the Nirvana version provider by default
            if (versionProvider == null) versionProvider = new NirvanaVersionProvider();
            _versionProvider = versionProvider;

            ErrorBuilder = new StringBuilder();
            ErrorSpacer  = new string(' ', 7);
        }

        /// <summary>
        /// checks if an input directory exists
        /// </summary>
        protected void CheckDirectoryExists(string directoryPath, string description, string commandLineOption, bool isRequired = true)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                if (isRequired)
                {
                    ErrorBuilder.AppendFormat("{0}ERROR: The {1} directory was not specified. Please use the {2} parameter.\n", ErrorSpacer, description, commandLineOption);
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                }
            }
            else if (!Directory.Exists(directoryPath))
            {
                ErrorBuilder.AppendFormat("{0}ERROR: The {1} directory ({2}) does not exist.\n", ErrorSpacer, description, directoryPath);
                SetExitCode(ExitCodes.PathNotFound);
            }
        }

		protected void CheckAndCreateDirectory(string directoryPath, string description, string commandLineOption, bool isRequired = true)
		{
			if (string.IsNullOrEmpty(directoryPath))
			{
				if (isRequired)
				{
					ErrorBuilder.AppendFormat("{0}ERROR: The {1} directory was not specified. Please use the {2} parameter.\n", ErrorSpacer, description, commandLineOption);
					SetExitCode(ExitCodes.MissingCommandLineOption);
				}
			}
			else if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
		}

		/// <summary>
		/// checks if an input file exists
		/// </summary>
		protected void CheckInputFilenameExists(string filePath, string description, string commandLineOption, bool isRequired = true)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                if (isRequired)
                {
                    ErrorBuilder.AppendFormat("{0}ERROR: The {1} file was not specified. Please use the {2} parameter.\n", ErrorSpacer, description, commandLineOption);
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                }
            }
            else if (!File.Exists(filePath))
            {
                ErrorBuilder.AppendFormat("{0}ERROR: The {1} file ({2}) does not exist.\n", ErrorSpacer, description, filePath);
                SetExitCode(ExitCodes.FileNotFound);
            }
        }

        /// <summary>
        /// checks if the required parameter has been set
        /// </summary>
        protected void HasRequiredParameter<T>(T parameterValue, string description, string commandLineOption)
        {
            if (EqualityComparer<T>.Default.Equals(parameterValue, default(T)))
            {
                ErrorBuilder.AppendFormat("{0}ERROR: The {1} was not specified. Please use the {2} parameter.\n", ErrorSpacer, description, commandLineOption);
                SetExitCode(ExitCodes.MissingCommandLineOption);
            }
        }

        /// <summary>
        /// checks if the required parameter has been set
        /// </summary>
        protected void HasRequiredDate(string date, string description, string commandLineOption)
        {
            if (date == null)
            {
                ErrorBuilder.AppendFormat("{0}ERROR: The {1} was not specified. Please use the {2} parameter.\n", ErrorSpacer, description, commandLineOption);
                SetExitCode(ExitCodes.MissingCommandLineOption);
            }
            else
            {
                DateTime result;
                if (!DateTime.TryParse(date, out result))
                {
                    ErrorBuilder.AppendFormat("{0}ERROR: {1} was not specified as a date (YYYY-MM-dd). Please use the {2} parameter.\n", ErrorSpacer, description, commandLineOption);
                    SetExitCode(ExitCodes.BadArguments);
                }   
            }
        }

        /// <summary>
        /// checks if one and only one option has been selected
        /// </summary>
        protected void HasOneOptionSelected(bool a, string aCommandLineOption, bool b, string bCommandLineOption)
        {
            if ((!a && !b) || (a && b))
            {
                ErrorBuilder.AppendFormat("{0}ERROR: Either the {1} or the {2} option should be selected.\n", ErrorSpacer, aCommandLineOption, bCommandLineOption);
                SetExitCode(ExitCodes.BadArguments);
            }
        }

        /// <summary>
        /// checks that a non-zero integer was supplied
        /// </summary>
        protected void CheckNonZero(int num, string description)
        {
            if (num == 0)
            {
                ErrorBuilder.AppendFormat("{0}ERROR: At least one {1} should be provided.\n", ErrorSpacer, description);
                SetExitCode(ExitCodes.MissingCommandLineOption);
            }
        }

        /// <summary>
        /// checks that only one option was specified
        /// </summary>
        protected void HasOnlyOneOption(int num, string possibleCommandLineOptions)
        {
            if (num == 0)
            {
                ErrorBuilder.AppendFormat("{0}ERROR: One option must be selected: {1}.\n", ErrorSpacer, possibleCommandLineOptions);
                SetExitCode(ExitCodes.BadArguments);
            }
            else if (num > 1)
            {
                ErrorBuilder.AppendFormat("{0}ERROR: Only one of the options can be selected: {1}.\n", ErrorSpacer, possibleCommandLineOptions);
                SetExitCode(ExitCodes.BadArguments);
            }
        }

        /// <summary>
        /// sets the exit code
        /// </summary>
        protected void SetExitCode(ExitCodes exitCode)
        {
            if (ExitCode == 0) ExitCode = (int)exitCode;
        }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected abstract void ValidateCommandLine();

        /// <summary>
        /// executes the program
        /// </summary>
        protected abstract void ProgramExecution();

        /// <summary>
        /// executes the command-line workflow
        /// </summary>
        public void Execute(string[] args)
        {
            var bench = new Benchmark();

            try
            {
                List<string> unsupportedOps = null;

                if (args == null || args.Length == 0)
                {
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                    _showHelpMenu = true;
                }
                else
                {
                    try
                    {
                        unsupportedOps = _commandLineOps.Parse(args);

                        if (unsupportedOps.Count > 0)
                        {
                            SetExitCode(ExitCodes.UnknownCommandLineOption);
                            _showHelpMenu = true;
                        }
                    }
                    catch (OptionException oe)
                    {
                        ErrorBuilder.AppendFormat("{0}ERROR: {1}\n", ErrorSpacer, oe.Message);
                        SetExitCode(ExitCodes.UnknownCommandLineOption);
                        _showHelpMenu = true;
                    }
                }

                if (_showVersion)
                {
                    Console.WriteLine("{0} {1}", _versionProvider.GetProgramVersion(), _versionProvider.GetDataVersion());
                    SetExitCode(ExitCodes.Success);
                }
                else
                {
                    CommandLineUtilities.DisplayBanner(_programAuthors);

                    if (_showHelpMenu)
                    {
                        Help.Show(_commandLineOps, _commandLineExample, _programDescription);

                        CommandLineUtilities.ShowUnsupportedOptions(unsupportedOps);

                        Console.WriteLine();
                        Console.WriteLine(_versionProvider.GetDataVersion());
                        Console.WriteLine();

                        // print the errors if any were found
                        if (FoundParsingErrors()) return;
                    }
                    else
                    {
                        ValidateCommandLine();

                        // print the errors if any were found
                        if (FoundParsingErrors()) return;

                        ProgramExecution();
                    }
                }
            }
            catch (Exception e)
            {
                ExitCode = ExitCodeUtilities.ShowException(e);
            }

            PeakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            WallTimeSpan         = bench.GetElapsedTime();

            if (!_showVersion && !_showHelpMenu)
            {
                Console.WriteLine();
                if(PeakMemoryUsageBytes > 0) Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(PeakMemoryUsageBytes));
                Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(WallTimeSpan));
            }
        }

        /// <summary>
        /// returns true if command-line parsing errors were found
        /// </summary>
        private bool FoundParsingErrors()
        {
            // print the errors if any were found
            if (ExitCode == (int) ExitCodes.Success) return false;

            Console.WriteLine("Some problems were encountered when parsing the command line options:");
            Console.WriteLine("{0}", ErrorBuilder);
            Console.WriteLine("For a complete list of command line options, type \"{0} -h\"", Path.GetFileName(Environment.GetCommandLineArgs()[0]));

            return true;
        }
    }
}
