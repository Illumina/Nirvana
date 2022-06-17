using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.Decipher;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;

namespace SAUtils.CreateDecipherDb
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
                    "input txt file path",
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
                .HasRequiredParameter(_inputFile, "Decipher txt file", "--in")
                .CheckInputFilenameExists(_inputFile, "Decipher txt file", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with Decipher", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            
            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            
            string outFileName = $"{version.Name}_{version.Version}".Replace(' ','_');
            using (var decipherParser = new DecipherParser(GZipUtilities.GetAppropriateStreamReader(_inputFile), referenceProvider.RefNameToChromosome)) 
            using (FileStream nsiStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.IntervalFileSuffix)))
            using (var nsiWriter = new NsiWriter(nsiStream, version, referenceProvider.Assembly, SaCommon.DecipherTag, ReportFor.StructuralVariants, SaCommon.SchemaVersion))
            {
                nsiWriter.Write(decipherParser.GetItems());
            }
            
            return ExitCodes.Success;
        }
    }

}