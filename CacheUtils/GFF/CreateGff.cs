using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using VariantAnnotation.DataStructures;

namespace CacheUtils.GFF
{
    public sealed class CreateGff : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input cache {prefix}",
                    v => ConfigurationSettings.CachePrefix = v
                },
                {
                    "out|o=",
                    "output {file name}",
                    v => ConfigurationSettings.OutputFileName = v
                },
                {
                    "ref|r=",
                    "reference {file}",
                    v => ConfigurationSettings.CompressedReferencePath = v
                }
            };

            var commandLineExample = $"{command} --in <cache prefix> --out <GFF path>";

            var extractor = new CreateGff("Outputs exon coordinates for all transcripts in a database.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        private CreateGff(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {}

        protected override void ValidateCommandLine()
        {
            HasRequiredParameter(ConfigurationSettings.CachePrefix, "input cache prefix", "--in");
            CheckOutputFilenameSuffix(ConfigurationSettings.OutputFileName, ".gz", "GFF");
        }

        protected override void ProgramExecution()
        {
            var creator = new GffCreator(ConfigurationSettings.CachePrefix,
                ConfigurationSettings.CompressedReferencePath);
            creator.Create(ConfigurationSettings.OutputFileName);
        }
    }
}
