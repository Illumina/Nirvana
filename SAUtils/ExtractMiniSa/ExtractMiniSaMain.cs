using System;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.ExtractMiniSa
{
    internal static class ExtractMiniSaMain 
	{
	    #region members

	    // filenames
	    private  static string _compressedReference;
	    private  static string _inputSuppAnnotPath;
	    private  static string _dataSourceName;

	    private  static int _begin;
	    private  static int _end;
	    private  static string _miniSaDirectory;

	    #endregion

        /// <summary>
        /// executes the program
        /// </summary>
        private static ExitCodes ProgramExecution()
		{
			var extractor = new MiniSaExtractor(_compressedReference, _inputSuppAnnotPath, _begin, _end, _dataSourceName, _miniSaDirectory);
			var count = extractor.Extract();

			Console.WriteLine("Extracted {0} supplementary annotations", count);

		    return ExitCodes.Success;
		}
		
		public static ExitCodes Run(string command,string[] commandArgs)
		{
            
			var ops = new OptionSet
			{
				{
					 "ref|r=",
					 "compressed reference sequence file",
					 v => _compressedReference = v
				 },
				{
					"in|i=",
					"input Nirvana Supplementary Annotations {file}",
					v => _inputSuppAnnotPath = v
				},
				{
					"name|n=",
					"data source {name}",
					v => _dataSourceName = v
				},
				{
					"begin|b=",
					"reference begin {position}",
					(int v) => _begin= v
				},
				{
					"end|e=",
					"reference end {allele}",
					(int v) => _end= v
				},
				{
					"out|o=",
					"output {directory}",
					v => _miniSaDirectory= v
				}
			};

			var commandLineExample = $"{command} --in <Supplementary Annotations path> --out <Supplementary Annotations Directory> --begin <position> --end <position> --name <dataSource>";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_inputSuppAnnotPath, "Nirvana supplementary annotations", "--in")
                .CheckInputFilenameExists(_compressedReference, "Compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_miniSaDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
	    }
	}
}
