using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.Gme;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CreateGmeDb
{
    public static class Main
    {
        private static string _inputFile;
        private static string _compressedReference;
        private static string _outputDirectory;
        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _compressedReference = v
                 },
                {
                    "in|i=",
                    "input TSV file path",
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
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_inputFile, "GME TSV file", "--in")
                .CheckInputFilenameExists(_inputFile, "GME TSV file", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database containing 1000 Genomes allele frequencies", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            var version           = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            
            string outFileName = $"{version.Name}_{version.Version}";
            using (var gmeReader = new GmeParser(GZipUtilities.GetAppropriateStreamReader(_inputFile), referenceProvider))
            using (var nsaStream    = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream  = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSuffix)))
            using (var nsaWriter    = new NsaWriter(nsaStream, indexStream, version, referenceProvider, SaCommon.GmeTag, true, false, SaCommon.SchemaVersion, false))
            {
                nsaWriter.Write(gmeReader.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}

