using System;
using System.Linq;
using NDesk.Options;
using SAUtils.CreateCustomIntervalDatabase;
using SAUtils.CreateOmimDatabase;
using SAUtils.CreateSupplementaryDatabase;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Utilities;

namespace SAUtils
{
    static class SaUtils
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                DisplayHelp();
                return -1;
            }

            var subCommand = args[0];
            var commandArgs = args.Length == 1 ? new string[] { } : args.Skip(1).Take(args.Length - 1).ToArray();

            int exitCode;
            switch (subCommand)
            {
                case "createSA":
                    exitCode = ExecuteCreateSupplementaryDatabase(commandArgs);
                    break;
                case "createCI":
                    exitCode = ExecuteCreateCustomeIntervals(commandArgs);
                    break;
                case "createOMIM":
                    exitCode = ExecuteCreateOmimDatabase(commandArgs);
                    break;
                case "--help":
                    DisplayHelp();
                    exitCode = 0;
                    break;
                default:
                    Console.WriteLine("unrecognized command " + subCommand);
                    exitCode = -1;
                    break;
            }

            return exitCode;
        }

        private static void DisplayHelp()
        {
            CommandLineUtilities.DisplayBanner(Constants.Authors);
            Console.WriteLine("Usage: SAUtils <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands: ");
            var filler = new string(' ', 7);
            var filler2 = new string(' ', 10);
            Console.WriteLine(filler + "createCI" + filler2 + "Create customeIntervals");

            Console.WriteLine(filler + "createSA" + filler2 + "Create supplementary database");
            Console.WriteLine(filler + "createOMIM" + filler2 + "Create OMIM database");

        }
        private static int ExecuteCreateOmimDatabase(string[] commandArgs)
        {
            var ops = new OptionSet
            {

                 {
                     "m|mim=",
                     "input genemap file",
                     v => CreateOmimDatabase.ConfigurationSettings.OmimFile =v
                 },
                 {
                     "o|out=",
                     "output Nirvana Omim directory",
                     v => CreateOmimDatabase.ConfigurationSettings.OutputOmimDirectory = v
                 }
            };

            var commandLineExample = "createOMIM [options]";

            var converter = new CreateOmimDatabaseMain("Reads omim gene map file and creates Omim database", ops, commandLineExample, Constants.Authors);
            converter.Execute(commandArgs);
            return converter.ExitCode;
        }

        private static int ExecuteCreateCustomeIntervals(string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "r|ref=",
                     "compressed reference sequence file",
                     v => CreateCustomIntervalDatabase.ConfigurationSettings.CompressedReference = v
                 },
                 {
                     "b|bed=",
                     "input bed file",
                     v => CreateCustomIntervalDatabase.ConfigurationSettings.BedFile =v
                 },
                 {
                     "o|out=",
                     "output Nirvana Supplementary directory",
                     v => CreateCustomIntervalDatabase.ConfigurationSettings.OutputDirectory = v
                 }
            };

            var commandLineExample = "createCI [options]";

            var converter = new CreateCustomIntervalDbMain("Reads provided bed file and creates custom interval database", ops, commandLineExample, Constants.Authors);
            converter.Execute(commandArgs);
            return converter.ExitCode;
        }

        private static int ExecuteCreateSupplementaryDatabase(string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "r|ref=",
                     "compressed reference sequence file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.CompressedReference = v
                 },
                 {
                     "d|dbs=",
                     "input dbSNP vcf.gz file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputDbSnpFileName = v
                 },
                 {
                     "c|csm=",
                     "input COSMIC vcf file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputCosmicVcfFileName = v
                 },
                 {
                     "tsv=",
                     "input COSMIC TSV file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputCosmicTsvFileName = v
                 },
                 {
                     "V|cvr=",
                     "input ClinVar file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputClinVarFileName= v
                 },
                 {
                     "vcf=",
                     "input ClinVar no known medical importance file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputClinvarXml= v
                 },
                 {
                     "k|onek=",
                     "input 1000 Genomes AF file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.Input1000GFileName= v
                 },
                 {
                     "e|evs=",
                     "input EVS file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputEvsFile= v
                 },
                 {
                     "x|exac=",
                     "input ExAc file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputExacFile= v
                 },
                 {
                     "g|dgv=",
                     "input Dgv file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.InputDgvFile= v
                 },
                 {
                     "t|cust=",
                     "input Custom annotation file",
                     v => CreateSupplementaryDatabase.ConfigurationSettings.CustomAnnotationFiles.Add(v)
                 },
                {
                    "s|onekSv=",
                    "input 1000 Genomes Structural file",
                    v => CreateSupplementaryDatabase.ConfigurationSettings.Input1000GSvFileName = v
                },
                {
                    "l|clinGen=",
                    "input ClinGen file",
                    v => CreateSupplementaryDatabase.ConfigurationSettings.InputClinGenFileName = v
                },
                {
                    "chr=",
                    "comma separated list of chromosomes to produce SA for",
                    v => CreateSupplementaryDatabase.ConfigurationSettings.ChromosomeList = v
                },
                {
                    "o|out=",
                    "output Nirvana Supplementary directory",
                    v => CreateSupplementaryDatabase.ConfigurationSettings.OutputSupplementaryDirectory = v
                }
            };

            var commandLineExample = "createSA [options]";

            var converter = new CreateSupplementaryDatabaseMain("Reads provided supplementary data files and populates the combined nirvana supplementary database file", ops, commandLineExample, Constants.Authors);
            converter.Execute(commandArgs);
            return converter.ExitCode;
        }
    }
}
