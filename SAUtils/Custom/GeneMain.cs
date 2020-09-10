using System;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.GeneIdentifiers;
using VariantAnnotation.SA;

namespace SAUtils.Custom
{
    public static class GeneMain
    {
        private static string _inputFile;
        private static string _universalGeneArchivePath;
        private static string _outputDirectory;
        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "uga|u=",
                    "universal gene archive file path",
                    v => _universalGeneArchivePath = v
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
                .CheckInputFilenameExists(_universalGeneArchivePath, "universal gene archive", "--uga")
                .CheckInputFilenameExists(_inputFile, "Custom gene annotation TSV", "--in")
                .CheckDirectoryExists(_outputDirectory, "output", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary gene annotation database from a custom input file", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {

            var (entrezGeneIdToSymbol, ensemblGeneIdToSymbol) = GeneUtilities.ParseUniversalGeneArchive(null, _universalGeneArchivePath);

            string outputPrefix = GetOutputPrefix(_inputFile);
            string ngaFilePath = Path.Combine(_outputDirectory, outputPrefix + SaCommon.GeneFileSuffix);
            string ngaSchemaFilePath = ngaFilePath + SaCommon.JsonSchemaSuffix;

            using (var parser = GeneAnnotationsParser.Create(GZipUtilities.GetAppropriateStreamReader(_inputFile), entrezGeneIdToSymbol, ensemblGeneIdToSymbol))
            using (var ngaStream = FileUtilities.GetCreateStream(ngaFilePath))
            using (var ngaWriter = CaUtilities.GetNgaWriter(ngaStream, parser, CaUtilities.GetInputFileName(_inputFile)))
            using (var saJsonSchemaStream = FileUtilities.GetCreateStream(ngaSchemaFilePath))
            using (var schemaWriter = new StreamWriter(saJsonSchemaStream))
            {
                ngaWriter.Write(parser.GetItems());
                if(parser.GetUnknownGenes().Count > 0)
                    throw new UserErrorException($"The following gene IDs were not recognized in Nirvana: {string.Join(',',parser.GetUnknownGenes())}.");
                schemaWriter.Write(parser.JsonSchema);
            }

            return ExitCodes.Success;
        }

        private static string GetOutputPrefix(string inputFilePath)
        {
            string fileName = GetInputFileName(inputFilePath);
            if (fileName.EndsWith(".tsv"))
                return fileName.Substring(0, fileName.Length - 4);
            return fileName.EndsWith(".tsv.gz") ? fileName.Substring(0, fileName.Length - 7) : fileName;
        }

        private static string GetInputFileName(string inputFilePath)
        {
            int fileNameIndex = inputFilePath.LastIndexOf(Path.DirectorySeparatorChar);
            return fileNameIndex < 0 ? inputFilePath : inputFilePath.Substring(fileNameIndex + 1);
        }
    }
}