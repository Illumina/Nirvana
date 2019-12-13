using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.OneKGen;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.OneKGenSvDb
{
    public static class Create
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
                    "OneKGenSv BED file",
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
                .CheckInputFilenameExists(_inputFileName, "OneKGenSv BED file", "--in")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with 1000 Genome structural variant annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            var version = DataSourceVersionReader.GetSourceVersion(_inputFileName + ".version");

            string outFileName = $"{version.Name}_{version.Version}".Replace(' ','_');
            using(var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileName))
            using(var oneKGenSvReader = new OneKGenSvReader(reader, referenceProvider.RefNameToChromosome))
            using(var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SiFileSuffix)))
            using(var nsiWriter = new NsiWriter(nsaStream, version, referenceProvider.Assembly,
                SaCommon.OnekSvTag, ReportFor.StructuralVariants, SaCommon.SchemaVersion))
            {
                nsiWriter.Write(oneKGenSvReader.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}