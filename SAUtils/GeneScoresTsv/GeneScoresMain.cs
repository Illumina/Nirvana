using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using SAUtils.InputFileParsers;
using SAUtils.TsvWriters;

namespace SAUtils.GeneScoresTsv
{
    public sealed class GeneScoresMain
    {
        private  static string _inputPath;
        private  static string _outputDirectory;
        private const string JsonKeyName = "exac";
        private static ExitCodes ProgramExecution()
        {
            var geneScoreCreator= new GeneScoreTsvCreator(GZipUtilities.GetAppropriateStreamReader(_inputPath), 
                new GeneAnnotationTsvWriter(_outputDirectory, 
                DataSourceVersionReader.GetSourceVersion(_inputPath+".version"), null, 0, JsonKeyName, false));

            return geneScoreCreator.Create();
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var creator = new GeneScoresMain();

            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input gene scores {path}",
                    v => _inputPath = v
                },
                {
                    "out|o=",
                    "output directory {path}",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_inputPath, "gene scores file", "--in")
                .CheckDirectoryExists(_outputDirectory, "Output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided OMIM data files and populates tsv file", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
    }
}