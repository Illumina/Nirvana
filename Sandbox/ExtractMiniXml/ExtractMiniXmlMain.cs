using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Utilities;

namespace ExtractMiniXml
{
    sealed class ExtractMiniXmlMain:AbstractCommandLineHandler
	{
		static int Main(string[] args)
		{
			var ops = new OptionSet()
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

			var commandLineExample = "--in <xml file> --out <output Directory> --rcv <RCV ID>";

			var extractor = new ExtractMiniXmlMain("Extracts mini XML for the given RCV ID from Original clinvar xml files.", ops, commandLineExample, Constants.Authors);
			extractor.Execute(args);
			return extractor.ExitCode;

		}

		public ExtractMiniXmlMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
		{
		}

		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.InputXmlFile, "Input Xml File", "--in");
			HasRequiredParameter(ConfigurationSettings.RcvId, "RCV need to be extracted", "--rcv");
			CheckDirectoryExists(ConfigurationSettings.OutputDir, "output directory, current directory if empty", "--out", false);
		}

		protected override void ProgramExecution()
		{
			var extractor = new XmlExtractor(ConfigurationSettings.InputXmlFile,ConfigurationSettings.RcvId,ConfigurationSettings.OutputDir);
			extractor.Extract();

			
		}
	}
}
