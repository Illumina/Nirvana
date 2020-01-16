using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.Revel
{
    public static class Create
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
                    "input REVEL file path",
                    v => _inputFile = v
                },
                {
                    "out|o=",
                    "output directory",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .CheckInputFilenameExists(_inputFile, "input REVEL file Path", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Create a supplementary database from REVEL input file ", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            var outFileName = $"{version.Name}_{version.Version}";
            using (var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference)))
            using (var streamReader = GZipUtilities.GetAppropriateStreamReader(_inputFile))
            using (var revelReader = new RevelReader(streamReader, referenceProvider.RefNameToChromosome))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var indexWriter = new ExtendedBinaryWriter(indexStream))
            using  (var nsaWriter = new NsaWriter(new ExtendedBinaryWriter(nsaStream), indexWriter, version, referenceProvider, SaCommon.RevelTag, true, false, SaCommon.SchemaVersion, false))
            {
                nsaWriter.Write(revelReader.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}