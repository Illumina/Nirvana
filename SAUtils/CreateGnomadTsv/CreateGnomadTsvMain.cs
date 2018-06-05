using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;

namespace SAUtils.CreateGnomadTsv
{
    public sealed class CreateGnomadTsvMain
    {
        private readonly HashSet<string> _supportedSequencingDataType = new HashSet<string> {"genome", "exome"};
        private static string _sequencingDataType;
        private static string _inputDirectory;
        private static string _compressedReference;
        private static string _outputDirectory;

        private ExitCodes ProgramExecution()
        {
            if (!_supportedSequencingDataType.Contains(_sequencingDataType)) 
                throw new ArgumentException($"Only the following sequencing data types are supported: {string.Join(_supportedSequencingDataType.ToString(), ", ")}");

            var inputFiles = Directory.GetFiles(_inputDirectory, "*.vcf.bgz");
            
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));

            if (inputFiles.Length == 0)
                throw new UserErrorException("input directory does not conatin any .vcf.bgz files");

            var versionFiles = Directory.GetFiles(_inputDirectory, "*.version");
            if (versionFiles.Length != 1)
                throw new InvalidDataException("more than one .version file found in input directory");

            Console.WriteLine($"Creating gnomAD TSV file from {inputFiles.Length} input files");

            var version = DataSourceVersionReader.GetSourceVersion(versionFiles[0]);
            var gnomadTsvCreator = new GnomadTsvCreator(inputFiles, referenceProvider, version, _outputDirectory, _sequencingDataType);

            gnomadTsvCreator.CreateTsvsParallel();
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
    }
}