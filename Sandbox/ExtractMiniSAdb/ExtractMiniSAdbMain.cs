using System;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;

namespace ExtractMiniSAdB
{
    sealed class ExtractMiniSAdbMain : AbstractCommandLineHandler
	{
		// constructor
	    private ExtractMiniSAdbMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
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
			var extractor = new SuppAnnotExtractor(ConfigurationSettings.CompressedReference, ConfigurationSettings.InputSuppAnnotPath, ConfigurationSettings.Begin, ConfigurationSettings.End, ConfigurationSettings.DataSourceName, ConfigurationSettings.MiniSaDirectory);
			var count = extractor.Extract();

			Console.WriteLine("Extracted {0} supplementary annotations", count);
		}

		static int Main(string[] args)
		{
			var ops = new OptionSet
			{
				{
					 "r|ref=",
					 "compressed reference sequence file",
					 v => ConfigurationSettings.CompressedReference = v
				 },
				{
					"i|in=",
					"input Nirvana Supplementary Annotations {file}",
					v => ConfigurationSettings.InputSuppAnnotPath = v
				},
				{
					"n|name=",
					"data source {name}",
					v => ConfigurationSettings.DataSourceName = v
				},
				{
					"b|begin=",
					"reference begin {position}",
					(int v) => ConfigurationSettings.Begin= v
				},
				{
					"e|end=",
					"reference end {allele}",
					(int v) => ConfigurationSettings.End= v
				},
				{
					"o|out=",
					"output {directory}",
					v => ConfigurationSettings.MiniSaDirectory= v
				}
			};

			var commandLineExample = "--in <Supplementary Annotations path> --out <Supplementary Annotations Directory> --begin <position> --end <position> --name <dataSource>";

			var extractor = new ExtractMiniSAdbMain("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", ops, commandLineExample, Constants.Authors);
			extractor.Execute(args);
			return extractor.ExitCode;
		}
	}
}
