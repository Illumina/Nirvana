using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace ExtractRegulatoryFeatures
{
    class ExtractRegulatoryFeaturesMain : AbstractCommandLineHandler
    {
        // constructor
        private ExtractRegulatoryFeaturesMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputCachePath, "Nirvana cache", "--in");
            HasRequiredParameter(ConfigurationSettings.OutputCachePath, "output cache", "--out");
            HasRequiredParameter(ConfigurationSettings.TargetRegulatoryFeatureId, "regulatory ID", "--regulatory");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var extractor = new RegulatoryFeatureExtractor(ConfigurationSettings.InputCachePath);
            extractor.Extract(ConfigurationSettings.TargetRegulatoryFeatureId, ConfigurationSettings.OutputCachePath);
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input Nirvana cache {file}",
                    v => ConfigurationSettings.InputCachePath = v
                },
                {
                    "out|o=",
                    "output Nirvana cache {file}",
                    v => ConfigurationSettings.OutputCachePath = v
                },
                {
                    "regulatory|r=",
                    "regulatory feature {ID}",
                    v => ConfigurationSettings.TargetRegulatoryFeatureId = v
                }
            };

            var commandLineExample = "--in <cache path> --out <cache path> --regulatory <regulatory feature ID>";

            var extractor = new ExtractRegulatoryFeaturesMain("Extracts regulatory features from Nirvana cache files.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }
    }
}
