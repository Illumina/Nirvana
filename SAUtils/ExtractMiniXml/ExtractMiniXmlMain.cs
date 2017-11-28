using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using ExtractMiniXml;
using VariantAnnotation.Interface;

namespace SAUtils.ExtractMiniXml
{
    public static class ExtractMiniXmlMain
	{
	    private static ExitCodes ProgramExecution()
        {
	        var extractor = new XmlExtractor(ConfigurationSettings.InputXmlFile, ConfigurationSettings.RcvId, ConfigurationSettings.OutputDir);
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
					v => ConfigurationSettings.InputXmlFile = v
				},
				{
					"r|rcv=",
					"RCV ID",
					v => ConfigurationSettings.RcvId = v
				},
				{
					"o|out=",
					"Output {dir}",
					v => ConfigurationSettings.OutputDir = v
				}
			};

			var commandLineExample = $"{command} --in <xml file> --out <output Directory> --rcv <RCV ID>";

			var exitCode = new ConsoleAppBuilder(commandArgs, ops)
	            .Parse()
	            .CheckInputFilenameExists(ConfigurationSettings.InputXmlFile, "input XML file", "--in")
	            .HasRequiredParameter(ConfigurationSettings.OutputDir, "output directory", "--out")
	            .ShowBanner(Constants.Authors)
	            .ShowHelpMenu("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", commandLineExample)
	            .ShowErrors()
	            .Execute(ProgramExecution);
	        
	        return exitCode;
		}		
	}
}
