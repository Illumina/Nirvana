using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateTopMedTsv
{
    public sealed class CreateTopMedTsvMain
    {
        private static string _compressedReferenceArg;
        private static string _inputFileArg;
        private static string _outputDirArg;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var creator = new CreateTopMedTsvMain();
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input VCF (and .version) file(s)",
                    v => _inputFileArg = v
                },
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _compressedReferenceArg = v
                 },
                {
                    "out|o=",
                    "output directory for TSVs",
                    v => _outputDirArg = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReferenceArg, "compressed reference sequence file name", "--ref")
                .CheckInputFilenameExists(_inputFileArg, "input file containing TOPMed allele frequencies", "--in")
                .HasRequiredParameter(_outputDirArg, "output directory name", "--out")
                .CheckDirectoryExists(_outputDirArg, "output directory name", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(creator.ProgramExecution);

            return exitCode;
        }

        private ExitCodes ProgramExecution()
        {
            var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileArg);
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferenceArg));

            var version = DataSourceVersionReader.GetSourceVersion(_inputFileArg+".version");
            var topMedTsvCreator = new TopMedTsvCreator(reader, referenceProvider, version, _outputDirArg);

            topMedTsvCreator.CreateTsvs();
            return ExitCodes.Success;
        }
    }
}