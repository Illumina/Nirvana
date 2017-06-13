using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using VariantAnnotation.DataStructures;

namespace GenerateVcfFromOmimTests
{
    sealed class GenerateVcfMain : AbstractCommandLineHandler
    {
        static int Main(string[] args)
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
                    "input compressed reference sequence {path}",
                    v => ConfigurationSettings.CompressedReferencePath = v
                }
            };

            var commandLineExample = "--in <input Cache> --out <output file name> --ref <compressed Reference sequence>";

            var extractor = new GenerateVcfMain("Generate a vcf file with one variants in each genes.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        private GenerateVcfMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {
        }

        protected override void ValidateCommandLine()
        {
            HasRequiredParameter(ConfigurationSettings.CachePrefix, "cache prefix", "--in");
            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "compressed reference sequence", "--ref");
            HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out");
        }

        protected override void ProgramExecution()
        {
            var creator = new OmimVcfCreator(ConfigurationSettings.CachePrefix,
                ConfigurationSettings.CompressedReferencePath, ConfigurationSettings.OutputFileName);
            creator.Create();
        }
    }
}
