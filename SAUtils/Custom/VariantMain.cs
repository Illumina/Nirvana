using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.Custom
{
    public static class VariantMain
    {
        private static string _inputFile;
        private static string _compressedReference;
        private static string _outputDirectory;
        private static bool   _skipRefBaseValidation;
        
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
                },
                {
                    "skip-ref",
                    "skip ref base validation",
                    v => _skipRefBaseValidation = v != null
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
            SaJsonSchema         intervalJsonSchema;
            string               jsonTag;
            DataSourceVersion    version;
            string               outputPrefix      = GetOutputPrefix(_inputFile);
            string               nsaFileName       = Path.Combine(_outputDirectory, outputPrefix + SaCommon.SaFileSuffix);
            string               nsaIndexFileName  = nsaFileName + SaCommon.IndexSufix;
            string               nsaSchemaFileName = nsaFileName + SaCommon.JsonSchemaSuffix;
            ReportFor            reportFor;

            var nsaItemCount = 0;

            using (var parser = VariantAnnotationsParser.Create(GZipUtilities.GetAppropriateStreamReader(_inputFile), referenceProvider))
            using (var nsaStream   = FileUtilities.GetCreateStream(nsaFileName))
            using (var indexStream = FileUtilities.GetCreateStream(nsaIndexFileName))       
            using (var nsaWriter = CaUtilities.GetNsaWriter(nsaStream, indexStream, parser,  CaUtilities.GetInputFileName(_inputFile),referenceProvider, out version, _skipRefBaseValidation))
            using (var saJsonSchemaStream = FileUtilities.GetCreateStream(nsaSchemaFileName))
            using (var schemaWriter = new StreamWriter(saJsonSchemaStream))
            {
                (jsonTag, nsaItemCount, intervalJsonSchema, intervals) = CaUtilities.WriteSmallVariants(parser, nsaWriter, schemaWriter);
                reportFor = parser.ReportFor;
                if (intervals == null) return ExitCodes.Success;
            }

            if (nsaItemCount == 0)
            {
                File.Delete(nsaFileName);
                File.Delete(nsaIndexFileName);
                File.Delete(nsaSchemaFileName);
            }

            using (var nsiStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outputPrefix + SaCommon.IntervalFileSuffix)))
            using (var nsiWriter = CaUtilities.GetNsiWriter(nsiStream, version, referenceProvider.Assembly, jsonTag, reportFor))
            using (var siJsonSchemaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outputPrefix + SaCommon.IntervalFileSuffix + SaCommon.JsonSchemaSuffix)))
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