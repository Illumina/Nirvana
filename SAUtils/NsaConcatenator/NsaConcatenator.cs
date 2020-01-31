using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using System;
using System.IO;
using VariantAnnotation.SA;

namespace SAUtils.NsaConcatenator
{
    public static class NsaConcatenator
    {
        private static string _inputDir;
        private static string _outFileStub;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "dir|d=",
                    "input directory containing NSA (and index) files to be merged",
                    v => _inputDir = v
                },
                {
                    "out|o=",
                    "output NSA file stub",
                    v => _outFileStub = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckDirectoryExists(_inputDir, "input directory containing NSA files", "--in")
                .HasRequiredParameter(_outFileStub, "output NSA file stub", "--out")
                .SkipBanner()
                .ShowHelpMenu("Concatenate multiple (non-overlapping) NSA files from the same data source into one", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            Console.WriteLine($"Concatenating NSA files from {_inputDir}");

            ConcatUtilities.ConcatenateNsaFiles(Directory.GetFiles(_inputDir, $"*{SaCommon.SaFileSuffix}"), _outFileStub);
            
            return ExitCodes.Success;
        }
    }
}
