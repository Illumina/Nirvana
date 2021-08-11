using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.gnomAD
{
    public class LcrRegionsMain
    {
        private static string _referenceSequencePath;
        private static string _inputFile;
        private static string _outputDirectory;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _referenceSequencePath = v
                 },
                {
                    "in|i=",
                    "input file path (along with a .version file)",
                    v => _inputFile = v
                },
                {
                    "out|o=",
                    "output directory for NSI file",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_referenceSequencePath, "compressed reference sequence file name", "--ref")
                .CheckInputFilenameExists(_inputFile, "input file with LCR regions", "--ref")
                .CheckDirectoryExists(_outputDirectory, "output Supplementary directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var refProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_referenceSequencePath));
            var version     = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            var outFileName = $"{version.Name}_{version.Version}";
            
            using (var parser = new LcrRegionParser(GZipUtilities.GetAppropriateStreamReader(_inputFile), refProvider))
            using (var stream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.LcrFileSuffix)))
            using (var writer = new NsiWriter(stream, version, refProvider.Assembly, SaCommon.LowComplexityRegionTag, ReportFor.AllVariants, SaCommon.NsiSchemaVersion))
            {
                writer.Write(parser.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}