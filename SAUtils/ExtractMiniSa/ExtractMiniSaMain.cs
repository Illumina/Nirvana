using System;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using VariantAnnotation.DataStructures;

namespace SAUtils.ExtractMiniSa
{
    sealed class ExtractMiniSaMain : AbstractCommandLineHandler
	{
		// constructor
		private ExtractMiniSaMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
			: base(programDescription, ops, commandLineExample, programAuthors)
		{ }

		/// <summary>
		/// validates the command line
		/// </summary>
		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.InputSuppAnnotPath, "Nirvana supplementary annotations", "--in");
			CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Compressed reference sequence file name", "--ref");
			CheckDirectoryExists(ConfigurationSettings.MiniSaDirectory, "output directory, current directory if empty", "--out", false);
		}

		/// <summary>
		/// executes the program
		/// </summary>
		protected override void ProgramExecution()
		{
			var extractor = new MiniSaExtractor(ConfigurationSettings.CompressedReference, ConfigurationSettings.InputSuppAnnotPath, ConfigurationSettings.Begin, ConfigurationSettings.End, ConfigurationSettings.DataSourceName, ConfigurationSettings.MiniSaDirectory);
			var count = extractor.Extract();

			Console.WriteLine("Extracted {0} supplementary annotations", count);
		}
		
		public static int Run(string command,string[] commandArgs)
		{
			var ops = new OptionSet
			{
				{
					 "ref|r=",
					 "compressed reference sequence file",
					 v => ConfigurationSettings.CompressedReference = v
				 },
				{
					"in|i=",
					"input Nirvana Supplementary Annotations {file}",
					v => ConfigurationSettings.InputSuppAnnotPath = v
				},
				{
					"name|n=",
					"data source {name}",
					v => ConfigurationSettings.DataSourceName = v
				},
				{
					"begin|b=",
					"reference begin {position}",
					(int v) => ConfigurationSettings.Begin= v
				},
				{
					"end|e=",
					"reference end {allele}",
					(int v) => ConfigurationSettings.End= v
				},
				{
					"out|o=",
					"output {directory}",
					v => ConfigurationSettings.MiniSaDirectory= v
				}
			};

			var commandLineExample = $"{command} --in <Supplementary Annotations path> --out <Supplementary Annotations Directory> --begin <position> --end <position> --name <dataSource>";

			var extractor = new ExtractMiniSaMain("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", ops, commandLineExample, Constants.Authors);
			extractor.Execute(commandArgs);
			return extractor.ExitCode;
		}
	}
}
