using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;

namespace CacheUtils.CombineCacheDirectories
{
    sealed class CombineCacheDirectoriesMain : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|1=",
                    "input cache {prefix}",
                    v => ConfigurationSettings.InputPrefix = v
                },
                {
                    "in2|2=",
                    "input cache 2 {prefix}",
                    v => ConfigurationSettings.InputPrefix2 = v
                },
                {
                    "out|o=",
                    "output cache {prefix}",
                    v => ConfigurationSettings.OutputPrefix = v
                }
            };

            var commandLineExample = $"{command} --in <cache prefix> --in2 <cache prefix> --out <cache prefix>";

            var combiner = new CombineCacheDirectoriesMain("Combines two cache sets into one cache.", ops, commandLineExample, Constants.Authors);
            combiner.Execute(args);
            return combiner.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private CombineCacheDirectoriesMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }


        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            //CheckDirectoryExists(ConfigurationSettings.InputPrefix, "input cache", "--in");
            //CheckDirectoryExists(ConfigurationSettings.InputPrefix2, "input cache 2", "--in2");
            //CheckDirectoryExists(ConfigurationSettings.OutputPrefix, "output cache", "--out");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var combiner = new CacheCombiner(ConfigurationSettings.InputPrefix, ConfigurationSettings.InputPrefix2,
                ConfigurationSettings.OutputPrefix);

            combiner.CheckCacheIntegrity();
            combiner.Combine();
        }
    }
}
