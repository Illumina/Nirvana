using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;

namespace CacheUtils.UpdateMiniCacheFiles
{
    sealed class UpdateMiniCacheFilesMain : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "root=",
                    "input cache {root}",
                    v => ConfigurationSettings.CacheRoot = v
                },
                {
                    "in|i=",
                    "input mini-cache {directory}",
                    v => ConfigurationSettings.MiniCacheDirectory = v
                },
                {
                    "ref|r=",
                    "input reference {directory}",
                    v => ConfigurationSettings.ReferenceDirectory = v
                },
                {
                    "vep=",
                    "new VEP {version}",
                    (ushort v) => ConfigurationSettings.NewVepVersion = v
                }
            };

            var commandLineExample = $"{command} -i <mini-cache dir> --root <cache root> -v <VEP version>";

            var update = new UpdateMiniCacheFilesMain("Updates the mini-cache files", ops, commandLineExample, Constants.Authors);
            update.Execute(args);
            return update.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private UpdateMiniCacheFilesMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.MiniCacheDirectory, "cache", "--in");
            CheckDirectoryExists(ConfigurationSettings.ReferenceDirectory, "reference", "--ref");
            CheckDirectoryExists(ConfigurationSettings.CacheRoot, "cache root", "--root");
            HasRequiredParameter(ConfigurationSettings.NewVepVersion, "VEP version", "--vep");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var crawler = new UnitTestResourceCrawler(ConfigurationSettings.CacheRoot,
                ConfigurationSettings.ReferenceDirectory, ConfigurationSettings.NewVepVersion);

            crawler.Process(ConfigurationSettings.MiniCacheDirectory);
        }
    }
}
