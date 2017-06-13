using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using VariantAnnotation.DataStructures;


namespace SAUtils.CreateOmimDatabase
{
	public class CreateOmimDatabaseMain: AbstractCommandLineHandler
	{
		private CreateOmimDatabaseMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
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

		public static int Run(string command, string[] commandArgs)
		{
			var ops = new OptionSet
			{

				 {
					 "mim|m=",
					 "input genemap file",
					 v => ConfigurationSettings.OmimFile =v
				 },
				 {
					 "out|o=",
					 "output Nirvana Omim directory",
					 v => ConfigurationSettings.OutputOmimDirectory = v
				 }
			};

			var commandLineExample = $"{command} [options]";
			var converter = new CreateOmimDatabaseMain("Reads omim gene map file and creates Omim database", ops, commandLineExample, Constants.Authors);
			converter.Execute(commandArgs);
			return converter.ExitCode;
		}
	}
}