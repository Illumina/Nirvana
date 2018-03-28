using System;
using System.Collections.Generic;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using CommandLine.VersionProviders;
using ErrorHandling;
using VariantAnnotation.Interface.Providers;

namespace CommandLine.Builders
{
    public sealed class ConsoleAppBuilder : IConsoleAppBuilder
    {
        private readonly IConsoleAppBuilderData _data;
        private readonly string[] _args;

        public ConsoleAppBuilder(string[] args, OptionSet ops)
        {
            _args = args;
            _data = new ConsoleAppBuilderData
            {
                Ops          = ops,
                HasArguments = _args != null && _args.Length > 0
            };

            AddAdditionalOptions();
        }

        private void AddAdditionalOptions()
        {
            _data.Ops.Add("help|h", "displays the help menu", v => _data.ShowHelpMenu = v != null);
            _data.Ops.Add("version|v", "displays the version", v => _data.ShowVersion = v != null);
        }

        public IConsoleAppValidator Parse()
        {
            if (!_data.HasArguments)
            {
                _data.ExitCode = ExitCodes.MissingCommandLineOption;
                _data.ShowHelpMenu = true;
                return new ConsoleAppValidator(_data);
            }

            try
            {
                _data.UnsupportedOps = _data.Ops.Parse(_args);

                if (_data.UnsupportedOps.Count > 0)
                {
                    _data.AddError($"Found unknown command-line option(s): {string.Join(", ", _data.UnsupportedOps)}",
                        ExitCodes.UnknownCommandLineOption);
                }
            }
            catch (OptionException oe)
            {
                _data.AddError(oe.Message, ExitCodes.UnknownCommandLineOption);
            }

            return new ConsoleAppValidator(_data);
        }

        public IConsoleAppBuilder UseVersionProvider(IVersionProvider versionProvider)
        {
            _data.VersionProvider = versionProvider;
            return this;
        }
    }

    public sealed class ConsoleAppValidator : IConsoleAppValidator
    {
        public IConsoleAppBuilderData Data { get; }
        public bool SkipValidation { get; }

        public ConsoleAppValidator(IConsoleAppBuilderData data)
        {
            Data           = data;
            SkipValidation = !data.HasArguments || data.ShowHelpMenu || data.ShowVersion;
        }

        public IConsoleAppValidator DisableOutput(bool condition = true)
        {
            if (condition) Data.DisableOutput = true;
            return this;
        }

        public IConsoleAppBanner ShowBanner(string authors)
        {
            if (Data.ShowVersion) Console.WriteLine($"{CommandLineUtilities.Title} {CommandLineUtilities.InformationalVersion} {Data.VersionProvider.DataVersion}");
            else if (!Data.DisableOutput) CommandLineUtilities.DisplayBanner(authors);
            return new ConsoleAppBanner(Data);
        }

        public IConsoleAppBanner SkipBanner() => new ConsoleAppBanner(Data);
    }

    public sealed class ConsoleAppBanner : IConsoleAppBanner
    {
        private readonly IConsoleAppBuilderData _data;

        public ConsoleAppBanner(IConsoleAppBuilderData data) => _data = data;

        public IConsoleAppHelpMenu ShowHelpMenu(string description, string commandLineExample)
        {
            // ReSharper disable once InvertIf
            if (_data.ShowHelpMenu || _data.Errors.Count > 0)
            {
                Help.Show(_data.Ops, commandLineExample, description);
                Console.WriteLine($"\n{_data.VersionProvider.DataVersion}\n");
            }

            return new ConsoleAppHelpMenu(_data);
        }
    }

    public sealed class ConsoleAppHelpMenu : IConsoleAppHelpMenu
    {
        private readonly IConsoleAppBuilderData _data;

        public ConsoleAppHelpMenu(IConsoleAppBuilderData data) => _data = data;

        public IConsoleAppErrors ShowErrors()
        {
            // ReSharper disable once InvertIf
            if (_data.Errors.Count > 0)
            {
                Console.WriteLine("\nSome problems were encountered when parsing the command line options:");
                PrintErrors();
                Console.WriteLine("\nFor a complete list of command line options, type \"dotnet {0} -h\"", CommandLineUtilities.CommandFileName);
            }

            return new ConsoleAppErrors(_data);
        }

        private void PrintErrors()
        {
            foreach (string error in _data.Errors)
            {
                Console.Write("- ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                Console.ResetColor();
                Console.WriteLine(error);
            }
        }
    }

    public sealed class ConsoleAppErrors : IConsoleAppErrors
    {
        private readonly IConsoleAppBuilderData _data;
        private bool Continue => _data.ExitCode == ExitCodes.Success && _data.HasArguments && !_data.ShowVersion && !_data.ShowHelpMenu;

        public ConsoleAppErrors(IConsoleAppBuilderData data) => _data = data;

        public ExitCodes Execute(Func<ExitCodes> executeMethod)
        {
            if (!Continue) return _data.ExitCode;

            var benchmark = new Benchmark();
            ExitCodes exitCode;

            try
            {
                exitCode = executeMethod();
                ShowPerformanceData(benchmark);
            }
            catch (Exception e)
            {
                exitCode = ExitCodeUtilities.ShowException(e);
            }

            return exitCode;
        }

        private void ShowPerformanceData(Benchmark benchmark)
        {
            if (_data.DisableOutput) return;

            long peakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            var wallTimeSpan          = benchmark.GetElapsedTime();

            Console.WriteLine();
            if (peakMemoryUsageBytes > 0) Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(peakMemoryUsageBytes));
            Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(wallTimeSpan));
        }
    }

    public sealed class ConsoleAppBuilderData : IConsoleAppBuilderData
    {
        public OptionSet Ops { get; set; }
        public List<string> UnsupportedOps { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public ExitCodes ExitCode { get; set; } = ExitCodes.Success;
        public bool DisableOutput { get; set; }
        public bool HasArguments { get; set; }
        public IVersionProvider VersionProvider { get; set; } = new DefaultVersionProvider();
        public bool ShowHelpMenu { get; set; }
        public bool ShowVersion { get; set; }

        public void AddError(string errorMessage, ExitCodes exitCode)
        {
            ExitCode = exitCode;
            Errors.Add(errorMessage);
        }
    }
}
