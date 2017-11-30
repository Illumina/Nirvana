using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateGnomadTsv
{
    public sealed class CreateGnomadTsvMain
    {
        private readonly HashSet<string> _supportedSequencingDataType = new HashSet<string> {"genome", "exome"};
        private ExitCodes ProgramExecution()
        {
            if (!_supportedSequencingDataType.Contains(ConfigurationSettings.SequencingDataType)) 
                throw new ArgumentException($"Only the following sequencing data types are supported: {string.Join(_supportedSequencingDataType.ToString(), ", ")}");

            var inputStreamReaders = Directory.GetFiles(ConfigurationSettings.InputDirectory, "*.vcf.bgz").Select(fileName => GZipUtilities.GetAppropriateStreamReader(Path.Combine(ConfigurationSettings.InputDirectory, fileName))).ToArray();
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(ConfigurationSettings.CompressedReference));

            if (inputStreamReaders.Length == 0)
                throw new UserErrorException("input directory does not conatin any .vcf.bgz files");

            var versionFiles = Directory.GetFiles(ConfigurationSettings.InputDirectory, "*.version");
            if (versionFiles.Length != 1)
                throw new InvalidDataException("more than one .version file found in input directory");

            Console.WriteLine($"Creating gnomAD TSV file from {inputStreamReaders.Length} input files");

            var version = DataSourceVersionReader.GetSourceVersion(versionFiles[0]);
            var gnomadTsvCreator = new GnomadTsvCreator(inputStreamReaders, referenceProvider, version, ConfigurationSettings.OutputDirectory, ConfigurationSettings.SequencingDataType);

            gnomadTsvCreator.CreateTsvs();
            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var creator = new CreateGnomadTsvMain();
            var ops = new OptionSet
            {
                {
                    "type|t=",
                    "sequencing data type: genome or exome",
                    v => ConfigurationSettings.SequencingDataType = v
                },
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => ConfigurationSettings.CompressedReference = v
                 },
                {
                    "in|i=",
                    "input directory containing VCF (and .version) files",
                    v => ConfigurationSettings.InputDirectory = v
                },
                {
                    "out|o=",
                    "output directory for TSVs",
                    v => ConfigurationSettings.OutputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(ConfigurationSettings.SequencingDataType, "type of input sequencing data (", "--type")
                .CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(ConfigurationSettings.InputDirectory, "input directory containing gnomAD vcf files", "--in")
                .CheckDirectoryExists(ConfigurationSettings.InputDirectory, "input directory containing gnomAD vcf files", "--in")
                .HasRequiredParameter(ConfigurationSettings.OutputDirectory, "output Supplementary directory", "--out")
                .CheckDirectoryExists(ConfigurationSettings.OutputDirectory, "output Supplementary directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(creator.ProgramExecution);

            return exitCode;
        }
    }
}