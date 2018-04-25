using System;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.CreateTsvIndex
{
    public static class CreateTsvIndexMain
    {
        private static string _inputTsv;
        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input intermediate TSV file name",
                    v => _inputTsv = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(_inputTsv, "input intermediate TSV file name", "--in")
                .CheckInputFilenameExists(_inputTsv, "input intermediate TSV file name", "--in")
                .SkipBanner()
                .ShowHelpMenu("Reads provided iTSV and generates an index file (.tvi)", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            Console.WriteLine("Creating tsv index...");
            SaUtilsCommon.BuildTsvIndex(_inputTsv);
            
            return ExitCodes.Success;
        }
        
    }
}