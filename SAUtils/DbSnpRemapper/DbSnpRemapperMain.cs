using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using Nirvana;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.DbSnpRemapper
{
    public static class DbSnpRemapperMain
    {
        private static string _srcMapFile;
        private static string _destMapFile;
        private static string _srcRefSequence;
        private static string _desRefSequence;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "src|s=",
                    "VCF file with dbSNP ids and data to be remapped",
                    v => _srcMapFile = v
                },
                {
                    "des|d=",
                    "VCF file (with same chromosome order as src) with destination dbSNP mapping",
                    v => _destMapFile = v
                },
                {
                    "sref=",
                    "compressed reference sequence file for the source assembly",
                    v => _srcRefSequence = v
                },
                {
                    "dref=",
                    "compressed reference sequence file for the destination assembly",
                    v => _desRefSequence = v
                }

            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_srcMapFile, "VCF file with dbSNP ids and data to be remapped", "--src")
                .CheckInputFilenameExists(_destMapFile, "VCF file with destination dbSNP mapping", "--des")
                .CheckInputFilenameExists(_srcRefSequence, "reference sequence for source genome assembly", "--sref")
                .CheckInputFilenameExists(_desRefSequence, "reference sequence for destination genome assembly", "--dref")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            const string tempLeftoverFilename = "LeftOvers.vcf.gz";
            Dictionary<string, StreamWriter> writers;

            ISequenceProvider srcSequenceProvider = ProviderUtilities.GetSequenceProvider(_srcRefSequence);
            ISequenceProvider desSequenceProvider = ProviderUtilities.GetSequenceProvider(_desRefSequence);
            using (var srcReader = GZipUtilities.GetAppropriateStreamReader(_srcMapFile))
            using (var destReader = GZipUtilities.GetAppropriateStreamReader(_destMapFile))
            using (var leftoverWriter = GZipUtilities.GetStreamWriter(tempLeftoverFilename))
            {
                var chromMapper = new ChromMapper(srcReader, destReader, leftoverWriter, srcSequenceProvider, desSequenceProvider);
                writers = chromMapper.Map();
            }

            //now we will try to map the leftovers
            using (var destReader = GZipUtilities.GetAppropriateStreamReader(_destMapFile))
            using (var leftoverReader = GZipUtilities.GetAppropriateStreamReader(tempLeftoverFilename))
            {
                var leftOverMapper = new LeftoverMapper(leftoverReader, destReader, writers, desSequenceProvider);
                var leftoverCount = leftOverMapper.Map();
                Console.WriteLine($"{leftoverCount} leftovers mapped!!");
            }

            foreach (var writer in writers.Values)
            {
                writer.Dispose();
            }
            
            return ExitCodes.Success;
        }
    }
}