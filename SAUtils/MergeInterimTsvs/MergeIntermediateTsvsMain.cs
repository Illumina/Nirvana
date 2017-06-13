using System.Collections.Generic;
using System.IO;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using VariantAnnotation.DataStructures;

namespace SAUtils.MergeInterimTsvs
{
	public class MergeIntermediateTsvsMain : AbstractCommandLineHandler
	{
		private MergeIntermediateTsvsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
		{
		}

		protected override void ValidateCommandLine()
		{
			CheckAndCreateDirectory(ConfigurationSettings.OutputDirectory, "Output Supplementary directory", "--out");
			CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Onput compressed reference file", "--ref");
			CheckDirectoryExists(ConfigurationSettings.TsvFilesDirectory,"Directory for tsv files to input","--dir",false);

			//Get files in tsv file Directory and add them to intermediate files or interval files
			GetTsvAndIntervalFiles(ConfigurationSettings.TsvFilesDirectory, ConfigurationSettings.IntermediateFiles,
				ConfigurationSettings.IntervalFiles);
		    var fileCount = ConfigurationSettings.MiscFile == null
		        ? ConfigurationSettings.IntermediateFiles.Count + ConfigurationSettings.IntervalFiles.Count
		        : ConfigurationSettings.IntermediateFiles.Count + ConfigurationSettings.IntervalFiles.Count + 1;


            CheckNonZero(fileCount, "No intermediate files were provided, use --dir or --tsv /--int");

			foreach (var interimFiles in ConfigurationSettings.IntermediateFiles)
			{
				CheckInputFilenameExists(interimFiles, "Intermediate Annotation file name", "--tsv", false);
			}

			foreach (var interimFiles in ConfigurationSettings.IntervalFiles)
			{
				CheckInputFilenameExists(interimFiles, "Intermediate interval file name", "--int", false);
			}
            CheckInputFilenameExists(ConfigurationSettings.MiscFile, "Intermediate misc file name", "--misc", false);
        }

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

		protected override void ProgramExecution()
		{
			var intermediateSaMerger = new MergeInterimTsvs(ConfigurationSettings.IntermediateFiles,
				ConfigurationSettings.IntervalFiles,ConfigurationSettings.MiscFile,
				ConfigurationSettings.CompressedReference,
				ConfigurationSettings.OutputDirectory);
			intermediateSaMerger.Merge();
			
		}

		public static int Run(string command,string[] commandArgs)
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

			var commandLineExample = $"{command} [options]";

			var converter = new MergeIntermediateTsvsMain("Reads provided intermediate TSV files and creates supplementary database", ops, commandLineExample, Constants.Authors);
			converter.Execute(commandArgs);
			return converter.ExitCode;
		}
	}
}
