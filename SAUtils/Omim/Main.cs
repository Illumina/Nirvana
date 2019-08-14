using System.IO;
using System.IO.Compression;
using Amazon.Runtime;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using IO;
using VariantAnnotation.SA;
using static System.Environment;

namespace SAUtils.Omim
{
    public static class Main
    {
        private static string _apiKey;
        private static string _universalGeneArchivePath;
        private static string _outputDirectory;
        private static string _inputReferencePath;

        private const string OmimDumpFileBaseName = "OMIM_dump_";
        private const string OmimDumpFileSuffix = ".zip";
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
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference", "--ref")
                .CheckInputFilenameExists(_universalGeneArchivePath, "universal gene archive", "--uga")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from OMIM data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            _apiKey = GetEnvironmentVariable(OmimApiKeyEnvironmentVariableName);
            if (_apiKey == null) throw new InvalidDataException("Please set the OMIM API key as the environment variable \"OmimApiKey\".");

            var version = OmimVersion.GetVersion();
            string outFileName = $"{version.Name}_{version.Version}";
            string dumpFilePath = Path.Combine(_outputDirectory, OmimDumpFileBaseName + version.Version + OmimDumpFileSuffix);

            var (entrezGeneIdToSymbol, ensemblGeneIdToSymbol) = OmimUtilities.ParseUniversalGeneArchive(_inputReferencePath, _universalGeneArchivePath);
            var geneSymbolUpdater = new GeneSymbolUpdater(entrezGeneIdToSymbol, ensemblGeneIdToSymbol);

            var omimSchema = OmimSchema.Get();

            using (var omimParser = new OmimParser(geneSymbolUpdater, omimSchema, _apiKey, dumpFilePath))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.NgaFileSuffix)))
            using (var ngaWriter = new NgaWriter(nsaStream, version, SaCommon.OmimTag, SaCommon.SchemaVersion, true))
            using (var saJsonSchemaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.NgaFileSuffix + SaCommon.JsonSchemaSuffix)))
            using (var schemaWriter = new StreamWriter(saJsonSchemaStream))
            {
                var omimItems = omimParser.GetItems();
                var geneToItems = OmimUtilities.GetGeneToOmimEntriesAndSchema(omimItems);
                ngaWriter.Write(geneToItems);
                schemaWriter.Write(omimSchema);
            }

            geneSymbolUpdater.DisplayStatistics();
            using (var writer =new StreamWriter(FileUtilities.GetCreateStream("UpdatedGeneSymbols.txt")))
            {
                geneSymbolUpdater.WriteUpdatedGeneSymbols(writer);
            }
            
            return ExitCodes.Success;
        }
    }
}