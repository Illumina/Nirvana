using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateCustomIntervalDatabase
{
	public class CreateCustomIntervalDbMain : AbstractCommandLineHandler
	{
		// constructor
		public CreateCustomIntervalDbMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

		/// <summary>
		/// validates the command line
		/// </summary>
		protected override void ValidateCommandLine()
		{
			CheckInputFilenameExists(ConfigurationSettings.CompressedReference, "Compressed reference sequence file name", "--ref");

			CheckAndCreateDirectory(ConfigurationSettings.OutputDirectory, "output custom interval directory", "--out");

			CheckInputFilenameExists(ConfigurationSettings.BedFile, "Custom interval bed file name", "--bed", false);
			
			CheckNonZero(ConfigurationSettings.NumberOfProvidedInputFiles(), "custom interval data source");
		}

		/// <summary>
		/// executes the program
		/// </summary>
		protected override void ProgramExecution()
		{
		    var renamer = ChromosomeRenamer.GetChromosomeRenamer(FileUtilities.GetReadStream(ConfigurationSettings.CompressedReference));
			var customIntervalDbCreator = new CustomIntervalDbCreator(ConfigurationSettings.BedFile, ConfigurationSettings.OutputDirectory, renamer);

			customIntervalDbCreator.Create();
		}

	}
}
