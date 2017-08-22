using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine.Utilities;
using ErrorHandling;

namespace CommandLine.Builders
{
    public sealed class TopLevelAppBuilder : ITopLevelAppBuilder
    {
        private readonly ITopLevelAppBuilderData _data;

        public TopLevelAppBuilder(string[] args, Dictionary<string, TopLevelOption> ops)
        {
            _data = new TopLevelAppBuilderData(args, ops);
        }

        public ITopLevelAppValidator Parse()
        {
            if (!_data.HasArguments)
            {
                _data.ExitCode     = ExitCodes.MissingCommandLineOption;
                _data.ShowHelpMenu = true;
                return new TopLevelAppValidator(_data);
            }

            _data.ExecuteMethod = GetExecuteMethod(_data.Command);

            return new TopLevelAppValidator(_data);
        }

        private Func<string, string[], ExitCodes> GetExecuteMethod(string command)
        {
            var lowerDict = new Dictionary<string, TopLevelOption>();
            foreach (var kvp in _data.Ops) lowerDict[kvp.Key.ToLower()] = kvp.Value;

            TopLevelOption topLevelOption;
            if (lowerDict.TryGetValue(command, out topLevelOption)) return topLevelOption.CommandMethod;

            _data.AddError($"An unrecognized command '{_data.Command}' was specified.", ExitCodes.UnknownCommandLineOption);
            return null;
        }
    }

    public sealed class TopLevelAppValidator : ITopLevelAppValidator
    {
        private readonly ITopLevelAppBuilderData _data;

        public TopLevelAppValidator(ITopLevelAppBuilderData data) => _data = data;

        public ITopLevelAppBanner ShowBanner(string authors)
        {
            if(_data.ShowHelpMenu || _data.Errors.Count >0)
                CommandLineUtilities.DisplayBanner(authors);
            return new TopLevelAppBanner(_data);
        }
    }

    public sealed class TopLevelAppBanner : ITopLevelAppBanner
    {
        private readonly ITopLevelAppBuilderData _data;

        public TopLevelAppBanner(ITopLevelAppBuilderData data) => _data = data;

        public ITopLevelAppHelpMenu ShowHelpMenu(string description)
        {
            if (_data.ShowHelpMenu || _data.Errors.Count > 0)
            {
                Console.WriteLine(description);
                Console.WriteLine();

                OutputHelper.WriteLabel("USAGE: ");

                Console.WriteLine($"dotnet {CommandLineUtilities.CommandFileName} <command> [options]");
                Console.WriteLine();

                DisplayCommands(_data.Ops);
            }

            return new TopLevelAppHelpMenu(_data);
        }

        private static void DisplayCommands(Dictionary<string, TopLevelOption> ops)
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

        private static int GetMaxCommandLen(IEnumerable<string> ops)
        {
            return ops.Select(op => op.Length).Concat(new int[1]).Max();
        }
    }

    public sealed class TopLevelAppHelpMenu : ITopLevelAppHelpMenu
    {
        private readonly ITopLevelAppBuilderData _data;

        public TopLevelAppHelpMenu(ITopLevelAppBuilderData data) => _data = data;

        public ITopLevelAppErrors ShowErrors()
        {
            if (_data.Errors.Count > 0)
            {
                Console.WriteLine("\nSome problems were encountered when parsing the command line options:");
                PrintErrors();
            }

            return new TopLevelAppErrors(_data);
        }

        private void PrintErrors()
        {
            foreach (var error in _data.Errors)
            {
                Console.Write("- ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                Console.ResetColor();
                Console.WriteLine(error);
            }
        }
    }

    public sealed class TopLevelAppErrors : ITopLevelAppErrors
    {
        private readonly ITopLevelAppBuilderData _data;
        private bool Continue => _data.ExitCode == ExitCodes.Success && _data.HasArguments && !_data.ShowHelpMenu;

        public TopLevelAppErrors(ITopLevelAppBuilderData data) => _data = data;

        public ExitCodes Execute()
        {
            if (!Continue) return _data.ExitCode;

            var benchmark = new Benchmark();
            ExitCodes exitCode;

            try
            {
                exitCode = _data.ExecuteMethod(_data.Command, _data.Arguments);
                ShowPerformanceData(benchmark);
            }
            catch (Exception e)
            {
                exitCode = ExitCodeUtilities.ShowException(e);
            }

            return exitCode;
        }

        private static void ShowPerformanceData(Benchmark benchmark)
        {
            var peakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            var wallTimeSpan = benchmark.GetElapsedTime();

            Console.WriteLine();
            if (peakMemoryUsageBytes > 0) Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(peakMemoryUsageBytes));
            Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(wallTimeSpan));
        }
    }

    public sealed class TopLevelAppBuilderData : ITopLevelAppBuilderData
    {
        public string[] Arguments { get; }
        public Dictionary<string, TopLevelOption> Ops { get; }
        public bool HasArguments => Arguments != null && Arguments.Length > 0;
        public string Command { get; }

        public List<string> Errors { get; } = new List<string>();
        public ExitCodes ExitCode { get; set; }
        
        public bool ShowHelpMenu { get; set; }
        public Func<string, string[], ExitCodes> ExecuteMethod { get; set; }
        
        
        public TopLevelAppBuilderData(string[] arguments, Dictionary<string, TopLevelOption> ops)
        {
            Arguments = arguments;
            Ops       = ops;
            Command   = HasArguments ? arguments[0].ToLower() : null;
        }

        public void AddError(string errorMessage, ExitCodes exitCode)
        {
            ExitCode = exitCode;
            Errors.Add(errorMessage);
        }
    }
}
