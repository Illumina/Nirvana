using CacheUtils.GFF;
using CacheUtils.Helpers;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;

namespace CacheUtils.Commands.GFF
{
    public static class CreateGffMain
    {
        private static string _compressedReferencePath;
        private static string _inputPrefix;
        private static string _outputFileName;

        private static ExitCodes ProgramExecution()
        {
            string cachePath                 = CacheConstants.TranscriptPath(_inputPrefix);
            var (refIndexToChromosome, _, _) = SequenceHelper.GetDictionaries(_compressedReferencePath);
            var cache                        = TranscriptCacheHelper.GetCache(cachePath, refIndexToChromosome);
            var geneToInternalId             = InternalGenes.CreateDictionary(cache.Genes);

            using (var writer = new GffWriter(GZipUtilities.GetStreamWriter(_outputFileName)))
            {
                var creator = new GffCreator(writer, geneToInternalId);
                creator.Create(cache.TranscriptIntervalArrays);
            }

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

            string commandLineExample = $"{command} --in <cache prefix> --out <GFF path>";

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
