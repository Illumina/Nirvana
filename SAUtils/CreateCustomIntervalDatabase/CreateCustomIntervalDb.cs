using VariantAnnotation.FileHandling;
using NDesk.Options;
using VariantAnnotation.CommandLine;

namespace SAUtils.CreateCustomIntervalDatabase
{
	public class CreateCustomIntervalDb : AbstractCommandLineHandler
	{
        public static int Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "r|ref=",
                     "compressed reference sequence file",
                     v => ConfigurationSettings.CompressedReference = v
                 },
                 {
                     "b|bed=",
                     "input bed file",
                     v => ConfigurationSettings.BedFile =v
                 },
                 {
                     "o|out=",
                     "output Nirvana Supplementary directory",
                     v => ConfigurationSettings.OutputDirectory = v
                 }
            };

            var commandLineExample = $"{command} [options]";

            var converter = new CreateCustomIntervalDb("Reads provided bed file and creates custom interval database", ops, commandLineExample, VariantAnnotation.DataStructures.Constants.Authors);
            converter.Execute(commandArgs);
            return converter.ExitCode;
        }

        // constructor
        public CreateCustomIntervalDb(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
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
			// load the reference sequence
			AnnotationLoader.Instance.LoadCompressedSequence(ConfigurationSettings.CompressedReference);

			var customIntervalDbCreator = new CustomIntervalDbCreator(ConfigurationSettings.BedFile, ConfigurationSettings.OutputDirectory);

			customIntervalDbCreator.Create();
		}

	}
}
