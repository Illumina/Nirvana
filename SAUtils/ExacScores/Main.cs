using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.SA;

namespace SAUtils.ExacScores
{
    public static class Main
    {
        private static string _outputDirectory;
        private static string _inputFile;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input tsv file",
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
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .CheckInputFilenameExists(_inputFile, "input TSV file", "--in")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from ExAC data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");

            string outFileName = $"{version.Name}_{version.Version}";

            //create universal gene archive
            using (var exacParser= new ExacScoresParser(GZipUtilities.GetAppropriateStreamReader(_inputFile)))
            using (var stream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.NgaFileSuffix)))
            using (var ngaWriter = new NgaWriter(stream, version, SaCommon.ExacScoreTag, SaCommon.SchemaVersion, false))
            {
                ngaWriter.Write(exacParser.GetItems());
            }

            return ExitCodes.Success;
        }

        

    }
}