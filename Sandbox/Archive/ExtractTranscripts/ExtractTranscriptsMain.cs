using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace ExtractTranscripts
{
    class ExtractTranscriptsMain : AbstractCommandLineHandler
    {
        #region members

        private bool _hasVariantTarget;
        private bool _hasTranscriptTarget;
        private bool _hasVcfLine;

        #endregion

        // constructor
        private ExtractTranscriptsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            _hasVariantTarget = !string.IsNullOrEmpty(ConfigurationSettings.ReferenceName) &&
                        !string.IsNullOrEmpty(ConfigurationSettings.ReferenceAllele) &&
                        (ConfigurationSettings.AlternateAlleles.Count > 0) &&
                        (ConfigurationSettings.ReferencePosition != ConfigurationSettings.DefaultIntValue);

            _hasTranscriptTarget = !string.IsNullOrEmpty(ConfigurationSettings.TargetTranscriptId);

            _hasVcfLine = !string.IsNullOrEmpty(ConfigurationSettings.VcfLine);

            CheckInputFilenameExists(ConfigurationSettings.InputCachePath, "Nirvana cache", "--in");
            HasRequiredParameter(ConfigurationSettings.OutputCachePath, "output cache file", "--out");

            // make sure we have exactly one option selected
            int numSelectedOptions = 0;
            if (_hasVariantTarget)    numSelectedOptions++;
            if (_hasTranscriptTarget) numSelectedOptions++;
            if (_hasVcfLine)          numSelectedOptions++;

            HasOnlyOneOption(numSelectedOptions, "--vcf, --transcript, or the combination of --name,> --pos, --ref, and --alt");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var extractor = new TranscriptExtractor(ConfigurationSettings.InputCachePath);

            // set our options
            if (_hasTranscriptTarget)
            {
                extractor.SetTranscriptTarget(ConfigurationSettings.TargetTranscriptId);
            }
            else if (_hasVcfLine)
            {
                extractor.SetVcfLine(ConfigurationSettings.VcfLine);
            }
            else if (_hasVariantTarget)
            {
                extractor.SetVariantTarget(ConfigurationSettings.ReferenceName,
                    ConfigurationSettings.ReferencePosition, ConfigurationSettings.ReferenceAllele,
                    ConfigurationSettings.AlternateAlleles);
            }

            extractor.Extract(ConfigurationSettings.OutputCachePath);
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "alt|a=",
                    "alternate {alleles}",
                    v => ConfigurationSettings.AlternateAlleles.Add(v)
                },
                {
                    "in|i=",
                    "input Nirvana cache {file}",
                    v => ConfigurationSettings.InputCachePath = v
                },
                {
                    "name|n=",
                    "reference {name}",
                    v => ConfigurationSettings.ReferenceName = v
                },
                {
                    "out|o=",
                    "output Nirvana cache {file}",
                    v => ConfigurationSettings.OutputCachePath = v
                },
                {
                    "pos|p=",
                    "reference {position}",
                    (int v) => ConfigurationSettings.ReferencePosition = v
                },
                {
                    "ref|r=",
                    "reference {allele}",
                    v => ConfigurationSettings.ReferenceAllele = v
                },
                {
                    "transcript|t=",
                    "transcript {ID}",
                    v => ConfigurationSettings.TargetTranscriptId = v
                },
                {
                    "vcf=",
                    "vcf {line}",
                    v => ConfigurationSettings.VcfLine = v
                }
            };

            var commandLineExample = "";

            var extractor = new ExtractTranscriptsMain("", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }
    }
}
