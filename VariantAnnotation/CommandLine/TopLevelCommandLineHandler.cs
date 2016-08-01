using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErrorHandling.DataStructures;
using ErrorHandling.Utilities;
using NDesk.Options;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.CommandLine
{
    public abstract class TopLevelCommandLineHandler
    {
        #region members

        protected int ExitCode;

        private bool _showHelpMenu;
        private bool _showVersion;

        private readonly Dictionary<string, TopLevelOption> _commandLineOps;

        private readonly string _executableName;        
        private readonly string _programDescription;
        private readonly string _programAuthors;

        private readonly StringBuilder _errorBuilder;
        private readonly string _errorSpacer;

        private readonly IVersionProvider _versionProvider;

        private long _peakMemoryUsageBytes;
        private TimeSpan _wallTimeSpan;

        #endregion

        protected TopLevelCommandLineHandler(string programDescription, string executableName, Dictionary<string, TopLevelOption> ops, string authors, IVersionProvider versionProvider = null)
        {
            _programDescription = programDescription;
            _programAuthors     = authors;
            _commandLineOps     = ops;
            _executableName     = executableName;

            if (versionProvider == null) versionProvider = new NirvanaVersionProvider();
            _versionProvider = versionProvider;

            _errorBuilder = new StringBuilder();
            _errorSpacer  = new string(' ', 7);
        }

        protected void ParseCommandLine(string[] args)
        {
            Benchmark benchmark = new Benchmark();

            try
            {
                _commandLineOps["version"] = new TopLevelOption("displays the version", null);

                string command = null;
                Func<string, string[], int> commandMethod = null;
                string unsupportedOp = null;

                if (args == null || args.Length == 0)
                {
                    SetExitCode(ExitCodes.MissingCommandLineOption);
                    _showHelpMenu = true;
                }
                else
                {
                    command = args[0].ToLower();
                    if (command == "version") _showVersion = true;

                    commandMethod = GetCommandMethod(command);
                    if (commandMethod == null) unsupportedOp = command;
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
                        ShowHelpMenu(unsupportedOp);
                    }
                    else
                    {
                        if (FoundParsingErrors()) return;
                        ExitCode = commandMethod(command, args.Skip(1).ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                ExitCode = ExitCodeUtilities.ShowException(e);
            }

            _peakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            _wallTimeSpan         = benchmark.GetElapsedTime();

            if (_showVersion || _showHelpMenu) return;

            Console.WriteLine();
            Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(_peakMemoryUsageBytes));
            Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(_wallTimeSpan));
        }

        private Func<string, string[], int> GetCommandMethod(string command)
        {
            var lowerDict = new Dictionary<string, TopLevelOption>();
            foreach (var kvp in _commandLineOps) lowerDict[kvp.Key.ToLower()] = kvp.Value;

            TopLevelOption topLevelOption;
            if (lowerDict.TryGetValue(command, out topLevelOption)) return topLevelOption.CommandMethod;

            _errorBuilder.AppendFormat("{0}ERROR: An unrecognized command '{1}' was specified.\n", _errorSpacer, command);
            SetExitCode(ExitCodes.UnknownCommandLineOption);
            return null;
        }

        private void ShowHelpMenu(string unsupportedOp)
        {
            Console.WriteLine(_programDescription);
            Console.WriteLine();

            OutputHelper.WriteLabel("USAGE: ");

            Console.WriteLine(string.Format("dotnet {0} <command> [options]", _executableName));
            Console.WriteLine();

            DisplayCommands(_commandLineOps);

            if (unsupportedOp != null) CommandLineUtilities.ShowUnsupportedOptions(new List<string> { unsupportedOp });

            Console.WriteLine();
            Console.WriteLine(_versionProvider.GetDataVersion());
        }

        private void DisplayCommands(Dictionary<string, TopLevelOption> ops)
        {
            string label  = "COMMAND: ";
            string filler = new string(' ', label.Length);

            int commandColumnLen = GetMaxCommandLen(ops.Keys) + 3;
            bool useLabel = true;

            foreach (var op in ops)
            {
                if (useLabel)
                {
                    OutputHelper.WriteLabel(label);
                    useLabel = false;
                }
                else Console.Write(filler);

                string commandFiller = new string(' ', commandColumnLen - op.Key.Length);
                Console.WriteLine(op.Key + commandFiller + op.Value.Description);
            }
        }

        private int GetMaxCommandLen(IEnumerable<string> ops)
        {
            return ops.Select(op => op.Length).Concat(new int[1]).Max();
        }

        private void SetExitCode(ExitCodes exitCode)
        {
            if (ExitCode != 0) return;
            ExitCode = (int)exitCode;
        }

        private bool FoundParsingErrors()
        {
            if (ExitCode == 0) return false;
            Console.WriteLine("Some problems were encountered when parsing the command line options:");
            Console.WriteLine("{0}", _errorBuilder);
            Console.WriteLine("For a complete list of command line options, type \"dotnet {0}\"", _executableName);
            return true;
        }
    }
}
