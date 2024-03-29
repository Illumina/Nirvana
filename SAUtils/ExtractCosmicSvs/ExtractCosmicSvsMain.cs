﻿using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;

namespace SAUtils.ExtractCosmicSvs
{
    public static class ExtractCosmicSvsMain
    {
        private static string _breakendTsv;
        private static string _cnvTsv;
        private static string _outputDir;
        private static string _compressedReference;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "brk|b=",
                    "input TSV file with breakend data",
                    v => _breakendTsv = v
                },
                {
                    "cnv|c=",
                    "input TSV file with CNV data",
                    v => _cnvTsv = v
                },
                {
                    "ref|r=",
                    "compressed reference sequence file",
                    v => _compressedReference = v
                },
                {
                    "out|o=",
                    "output directory for intermediate TSV",
                    v => _outputDir = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(_cnvTsv, "input TSV file with CNV data", "--cnv")
                .HasRequiredParameter(_outputDir, "output directory name", "--out")
                .CheckDirectoryExists(_outputDir, "output directory name", "--out")
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var version = DataSourceVersionReader.GetSourceVersion(_cnvTsv+ ".version");
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));

            var cnvStream = _cnvTsv==null? null: GZipUtilities.GetAppropriateReadStream(_cnvTsv);
            var breakendStream = _breakendTsv == null ? null : GZipUtilities.GetAppropriateReadStream(_breakendTsv);

            using (new CosmicSvReader(cnvStream, breakendStream, version, _outputDir,
                referenceProvider.Assembly, referenceProvider.RefNameToChromosome))
            {
                //cosmicSvExtractor.CreateTsv();
            }
            
            return ExitCodes.Success;
        }
    }
}