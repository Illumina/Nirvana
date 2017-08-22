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


		private static void GetTsvAndIntervalFiles(string tsvFilesDirectory, List<string> intermediateFiles, List<string> intervalFiles)
		{
			if(!Directory.Exists(tsvFilesDirectory)) return;
			foreach (var file in Directory.GetFiles(tsvFilesDirectory))
			{
				if(!file.EndsWith(".tsv.gz")) continue;
				if (file.EndsWith(".interval.tsv.gz"))
				{
					intervalFiles.Add(file);
					continue;
				}
			    if (file.EndsWith(".misc.tsv.gz"))
			    {
			        ConfigurationSettings.MiscFile = file;
                    continue;
                }
				intermediateFiles.Add(file);
			}
		}

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


		    //Get files in tsv file Directory and add them to intermediate files or interval files
		    GetTsvAndIntervalFiles(ConfigurationSettings.TsvFilesDirectory, ConfigurationSettings.IntermediateFiles,
		        ConfigurationSettings.IntervalFiles);
		    var fileCount = ConfigurationSettings.MiscFile == null
		        ? ConfigurationSettings.IntermediateFiles.Count + ConfigurationSettings.IntervalFiles.Count
		        : ConfigurationSettings.IntermediateFiles.Count + ConfigurationSettings.IntervalFiles.Count + 1;

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
		        .Parse()
                .CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Onput compressed reference file", "--ref")
		        .HasRequiredParameter(ConfigurationSettings.OutputDirectory, "Output Supplementary directory", "--out")
                .CheckNonZero(fileCount, "No intermediate files were provided, use --dir or --tsv /--int")
                .CheckEachFilenameExists(ConfigurationSettings.IntermediateFiles, "Intermediate Annotation file name", "--tsv", false)
		        .CheckEachFilenameExists(ConfigurationSettings.IntervalFiles, "Intermediate interval file name", "--int",false)
		        .CheckInputFilenameExists(ConfigurationSettings.MiscFile, "Intermediate misc file name", "--misc", false)
    .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Reads provided intermediate TSV files and creates supplementary database", commandLineExample)
		        .ShowErrors()
		        .Execute(merger.ProgramExecution);


			return exitCode;



        }
	}
}
