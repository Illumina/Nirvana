using System;
using VariantAnnotation.DataStructures;
using NDesk.Options;
using VariantAnnotation.CommandLine;

namespace ExtractCustomIntervals
{
	public sealed class ExtractCustomInterval : AbstractCommandLineHandler
	{
		// constructor
	    private ExtractCustomInterval(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

		/// <summary>
		/// validates the command line
		/// </summary>
		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.InputCustIntervalPath, "Nirvana custom interval database file", "--in");
			CheckDirectoryExists(ConfigurationSettings.OutputCustIntervalDbPath, "output mini Custom Interval database directory", "--out");
		}

		/// <summary>
		/// executes the program
		/// </summary>
		protected override void ProgramExecution()
		{
			var extractor = new CustomIntervalExtractor(ConfigurationSettings.InputCustIntervalPath, ConfigurationSettings.OutputCustIntervalDbPath, ConfigurationSettings.Begin, ConfigurationSettings.End);
			var count = extractor.Extract();

			Console.WriteLine("Extracted {0} custom intervals", count);
		}

		static int Main(string[] args)
		{
			var ops = new OptionSet
			{
				{
					"i|in=",
					"input Nirvana Custom Interval database {file}",
					v => ConfigurationSettings.InputCustIntervalPath = v
				},
				{
					"o|out=",
					"output mini Custom Interval database directory {dir}",
					v => ConfigurationSettings.OutputCustIntervalDbPath = v
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
				}
			};

			var commandLineExample = "--in <custom interval path> --out <mini custom interval db path> --begin <position> --end <position>";

			var extractor = new ExtractCustomInterval("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", ops, commandLineExample, Constants.Authors);
			extractor.Execute(args);
			return extractor.ExitCode;
		}
	}

	
}
