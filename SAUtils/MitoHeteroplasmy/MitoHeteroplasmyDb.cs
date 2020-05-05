using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;

namespace SAUtils.MitoHeteroplasmy
{
    public static class MitoHeteroplasmyDb
    {
        private static string _inputFile;
        private static string _outputDirectory;
        private const string OutFileName = "MitoHeteroplasmy.tsv";
        private const string HeaderLine = "#POS\tREF\tALT\tVRFs\tAlleleDepths";

        public static ExitCodes Run(string command, string[] commandArgs)

        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input BED file path",
                    v => _inputFile = v
                },
                {
                    "out|o=",
                    "output directory",
                    v => _outputDirectory = v
                }
            };

            string commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_inputFile, "Mitochondrial Heteroplasmy BED file", "--in")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a TSV file with mitochondrial heteroplasmy information", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            using var mitoHeteroplasmyParser = new MitoHeteroplasmyParser(GZipUtilities.GetAppropriateReadStream(_inputFile));
            using var tsvStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, OutFileName));
            using var tsvWriter = new StreamWriter(tsvStream);
            tsvWriter.WriteLine(HeaderLine);
            foreach(var line in mitoHeteroplasmyParser.GetOutputLines())
                tsvWriter.WriteLine(line);

            return ExitCodes.Success;
        }
    }
}