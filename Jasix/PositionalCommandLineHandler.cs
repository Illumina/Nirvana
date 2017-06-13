using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using CommandLine.VersionProvider;
using ErrorHandling.DataStructures;
using ErrorHandling.Utilities;
using VariantAnnotation.Utilities;

namespace Jasix
{
    public abstract class PositionalCommandLineHandler
    {
        #region members

        private int _exitCode;
        private bool _showHelpMenu;
        private bool _showVersion;

        private readonly OptionSet _commandLineOps;
        private readonly string _commandLineExample;
        private readonly string _programDescription;
        private readonly string _programAuthors;

        private readonly StringBuilder _errorBuilder;
        private readonly string _errorSpacer;

        private readonly IVersionProvider _versionProvider;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        protected PositionalCommandLineHandler(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null)
        {
            _programDescription = programDescription;
            _programAuthors = programAuthors;
            _commandLineOps = ops;
            _commandLineExample = commandLineExample;


            // add the help and version options
            if (ops.Any(x => x.Names.Contains("h")))
            {
                ops.Add("help", "displays the help menu", v => _showHelpMenu = v != null);
            }
            else
            {
                ops.Add("h|help", "displays the help menu", v => _showHelpMenu = v != null);
            }


            ops.Add("v|version", "displays the version", v => _showVersion = v != null);

            // use the Nirvana version provider by default
            if (versionProvider == null) versionProvider = new NirvanaVersionProvider();
            _versionProvider = versionProvider;

            _errorBuilder = new StringBuilder();
            _errorSpacer = new string(' ', 7);
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
        /// sets the exit code
        /// </summary>
        private void SetExitCode(ExitCodes exitCode)
        {
            if (_exitCode == 0) _exitCode = (int)exitCode;
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
        /// 

        protected abstract void ParseArguments(string[] args, out string[] updatedArgs, out int positionalArgsCount);

        protected void Execute(string[] args)
        {
            int positionalArgsCount;
            string[] updatedArgs;

            ParseArguments(args, out updatedArgs, out positionalArgsCount);

            try
            {
                List<string> unsupportedOps = null;

                if ((args == null || args.Length == 0) && positionalArgsCount == 0)
                {
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                    _showHelpMenu = true;
                }
                else
                {
                    try
                    {
                        unsupportedOps = _commandLineOps.Parse(updatedArgs);

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
                        PrintHelpMessage(unsupportedOps);
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
                _exitCode = ExitCodeUtilities.ShowException(e);
            }

        }

        private void PrintHelpMessage(List<string> unsupportedOps)
        {
            CommandLineUtilities.DisplayBanner(_programAuthors);
            Help.Show(_commandLineOps, _commandLineExample, _programDescription);

            if (_showVersion)
            {
                Console.WriteLine("{0} {1}", _versionProvider.GetProgramVersion(), _versionProvider.GetDataVersion());
                SetExitCode(ExitCodes.Success);
            }
            CommandLineUtilities.ShowUnsupportedOptions(unsupportedOps);

            Console.WriteLine();
            Console.WriteLine(_versionProvider.GetDataVersion());
            Console.WriteLine();

        }

        /// <summary>
        /// returns true if command-line parsing errors were found
        /// </summary>
        private bool FoundParsingErrors()
        {
            // print the errors if any were found
            if (_exitCode == (int)ExitCodes.Success) return false;

            Console.WriteLine("Some problems were encountered when parsing the command line options:");
            Console.WriteLine("{0}", _errorBuilder);
            Console.WriteLine("For a complete list of command line options, type \"{0} --help\"", Path.GetFileName(Environment.GetCommandLineArgs()[0]));

            return true;
        }
    }
}