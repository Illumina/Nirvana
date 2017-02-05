using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateOmimDatabase
{
	public class CreateOmimDatabaseMain: AbstractCommandLineHandler
	{
		public CreateOmimDatabaseMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
		{
		}

		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.OmimFile, "Omim GeneMap file", "--mim");
			CheckAndCreateDirectory(ConfigurationSettings.OutputOmimDirectory, "output Omim database directory", "--out");
		}

		protected override void ProgramExecution()
		{
			var omimDatabaseCreator = new CreateOmimDatabase(ConfigurationSettings.OmimFile,ConfigurationSettings.OutputOmimDirectory);
            omimDatabaseCreator.Create();
		}
	}
}