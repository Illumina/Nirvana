using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;


namespace SAUtils.CreateOmimDatabase
{
	public class CreateOmimDatabaseMain
	{


		private ExitCodes ProgramExecution()
		{
			var omimDatabaseCreator = new CreateOmimDatabase(ConfigurationSettings.OmimFile,ConfigurationSettings.OutputOmimDirectory);
            omimDatabaseCreator.Create();

		    return ExitCodes.Success;
		}

		public static ExitCodes Run(string command, string[] commandArgs)
		{
            var creator = new CreateOmimDatabaseMain();
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

            var exitCode = new ConsoleAppBuilder(commandArgs,ops)
                .Parse().CheckInputFilenameExists(ConfigurationSettings.OmimFile, "Omim GeneMap file", "--mim")
                .CheckAndCreateDirectory(ConfigurationSettings.OutputOmimDirectory, "output Omim database directory", "--out")
                .ShowBanner(null).ShowHelpMenu("Reads omim gene map file and creates Omim database",commandLineExample)
                .ShowErrors()
                .Execute(creator.ProgramExecution);

			return exitCode;
		}
	}
}