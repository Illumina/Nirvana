using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.ExtractMiniXml
{
    public static class ExtractMiniXmlMain
	{
	    private  static string _inputXmlFile;
	    private  static string _rcvId;
	    private  static string _outputDir;

	    private static ExitCodes ProgramExecution()
        {
	        var extractor = new XmlExtractor(_inputXmlFile, _rcvId, _outputDir);
	        extractor.Extract();

            return ExitCodes.Success;
        }
        public static ExitCodes Run(string command, string[] commandArgs)
        {
			var ops = new OptionSet
			{
				{
					"i|in=",
					"Input XML {file}",
					v => _inputXmlFile = v
				},
				{
					"r|rcv=",
					"RCV ID",
					v => _rcvId = v
				},
				{
					"o|out=",
					"Output {dir}",
					v => _outputDir = v
				}
			};

			var commandLineExample = $"{command} --in <xml file> --out <output Directory> --rcv <RCV ID>";

			var exitCode = new ConsoleAppBuilder(commandArgs, ops)
	            .Parse()
	            .CheckInputFilenameExists(_inputXmlFile, "input XML file", "--in")
	            .HasRequiredParameter<string>(_outputDir, "output directory", "--out")
			    .SkipBanner()
                .ShowHelpMenu("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", commandLineExample)
	            .ShowErrors()
	            .Execute(ProgramExecution);
	        
	        return exitCode;
		}
	}
}
