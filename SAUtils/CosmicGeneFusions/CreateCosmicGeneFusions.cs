using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.CosmicGeneFusions
{
    public static class CreateCosmicGeneFusions
    {
        private static string _transcriptCache37Path;
        private static string _transcriptCache38Path;
        private static string _dataDirectory;
        private static string _reference38Path;
        private static string _outputDirectory;
        
        private static ExitCodes ProgramExecution()
        {
            // var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            // var cosmicReader      = new MergedCosmicReader(_vcfFile, _tsvFile, referenceProvider);
            // var version           = DataSourceVersionReader.GetSourceVersion(_vcfFile + ".version");
            //
            // string outFileName = $"{version.Name}_{version.Version}";
            // using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            // using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSuffix)))
            // using (var nsaWriter = new NsaWriter(nsaStream, indexStream, version, referenceProvider, SaCommon.CosmicTag, false, true, SaCommon.SchemaVersion, false))
            // {
            //     nsaWriter.Write(cosmicReader.GetItems());
            // }

            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "cache37=",
                    "transcript cache {path} for GRCh37",
                    v => _transcriptCache37Path = v
                },
                {
                    "cache38=",
                    "transcript cache {path} for GRCh38",
                    v => _transcriptCache38Path = v
                },
                {
                    "in|i=",
                    "FusionCatcher data {directory}",
                    v => _dataDirectory = v
                },
                {
                    "out|o=",
                    "output {directory}",
                    v => _outputDirectory = v
                },
                {
                    "ref|r=",
                    "input reference sequence {path} for GRCh38",
                    v => _reference38Path = v
                }
            };

            var commandLineExample = $"{command} [options]";

            ExitCodes exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_reference38Path,       "reference sequence (GRCh38)", "--ref")
                .CheckInputFilenameExists(_transcriptCache37Path, "transcript cache (GRCh37)",   "--cache37")
                .CheckInputFilenameExists(_transcriptCache38Path, "transcript cache (GRCh38)",   "--cache38")
                .CheckDirectoryExists(_dataDirectory,   "FusionCatcher data directory", "--in")
                .CheckDirectoryExists(_outputDirectory, "output directory",             "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with FusionCatcher annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
    }
}