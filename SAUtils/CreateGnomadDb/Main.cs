using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CreateGnomadDb
{
    public sealed class Main
    {
        private readonly HashSet<string> _supportedSequencingDataType = new HashSet<string> { "genome", "exome" };
        private static string _sequencingDataType;
        private static string _inputDirectory;
        private static string _compressedReference;
        private static string _outputDirectory;
        
        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var creator = new Main();
            var ops = new OptionSet
            {
                {
                    "type|t=",
                    "sequencing data type: genome or exome",
                    v => _sequencingDataType = v
                },
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _compressedReference = v
                 },
                {
                    "in|i=",
                    "input directory containing VCF (and .version) files",
                    v => _inputDirectory = v
                },
                {
                    "out|o=",
                    "output directory for TSVs",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(_sequencingDataType, "type of input sequencing data (", "--type")
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_inputDirectory, "input directory containing gnomAD vcf files", "--in")
                .CheckDirectoryExists(_inputDirectory, "input directory containing gnomAD vcf files", "--in")
                .HasRequiredParameter(_outputDirectory, "output Supplementary directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output Supplementary directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(creator.ProgramExecution);

            return exitCode;
        }
        private ExitCodes ProgramExecution()
        {
            if (!_supportedSequencingDataType.Contains(_sequencingDataType))
                throw new ArgumentException($"Only the following sequencing data types are supported: {string.Join(_supportedSequencingDataType.ToString(), ", ")}");
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));

            var inputFiles = Directory.GetFiles(_inputDirectory, "*.vcf.bgz");
            if (inputFiles.Length==0) inputFiles = Directory.GetFiles(_inputDirectory, "*.vcf.gz");

            if (inputFiles.Length == 0)
                throw new UserErrorException("input directory does not contain any .vcf.bgz files");

            var versionFiles = Directory.GetFiles(_inputDirectory, "*.version");
            if (versionFiles.Length != 1)
                throw new InvalidDataException("more than one .version file found in input directory");
            var version = DataSourceVersionReader.GetSourceVersion(versionFiles[0]);

            Console.WriteLine($"Creating gnomAD TSV file from {inputFiles.Length} input files");

           
            string outFileName = $"{version.Name}_{version.Version}";
            var jsonTag = _sequencingDataType == "genome" ? SaCommon.GnomadTag : SaCommon.GnomadExomeTag;

            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var nsaWriter = new NsaWriter(new ExtendedBinaryWriter(nsaStream), new ExtendedBinaryWriter(indexStream), version, referenceProvider, jsonTag, true, false, SaCommon.SchemaVersion, false))
            {
                nsaWriter.Write(GetItems(inputFiles, referenceProvider));
            }

            return ExitCodes.Success;
        }

        private static IEnumerable<ISupplementaryDataItem> GetItems(IEnumerable<string> filePaths,
            ISequenceProvider referenceProvider)
        {
            IEnumerable<ISupplementaryDataItem> items = null;

            foreach (string filePath in filePaths)
            {
                var fileStreamReader = GZipUtilities.GetAppropriateStreamReader(filePath);
                var reader = new GnomadReader(fileStreamReader, referenceProvider);
                items = items == null ? reader.GetItems() : items.Concat(reader.GetItems());
            }

            return items;
        }
    }

}