using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.ClinGen;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.MakeClinGenDb
{
    public static class Main
    {
        private static string _inputFileName;
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
                    "ClinGen VCFfile",
                    v => _inputFileName = v
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
                .HasRequiredParameter(_inputFileName, "ClinGen VCFfile", "--in")
                .CheckInputFilenameExists(_inputFileName, "ClinGen VCFfile", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with ClinVar annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            var version = DataSourceVersionReader.GetSourceVersion(_inputFileName + ".version");
            string outFileName = $"{version.Name}_{version.Version}";

            using (var clinGenReader = new ClinGenReader(GZipUtilities.GetAppropriateStreamReader(_inputFileName), referenceProvider.RefNameToChromosome))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SiFileSuffix)))
            using (var nsiWriter = new NsiWriter(new ExtendedBinaryWriter(nsaStream), version, referenceProvider.Assembly, SaCommon.ClinGenTag, ReportFor.StructuralVariants, SaCommon.SchemaVersion))
            {
                nsiWriter.Write(clinGenReader.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}