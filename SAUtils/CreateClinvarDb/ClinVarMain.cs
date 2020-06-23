using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CreateClinvarDb
{
    public static class ClinVarMain
    {
        private static string _rcvFile;
        private static string _vcvFile;
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
                    "rcv|i=",
                    "ClinVar Full release XML file",
                    v => _rcvFile = v
                },
                {
                    "vcv|c=",
                    "ClinVar Variation release XML file",
                    v => _vcvFile = v
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
                .CheckInputFilenameExists(_rcvFile, "ClinVar full release XML file", "--rcv")
                .CheckInputFilenameExists(_vcvFile, "ClinVar variation release XML file", "--vcv")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with ClinVar annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var version           = DataSourceVersionReader.GetSourceVersion(_rcvFile + ".version");
            string outFileName = $"{version.Name}_{version.Version}";
            
            using (var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference)))
            using (var clinvarReader     = new ClinVarReader(GZipUtilities.GetAppropriateReadStream(_rcvFile), GZipUtilities.GetAppropriateReadStream(_vcvFile), referenceProvider))
            using (var nsaStream         = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName+SaCommon.SaFileSuffix)))
            using (var indexStream       = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var nsaWriter         = new NsaWriter(nsaStream, indexStream, version, referenceProvider, SaCommon.ClinvarTag, false, true, SaCommon.SchemaVersion, false))
            using (var schemaStream      = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.JsonSchemaSuffix)))
            using (var schemaWriter      = new StreamWriter(schemaStream))
            {
                nsaWriter.Write(clinvarReader.GetItems());
                schemaWriter.Write(clinvarReader.JsonSchema);
            }

            return ExitCodes.Success;
        }
    }
}