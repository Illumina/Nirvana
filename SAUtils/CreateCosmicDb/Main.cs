using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.Cosmic;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CreateCosmicDb
{
    public static class Main
    {
        private static string _vcfFile;
        private static string _tsvFile;
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
                    "COSMIC VCF file",
                    v => _vcfFile = v
                },
                {
                    "tsv|t=",
                    "COSMIC TSV file",
                    v => _tsvFile = v
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
                .HasRequiredParameter(_vcfFile, "COSMIC VCF file", "--in")
                .CheckInputFilenameExists(_vcfFile, "COSMIC VCF file", "--in")
                .HasRequiredParameter(_tsvFile, "COSMIC TSV file", "--tsv")
                .CheckInputFilenameExists(_tsvFile, "COSMIC TSV file", "--tsv")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with COSMIC annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            var cosmicReader = new MergedCosmicReader(_vcfFile, _tsvFile, referenceProvider);
            var version = DataSourceVersionReader.GetSourceVersion(_vcfFile + ".version");

            string outFileName = $"{version.Name}_{version.Version}";
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var nsaWriter = new NsaWriter(new ExtendedBinaryWriter(nsaStream), new ExtendedBinaryWriter(indexStream), version, referenceProvider, SaCommon.CosmicTag, false, true, SaCommon.SchemaVersion, false))
            {
                nsaWriter.Write(cosmicReader.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}