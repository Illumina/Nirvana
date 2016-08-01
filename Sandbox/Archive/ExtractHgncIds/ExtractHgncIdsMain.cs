using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace ExtractHgncIds
{
    class ExtractHgncIdsMain : AbstractCommandLineHandler
    {
        // constructor
        private ExtractHgncIdsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.CacheDirectory, "cache", "--dir");
            CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference", "--ref");
            HasRequiredParameter(ConfigurationSettings.OutputFilename, "output filename", "--out");
        }

	    /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var annotator = new HgncExtractor();
            annotator.DumpIds(ConfigurationSettings.CacheDirectory, ConfigurationSettings.InputReferencePath, ConfigurationSettings.OutputFilename);
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "dir|d=",
                    "input cache {directory}",
                    v => ConfigurationSettings.CacheDirectory = v
                },
                {
                    "out|o=",
                    "output {filename}",
                    v => ConfigurationSettings.OutputFilename = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => ConfigurationSettings.InputReferencePath = v
                }
            };

            var commandLineExample = "-d <cache dir>";

            var extractor = new ExtractHgncIdsMain("Checks the discrepancies between Nirvana and the baseline", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }
    }
}
