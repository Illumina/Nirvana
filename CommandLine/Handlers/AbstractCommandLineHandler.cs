using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using CommandLine.VersionProvider;
using ErrorHandling.DataStructures;
using ErrorHandling.Utilities;

namespace CommandLine.Handlers
{
    public abstract class AbstractCommandLineHandler
    {
        #region members

	    protected int ExitCode;
        private bool _showHelpMenu;
        private bool _showVersion;

        protected bool DisableOutput;

        private readonly OptionSet _commandLineOps;
        private readonly string _commandLineExample;
        private readonly string _programDescription;
        private readonly string _programAuthors;

        private readonly string _errorSpacer;
        private readonly StringBuilder _errorBuilder;

        private readonly IVersionProvider _versionProvider;

        private long _peakMemoryUsageBytes;
        private TimeSpan _wallTimeSpan;

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
            if (versionProvider == null) versionProvider = new DefaultVersionProvider();
            _versionProvider = versionProvider;
            
            _errorSpacer  = new string(' ', 7);
            _errorBuilder = new StringBuilder();
        }

        protected void DisableConsoleOutput()
        {
            DisableOutput = true;
        }

        /// <summary>
        /// checks that the output filename has been specified and that it contains the appropriate suffix
        /// </summary>
        protected void CheckOutputFilenameSuffix(string filePath, string fileSuffix, string description)
        {
            // check that a path was specified
            if (string.IsNullOrEmpty(filePath))
            {
                _errorBuilder.AppendFormat("{0}ERROR: The {1} filename was not specified\n", _errorSpacer, description);
                SetExitCode(ExitCodes.MissingCommandLineOption);
                return;
            }

            // check that the path ends in the suffix
            if (!filePath.EndsWith(fileSuffix))
            {
                _errorBuilder.AppendFormat("{0}ERROR: The {1} filename ({2}) does not end with a {3}\n", _errorSpacer, description, Path.GetFileName(filePath), fileSuffix);
                SetExitCode(ExitCodes.MissingFilenameSuffix);
            }
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
                    _errorBuilder.AppendFormat("{0}ERROR: The {1} directory was not specified. Please use the {2} parameter.\n", _errorSpacer, description, commandLineOption);
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                }
            }
            else if (!Directory.Exists(directoryPath))
            {
                _errorBuilder.AppendFormat("{0}ERROR: The {1} directory ({2}) does not exist.\n", _errorSpacer, description, directoryPath);
                SetExitCode(ExitCodes.PathNotFound);
            }
        }

        /// <summary>
        /// checks if an input directory exists
        /// </summary>
        protected void CheckDirectoryContainsFiles(string directoryPath, string description, string commandLineOption,
            string searchPattern)
        {
            if (!Directory.Exists(directoryPath)) return;

            var files = Directory.GetFiles(directoryPath, searchPattern);
            if (files.Length != 0) return;

            _errorBuilder.Append($"{_errorSpacer}ERROR: The {description} directory ({directoryPath}) does not contain the required files ({searchPattern}). Please use the {commandLineOption} parameter.\n");
            SetExitCode(ExitCodes.FileNotFound);
        }

        protected void CheckAndCreateDirectory(string directoryPath, string description, string commandLineOption, bool isRequired = true)
		{
			if (string.IsNullOrEmpty(directoryPath))
			{
				if (isRequired)
				{
					_errorBuilder.AppendFormat("{0}ERROR: The {1} directory was not specified. Please use the {2} parameter.\n", _errorSpacer, description, commandLineOption);
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
                    _errorBuilder.AppendFormat("{0}ERROR: The {1} file was not specified. Please use the {2} parameter.\n", _errorSpacer, description, commandLineOption);
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                }
            }
            else if (!File.Exists(filePath))
            {
                _errorBuilder.AppendFormat("{0}ERROR: The {1} file ({2}) does not exist.\n", _errorSpacer, description, filePath);
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
                _errorBuilder.AppendFormat("{0}ERROR: The {1} was not specified. Please use the {2} parameter.\n", _errorSpacer, description, commandLineOption);
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
                _errorBuilder.AppendFormat("{0}ERROR: The {1} was not specified. Please use the {2} parameter.\n", _errorSpacer, description, commandLineOption);
                SetExitCode(ExitCodes.MissingCommandLineOption);
            }
            else
            {
                DateTime result;
                if (!DateTime.TryParse(date, out result))
                {
                    _errorBuilder.AppendFormat("{0}ERROR: {1} was not specified as a date (YYYY-MM-dd). Please use the {2} parameter.\n", _errorSpacer, description, commandLineOption);
                    SetExitCode(ExitCodes.BadArguments);
                }   
            }
        }

        /// <summary>
        /// checks if one and only one option has been selected
        /// </summary>
        protected void HasOneOptionSelected(bool a, string aCommandLineOption, bool b, string bCommandLineOption)
        {
            if (!a && !b || a && b)
            {
                _errorBuilder.AppendFormat("{0}ERROR: Either the {1} or the {2} option should be selected.\n", _errorSpacer, aCommandLineOption, bCommandLineOption);
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
                _errorBuilder.AppendFormat("{0}ERROR: At least one {1} should be provided.\n", _errorSpacer, description);
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
                _errorBuilder.AppendFormat("{0}ERROR: One option must be selected: {1}.\n", _errorSpacer, possibleCommandLineOptions);
                SetExitCode(ExitCodes.BadArguments);
            }
            else if (num > 1)
            {
                _errorBuilder.AppendFormat("{0}ERROR: Only one of the options can be selected: {1}.\n", _errorSpacer, possibleCommandLineOptions);
                SetExitCode(ExitCodes.BadArguments);
            }
        }

        /// <summary>
        /// sets the exit code
        /// </summary>
        private void SetExitCode(ExitCodes exitCode)
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
        protected void Execute(string[] args)
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
                        _errorBuilder.AppendFormat("{0}ERROR: {1}\n", _errorSpacer, oe.Message);
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
                    if (_showHelpMenu)
                    {
                        CommandLineUtilities.DisplayBanner(_programAuthors);
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

                        if (!DisableOutput) CommandLineUtilities.DisplayBanner(_programAuthors);
                        ProgramExecution();
                    }
                }
            }
            catch (Exception e)
            {
                ExitCode = ExitCodeUtilities.ShowException(e);
            }

            _peakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            _wallTimeSpan         = bench.GetElapsedTime();

            if (!_showVersion && !_showHelpMenu && !DisableOutput)
            {
                Console.WriteLine();
                if(_peakMemoryUsageBytes > 0) Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(_peakMemoryUsageBytes));
                Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(_wallTimeSpan));
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
            Console.WriteLine("{0}", _errorBuilder);
            Console.WriteLine("For a complete list of command line options, type \"dotnet {0} -h\"", Path.GetFileName(Environment.GetCommandLineArgs()[0]));

            return true;
        }
    }
}
