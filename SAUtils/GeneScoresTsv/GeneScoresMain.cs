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
        private const string JsonKeyName = "exac";
        private static ExitCodes ProgramExecution()
        {
            var geneScoreCreator= new GeneScoreTsvCreator(GZipUtilities.GetAppropriateStreamReader(ConfigurationSettings.InputPath), 
                new GeneAnnotationTsvWriter(ConfigurationSettings.OutputDirectory, 
                DataSourceVersionReader.GetSourceVersion(ConfigurationSettings.InputPath+".version"), null, 0, JsonKeyName, false));

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
                    v => ConfigurationSettings.InputPath = v
                },
                {
                    "out|o=",
                    "output directory {path}",
                    v => ConfigurationSettings.OutputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(ConfigurationSettings.InputPath, "gene scores file", "--in")
                .CheckDirectoryExists(ConfigurationSettings.OutputDirectory, "Output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided OMIM data files and populates tsv file", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
    }
}