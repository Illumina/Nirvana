using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface;

namespace SAUtils.CreateOmimTsv
{
    class CreateOmimTsvMain
    {
        private ExitCodes ProgramExecution()
        {

            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var creator = new CreateOmimTsvMain();


            var ops = new OptionSet
            {
                {
                    "gi=",
                    "gene_info {path} (newest first)",
                    v => ConfigurationSettings.GeneInfoPaths.Add(v)
                },
                {
                    "hgnc=",
                    "HGNC {path}",
                    v => ConfigurationSettings.HgncPath = v
                },
                {
                    "mim=",
                    "mim2gene {path}",
                    v => ConfigurationSettings.Mim2GenePath = v
                },
                {
                    "in|i=",
                    "input genemap2 {path}",
                    v => ConfigurationSettings.InputGeneMap2Path = v
                },
                {
                    "out|o=",
                    "output genemap2 {path}",
                    v => ConfigurationSettings.OutputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
            .CheckInputFilenameExists(ConfigurationSettings.InputGeneMap2Path, "genemap2", "--in")
            .HasRequiredParameter(ConfigurationSettings.OutputDirectory, "Output directory", "--out")
            .CheckInputFilenameExists(ConfigurationSettings.HgncPath, "HGNC", "--hgnc")
            .CheckEachFilenameExists(ConfigurationSettings.GeneInfoPaths, "geneinfo files", "--gi")
            .ShowBanner(Constants.Authors)
            .ShowHelpMenu("Reads provided OMIM data files and populates tsv file", commandLineExample)
            .ShowErrors()
            .Execute(creator.ProgramExecution);

            return exitCode;
        }
    }
}
