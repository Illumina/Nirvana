using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;

namespace SAUtils.DbSnpRemapper
{
    public sealed class DbSnpRemapperMain
    {
        private static string _srcMapFile;
        private static string _destMapFile;


        public static ExitCodes Run(string command, string[] commandArgs)
        {
        var creator = new DbSnpRemapperMain();
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
                }

            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_srcMapFile, "VCF file with dbSNP ids and data to be remapped", "--src")
                .CheckInputFilenameExists(_destMapFile, "VCF file with destination dbSNP mapping", "--des")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(creator.ProgramExecution);

            return exitCode;
        }

        private ExitCodes ProgramExecution()
        {
            var tempLeftoverFilename = "LeftOvers.vcf.gz";
            Dictionary<string, StreamWriter> writers;

            using (var srcReader = GZipUtilities.GetAppropriateStreamReader(_srcMapFile))
            using (var destReader = GZipUtilities.GetAppropriateStreamReader(_destMapFile))
            using (var leftoverWriter = GZipUtilities.GetStreamWriter(tempLeftoverFilename))
            {
                var chromMapper = new ChromMapper(srcReader, destReader, leftoverWriter);
                writers = chromMapper.Map();
            }

            //now we will try to map the leftovers
            using (var destReader = GZipUtilities.GetAppropriateStreamReader(_destMapFile))
            using (var leftoverReader = GZipUtilities.GetAppropriateStreamReader(tempLeftoverFilename))
            {
                var leftOverMapper = new LeftoverMapper(leftoverReader, destReader, writers);
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