using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using VariantAnnotation.Interface;

namespace SAUtils.MergeInterimTsvs
{
    public class MergeIntermediateTsvsMain 
	{
		private ExitCodes ProgramExecution()
		{
			var intermediateSaMerger = new MergeInterimTsvs(ConfigurationSettings.IntermediateFiles,
				ConfigurationSettings.IntervalFiles,ConfigurationSettings.MiscFile,
				ConfigurationSettings.CompressedReference,
				ConfigurationSettings.OutputDirectory);
			intermediateSaMerger.Merge();

		    return ExitCodes.Success;

		}

		public static ExitCodes Run(string command,string[] commandArgs)
		{
			var ops = new OptionSet
			{
				{
					 "dir|d=",
					 " directoried for TSV supplementary annotation file in intermediate format",
					 v =>ConfigurationSettings.TsvFilesDirectory = v
				 },
				{
					 "tsv|t=",
					 "TSV supplementary annotation file in intermediate format",
					 v => ConfigurationSettings.IntermediateFiles.Add(v)
				 },
				{
					 "int|i=",
					 "TSV supplementary interval file in intermediate format",
					 v => ConfigurationSettings.IntervalFiles.Add(v)
				 },
                {
                     "misc|m=",
                     "refminor and global major allele in tsv format",
                     v => ConfigurationSettings.MiscFile = v
                },
                {
                    "gene|g=",
                    "gene annotation files in intermediate format",
                    v=>ConfigurationSettings.GeneTsvFiles.Add(v)
                },
				 {
					 "out|o=",
					 "output Nirvana Supplementary directory",
					 v => ConfigurationSettings.OutputDirectory = v
				 },
				 {
					 "ref|r=",
					 "reference sequence",
					 v => ConfigurationSettings.CompressedReference= v
				 }
			};

            var merger = new MergeIntermediateTsvsMain();
			var commandLineExample = $"{command} [options]";



            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
		        .Parse()
                .GetTsvAndIntervalFiles(ConfigurationSettings.TsvFilesDirectory, ConfigurationSettings.IntermediateFiles,
                    ConfigurationSettings.IntervalFiles,ConfigurationSettings.GeneTsvFiles)
                .CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Onput compressed reference file", "--ref")
		        .HasRequiredParameter(ConfigurationSettings.OutputDirectory, "Output Supplementary directory", "--out")
                .CheckNonZero(ConfigurationSettings.MiscFile == null
                    ? ConfigurationSettings.IntermediateFiles.Count + ConfigurationSettings.IntervalFiles.Count + ConfigurationSettings.GeneTsvFiles.Count
                    : ConfigurationSettings.IntermediateFiles.Count + ConfigurationSettings.IntervalFiles.Count + 
                    ConfigurationSettings.GeneTsvFiles.Count + 1, "No intermediate files were provided, use --dir or --tsv /--int")
                .CheckEachFilenameExists(ConfigurationSettings.IntermediateFiles, "Intermediate Annotation file name", "--tsv", false)
		        .CheckEachFilenameExists(ConfigurationSettings.IntervalFiles, "Intermediate interval file name", "--int",false)
		        .CheckInputFilenameExists(ConfigurationSettings.MiscFile, "Intermediate misc file name", "--misc", false)
                .CheckEachFilenameExists(ConfigurationSettings.GeneTsvFiles,"Intermediate gene file name","--gene",false)
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Reads provided intermediate TSV files and creates supplementary database", commandLineExample)
		        .ShowErrors()
		        .Execute(merger.ProgramExecution);


			return exitCode;



        }


    }
}
