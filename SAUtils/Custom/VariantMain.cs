using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.Custom
{
    public static class VariantMain
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
                    "custom TSV file path",
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
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence", "--ref")
                .CheckInputFilenameExists(_inputFile, "Custom variant annotation TSV", "--in")
                .CheckDirectoryExists(_outputDirectory, "output", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary variant annotation database from a custom input file", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            
            List<CustomInterval> intervals;
            SaJsonSchema intervalJsonSchema;
            string jsonTag;
            DataSourceVersion version;
            string outputPrefix      = GetOutputPrefix(_inputFile);
            string nsaFileName       = Path.Combine(_outputDirectory, outputPrefix + SaCommon.SaFileSuffix);
            string nsaIndexFileName  = nsaFileName + SaCommon.IndexSufix;
            string nsaSchemaFileName = nsaFileName + SaCommon.JsonSchemaSuffix;
            int nsaItemsCount;

            using (var parser = VariantAnnotationsParser.Create(GZipUtilities.GetAppropriateStreamReader(_inputFile), referenceProvider))
            using (var nsaStream   = FileUtilities.GetCreateStream(nsaFileName))
            using (var indexStream = FileUtilities.GetCreateStream(nsaIndexFileName))       
            using (var nsaWriter = CaUtilities.GetNsaWriter(nsaStream, indexStream, parser, CaUtilities.GetInputFileName(_inputFile), referenceProvider, out version))
            using (var saJsonSchemaStream = FileUtilities.GetCreateStream(nsaSchemaFileName))
            using (var schemaWriter = new StreamWriter(saJsonSchemaStream))
            {
                (jsonTag, nsaItemsCount, intervalJsonSchema, intervals) = CaUtilities.WriteSmallVariants(parser, nsaWriter, schemaWriter);
                if (intervals == null) return ExitCodes.Success;
            }

            using (var nsiStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outputPrefix + SaCommon.SiFileSuffix)))
            using (var nsiWriter = CaUtilities.GetNsiWriter(nsiStream, version, referenceProvider.Assembly, jsonTag))
            using (var siJsonSchemaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outputPrefix + SaCommon.SiFileSuffix + SaCommon.JsonSchemaSuffix)))
            using (var schemaWriter = new StreamWriter(siJsonSchemaStream))
            {
                nsiWriter.Write(intervals);
                schemaWriter.Write(intervalJsonSchema);
            }

            return ExitCodes.Success;
        }

        private static string GetOutputPrefix(string inputFilePath)
        {
            string fileName = CaUtilities.GetInputFileName(inputFilePath);
            if (fileName.EndsWith(".tsv"))
                return fileName.Substring(0, fileName.Length - 4);
            return fileName.EndsWith(".tsv.gz") ? fileName.Substring(0, fileName.Length - 7) : fileName;
        }
    }
}