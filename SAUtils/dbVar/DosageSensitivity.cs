using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.SA;

namespace SAUtils.dbVar
{
    public static class DosageSensitivity
    {
        private static string _outputDirectory;
        private static string _dosageSensitivityFile;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "tsv|t=",
                    "input tsv file",
                    v => _dosageSensitivityFile = v
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
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .CheckInputFilenameExists(_dosageSensitivityFile, "dosage sensitivity TSV file", "--tsv")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from dbVar data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var dosageSensitivityVersion = DataSourceVersionReader.GetSourceVersion(_dosageSensitivityFile + ".version");

            string outFileName = $"{dosageSensitivityVersion.Name.Replace(' ','_')}_{dosageSensitivityVersion.Version}";

            //create universal gene archive
            using (var dosageSensitivityParser= new DosageSensitivityParser(GZipUtilities.GetAppropriateReadStream(_dosageSensitivityFile)))
            using (var stream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.NgaFileSuffix)))
            using (var ngaWriter = new NgaWriter(stream, dosageSensitivityVersion, SaCommon.DosageSensitivityTag, SaCommon.SchemaVersion, false))
            {
                ngaWriter.Write(dosageSensitivityParser.GetItems());
            }

            return ExitCodes.Success;
        }

    }
}