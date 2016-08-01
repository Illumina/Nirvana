using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;
using Nirvana;

namespace UpdateMiniCacheFiles
{
    class UpdateMiniCacheFiles : AbstractCommandLineHandler
    {
        // constructor
        public UpdateMiniCacheFiles(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.MiniCacheDirectory, "cache", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference", "--ref");
            CheckDirectoryExists(ConfigurationSettings.RootCacheDirectory, "root cache", "--cache");
            CheckDirectoryExists(ConfigurationSettings.RootUnfilteredCacheDirectory, "root unfiltered cache", "--unfiltered");
            HasRequiredParameter(ConfigurationSettings.DesiredVepVersion, "VEP version", "--vep");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var updater = new MiniCacheUpdaterMain(ConfigurationSettings.RootCacheDirectory,
                ConfigurationSettings.RootUnfilteredCacheDirectory, ConfigurationSettings.DesiredVepVersion,
                ConfigurationSettings.InputReferencePath);

            updater.Process(ConfigurationSettings.MiniCacheDirectory);
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "cache|c=",
                    "root cache {directory}",
                    v => ConfigurationSettings.RootCacheDirectory = v
                },
                {
                    "in|i=",
                    "input mini-cache {directory}",
                    v => ConfigurationSettings.MiniCacheDirectory = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => ConfigurationSettings.InputReferencePath = v
                },
                {
                    "unfiltered|u=",
                    "root unfiltered cache {directory}",
                    v => ConfigurationSettings.RootUnfilteredCacheDirectory = v
                },
                {
                    "vep=",
                    "desired VEP {version}",
                    (ushort v) => ConfigurationSettings.DesiredVepVersion = v
                }
            };

            var commandLineExample = "-i <mini-cache dir> -v <VEP version>";

            var update = new UpdateMiniCacheFiles("Updates the mini-cache files", ops, commandLineExample, Constants.Authors);
            update.Execute(args);
            return update.ExitCode;
        }
    }
}
