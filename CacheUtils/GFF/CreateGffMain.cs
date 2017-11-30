using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using VariantAnnotation.Providers;

namespace CacheUtils.GFF
{
    public static class CreateGffMain
    {
        private static string _compressedReferencePath;
        private static string _inputPrefix;
        private static string _outputFileName;

        private static ExitCodes ProgramExecution()
        {
            var creator = new GffCreator(_inputPrefix, _compressedReferencePath);
            creator.Create(_outputFileName);

            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input cache {prefix}",
                    v => _inputPrefix = v
                },
                {
                    "out|o=",
                    "output {file name}",
                    v => _outputFileName = v
                },
                {
                    "ref|r=",
                    "reference {file}",
                    v => _compressedReferencePath = v
                }
            };

            var commandLineExample = $"{command} --in <cache prefix> --out <GFF path>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_inputPrefix, "input cache prefix", "--in")
                .CheckOutputFilenameSuffix(_outputFileName, ".gz", "GFF")
                .SkipBanner()
                .ShowHelpMenu("Outputs exon coordinates for all transcripts in a database.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
