using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using SAUtils.GeneIdentifiers;
using static System.Environment;

namespace SAUtils.Omim
{
    public static class Downloader
    {
        private static string _apiKey;
        private static string _universalGeneArchivePath;
        private static string _outputDirectory;
        private static string _inputReferencePath;
        private const string OmimApiKeyEnvironmentVariableName = "OmimApiKey";

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "uga|u=",
                    "universal gene archive {path}",
                    v => _universalGeneArchivePath = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => _inputReferencePath = v
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
                .CheckInputFilenameExists(_universalGeneArchivePath, "universal gene archive", "--uga")
                .SkipBanner()
                .ShowHelpMenu("Download the OMIM gene annotation data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            _apiKey = GetEnvironmentVariable(OmimApiKeyEnvironmentVariableName);
            if (_apiKey == null) throw new InvalidDataException("Please set the OMIM API key as the environment variable \"OmimApiKey\".");

            var (entrezGeneIdToSymbol, ensemblGeneIdToSymbol) = GeneUtilities.ParseUniversalGeneArchive(_inputReferencePath, _universalGeneArchivePath);
            var geneSymbolUpdater = new GeneSymbolUpdater(entrezGeneIdToSymbol, ensemblGeneIdToSymbol);

            using (var omimQuery = new OmimQuery(_apiKey, _outputDirectory))
            {
                omimQuery.GenerateMimToGeneSymbolFile(geneSymbolUpdater);
                omimQuery.GenerateJsonResponse();
            }
            OmimVersion.WriteToFile(OmimQuery.JsonResponseFile, _outputDirectory);

            geneSymbolUpdater.DisplayStatistics();
            return ExitCodes.Success;
        }
    }
}