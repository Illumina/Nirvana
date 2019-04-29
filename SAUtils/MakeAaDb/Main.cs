using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.MakeAaDb
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
                    "input VCF file path",
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
                .HasRequiredParameter(_inputFile, "OneK Gen VCFfile", "--in")
                .CheckInputFilenameExists(_inputFile, "OneK Gen VCFfile", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("create Ancestral allele database from 1000Genomes data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            
            string outFileName = $"{version.Name}_{version.Version}_ancestralAlleles".Replace(' ','_');
            using (var ancestralAlleleReader = new AncestralAlleleReader(GZipUtilities.GetAppropriateStreamReader(_inputFile), referenceProvider))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var writer = new NsaWriter(new ExtendedBinaryWriter(nsaStream), new ExtendedBinaryWriter(indexStream), version, referenceProvider, SaCommon.AncestralAlleleTag, true, false, SaCommon.SchemaVersion, true))
            {
                writer.Write(ancestralAlleleReader.GetItems());
            }

            return ExitCodes.Success;
        }
    }

}