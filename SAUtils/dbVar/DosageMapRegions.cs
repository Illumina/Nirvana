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
using VariantAnnotation.Sequence;

namespace SAUtils.dbVar
{
    public static class DosageMapRegions
    {
        private static string _outputDirectory;
        private static string _dosageMapRegionFile;
        private static string _inputReferencePath;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "tsv|t=",
                    "input tsv file",
                    v => _dosageMapRegionFile = v
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
                .CheckInputFilenameExists(_dosageMapRegionFile, "dosage map region TSV file", "--tsv")
                .CheckInputFilenameExists(_inputReferencePath, "reference sequence file", "--tsv")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates an interval annotation database from dbVar data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var dosageMapRegionVersion = DataSourceVersionReader.GetSourceVersion(_dosageMapRegionFile + ".version");
            string outFileName =  $"{dosageMapRegionVersion.Name.Replace(' ', '_')}_{dosageMapRegionVersion.Version}";
            var referenceProvider = new ReferenceSequenceProvider(GZipUtilities.GetAppropriateReadStream(_inputReferencePath));
            using (var dosageSensitivityParser = new DosageMapRegionParser(GZipUtilities.GetAppropriateReadStream(_dosageMapRegionFile), referenceProvider.RefNameToChromosome))
            using (var stream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SiFileSuffix)))
            using (var nsiWriter = new NsiWriter(stream, dosageMapRegionVersion, referenceProvider.Assembly, SaCommon.DosageSensitivityTag, ReportFor.StructuralVariants, SaCommon.SchemaVersion))
            {
                nsiWriter.Write(dosageSensitivityParser.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}