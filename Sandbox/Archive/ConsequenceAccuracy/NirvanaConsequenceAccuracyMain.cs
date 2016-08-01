using System;
using System.Collections.Generic;
using System.Text;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;


namespace CdnaEndPointInvestigation
{
    class CdnaEndPointInvestigationMain
    {
        static void Main(string[] args)
        {
            // CommandLineUtilities.DisplayBanner("Michael Stromberg");

            // ==============================
            // parse our command line options
            // ==============================

            bool showHelpMenu = false;

            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input VCF {filename}",
                    v => ConfigurationSettings.InputVcfPath = v
                },
                {
                    "dir|d=",
                    "input Nirvana {directory}",
                    v => ConfigurationSettings.InputNirvanaDirectory = v
                },
                {
                    "g|go",
                    "do not stop on difference",
                    v => ConfigurationSettings.DoNotStopOnDifference = v != null
                },
                {
                    "o|out=",
                    "output VCF {filename}",
                    v => ConfigurationSettings.OutputVcfPath = v
                },
                {
                    "s|silent",
                    "enable silent mode",
                    v => ConfigurationSettings.Silent = v != null
                },
                {
                    "h|help",
                    "displays the help menu",
                    v => showHelpMenu = v != null
                }
            };

            List<string> unsupportedOps;
            try
            {
                unsupportedOps = ops.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR: The command line parameters could not be parsed: {0}", e.Message);
                return;
            }

            if ((args.Length == 0) || (unsupportedOps.Count > 0) || showHelpMenu)
            {
                Help.Show(ops, "--in <vcf filename> --dir <Nirvana directory>", "Annotates a set of variants");
                CommandLineUtilities.ShowUnsupportedOptions(unsupportedOps);
                return;
            }

            // =============================
            // check for missing information
            // =============================

            var errorBuilder = new StringBuilder();
            var errorSpacer = new string(' ', 7);
            bool foundError = CommandLineUtilities.CheckExistingFilenameOption(ConfigurationSettings.InputVcfPath, "vcf", "--in", errorBuilder, errorSpacer, false);
            foundError = CommandLineUtilities.CheckExistingDirectoryOption(ConfigurationSettings.InputNirvanaDirectory, "Nirvana", "--dir", errorBuilder, errorSpacer, foundError);


            // print the errors if any were found
            if (foundError)
            {
                Console.WriteLine("Some problems were encountered when parsing the command line options:");
                Console.WriteLine("{0}", errorBuilder);
                Console.WriteLine("For a complete list of command line options, type \"{0} -h\"", Environment.GetCommandLineArgs()[0]);
                Environment.Exit(1);
            }

            // =====================
            // annotate our variants
            // =====================

            var bench = new Benchmark();

            // try
            //{
            //    var annotator = new CdnaAccuracyChecker(!ConfigurationSettings.DoNotStopOnDifference,
            //        ConfigurationSettings.InputNirvanaDirectory);

            //    annotator.Compare(ConfigurationSettings.InputVcfPath, ConfigurationSettings.OutputVcfPath, ConfigurationSettings.Silent);
            //}
            // catch (Exception e)
            //{
            //    CommandLineUtilities.ShowException(e);
            //}

            try
            {
                // var annotator = new CdsAccuracyChecker(!ConfigurationSettings.DoNotStopOnDifference,
                    // ConfigurationSettings.InputNirvanaDirectory);
                var annotator = new ConsequenceAccuracyChecker(!ConfigurationSettings.DoNotStopOnDifference,
                    ConfigurationSettings.InputNirvanaDirectory);
                annotator.Compare(ConfigurationSettings.InputVcfPath, ConfigurationSettings.OutputVcfPath, ConfigurationSettings.Silent);
            }
            catch (Exception e)
            {
                CommandLineUtilities.ShowException(e);
            }

            Console.WriteLine();
            MemoryUtilities.ShowPeakMemoryUsage();
            Console.WriteLine("Time: {0}", bench.GetElapsedTime());
        }
    }
}
