using System;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;

namespace SAUtils.Psa
{
    public static class PsaMain
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
                    "sift/PolyPhen file path",
                    v => _inputFile = v
                },
                {
                    "out|o=",
                    "output directory for NSA file",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .CheckInputFilenameExists(_inputFile, "SIFT file path", "--in")
                .CheckDirectoryExists(_outputDirectory, "output Supplementary directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and creates binary data files.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");

            Console.WriteLine("Loading reference...");
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            Console.WriteLine("done");
            
            string outFileName   = $"{version.Name}_{version.Version}{SaCommon.PsaFileSuffix}";
            string indexFileName = outFileName + SaCommon.IndexSuffix;

            var jsonKey = version.Name == "SIFT" ? SaCommon.SiftTag : SaCommon.PolyPhenTag;

            var saHeader = new SaHeader(jsonKey, referenceProvider.Assembly, version,
                SaCommon.PsaSchemaVersion);
            using var psaStream   = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName));
            using var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, indexFileName));
            using var writer      = new PsaWriter(psaStream, indexStream, saHeader, referenceProvider.RefNameToChromosome);
            using var psaParser = new PsaParser(GZipUtilities.GetAppropriateStreamReader(_inputFile));
            writer.Write(psaParser.GetItems());

            return ExitCodes.Success;
        }
    }
}