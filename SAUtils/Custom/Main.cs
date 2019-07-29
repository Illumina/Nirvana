using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.Custom
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
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_inputFile, "Custom TSV file", "--in")
                .CheckInputFilenameExists(_inputFile, "Custom TSV file", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database from a custom input file", commandLineExample)
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
            string outputPrefix = GetOutputPrefix(_inputFile);
            var nsaFileName       = Path.Combine(_outputDirectory, outputPrefix + SaCommon.SaFileSuffix);
            var nsaIndexFileName  = Path.Combine(_outputDirectory, outputPrefix + SaCommon.SaFileSuffix + SaCommon.IndexSufix);
            var nsaSchemaFileName = Path.Combine(_outputDirectory, outputPrefix + SaCommon.SaFileSuffix + SaCommon.JsonSchemaSuffix);
            var nsaItemsCount     = 0;

            using (var customReader = CustomAnnotationsParser.Create(GZipUtilities.GetAppropriateStreamReader(_inputFile), referenceProvider))
            using (var nsaStream   = FileUtilities.GetCreateStream(nsaFileName))
            using (var indexStream = FileUtilities.GetCreateStream(nsaIndexFileName))            
            using (var nsaWriter = new NsaWriter(
                                new ExtendedBinaryWriter(nsaStream),
                                new ExtendedBinaryWriter(indexStream),
                                version = new DataSourceVersion(customReader.JsonTag, GetInputFileName(_inputFile), DateTime.Now.Ticks),
                                referenceProvider,
                                customReader.JsonTag,
                                customReader.MatchByAllele,  // match by allele
                                customReader.IsArray, // is array
                                SaCommon.SchemaVersion,
                                false,// is positional
                                false, // skip incorrect ref base
                                true // throw error on conflicting entries
                                ))
            using (var saJsonSchemaStream = FileUtilities.GetCreateStream(nsaSchemaFileName))
            using (var schemaWriter = new StreamWriter(saJsonSchemaStream))
            {
                jsonTag = customReader.JsonTag;
                nsaItemsCount = nsaWriter.Write(customReader.GetItems());
                schemaWriter.Write(customReader.JsonSchema);

                intervalJsonSchema = customReader.IntervalJsonSchema;
                intervals = customReader.GetCustomIntervals();
            }

            if (nsaItemsCount == 0)
            {
                if (File.Exists(nsaFileName)) File.Delete(nsaFileName);
                if (File.Exists(nsaIndexFileName)) File.Delete(nsaIndexFileName);
                if (File.Exists(nsaSchemaFileName)) File.Delete(nsaSchemaFileName);
            }

            if (nsaItemsCount == 0 && intervals == null)
                throw new UserErrorException("The provided TSV has no valid custom annotation entries.");

            if (intervals == null) return ExitCodes.Success;

            using (var nsiStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outputPrefix + SaCommon.SiFileSuffix)))
            using (var nsiWriter = new NsiWriter(new ExtendedBinaryWriter(nsiStream), version, referenceProvider.Assembly, jsonTag, ReportFor.AllVariants, SaCommon.SchemaVersion))
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
            var fileName = GetInputFileName(inputFilePath);
            if (fileName.EndsWith(".tsv"))
                return fileName.Substring(0, fileName.Length - 4);
            return fileName.EndsWith(".tsv.gz") ? fileName.Substring(0, fileName.Length - 7) : fileName;
        }

        private static string GetInputFileName(string inputFilePath)
        {
            var fileNameIndex = inputFilePath.LastIndexOf(Path.DirectorySeparatorChar);
            return fileNameIndex < 0 ? inputFilePath : inputFilePath.Substring(fileNameIndex + 1);
        }
    }

}