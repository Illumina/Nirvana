using System;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using VariantAnnotation.Interface;

namespace SAUtils.ExtractMiniSa
{
    sealed class ExtractMiniSaMain 
	{
	

		/// <summary>
		/// executes the program
		/// </summary>
		private ExitCodes ProgramExecution()
		{
			var extractor = new MiniSaExtractor(ConfigurationSettings.CompressedReference, ConfigurationSettings.InputSuppAnnotPath, ConfigurationSettings.Begin, ConfigurationSettings.End, ConfigurationSettings.DataSourceName, ConfigurationSettings.MiniSaDirectory);
			var count = extractor.Extract();

			Console.WriteLine("Extracted {0} supplementary annotations", count);

		    return ExitCodes.Success;
		}
		
		public static ExitCodes Run(string command,string[] commandArgs)
		{
            var extractor = new ExtractMiniSaMain();

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

	        var exitCode = new ConsoleAppBuilder(commandArgs, ops)
	            .Parse()
	            .CheckInputFilenameExists(ConfigurationSettings.InputSuppAnnotPath, "Nirvana supplementary annotations",
	                "--in")
	            .CheckInputFilenameExists(ConfigurationSettings.CompressedReference,
	                "Compressed reference sequence file name", "--ref")
	            .HasRequiredParameter(ConfigurationSettings.MiniSaDirectory, "output directory", "--out")
	            .ShowBanner(Constants.Authors)
	            .ShowHelpMenu(
	                "Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.",
	                commandLineExample)
	            .ShowErrors()
	            .Execute(extractor.ProgramExecution);
	        ;
	        return exitCode;
	    }
	}
}
