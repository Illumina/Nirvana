using System.Collections.Generic;
using CacheUtils.Commands.ParseVepCacheDirectory;
using CacheUtils.GFF;
using CacheUtils.Helpers;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using Genome;
using IO;
using ReferenceSequence.Utilities;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Providers;

namespace CacheUtils.Commands.GFF
{
    public static class CreateGffMain
    {
        private static string _compressedReferencePath;
        private static string _inputPrefix;
        private static string _outputFileName;
        private static string _transcriptSource;

        private static ExitCodes ProgramExecution()
        {
            Source transcriptSource = ParseVepCacheDirectoryMain.GetSource(_transcriptSource);
            string cachePath        = CacheConstants.TranscriptPath(_inputPrefix);

            Dictionary<ushort, Chromosome> refIndexToChromosome =
                SequenceHelper.GetDictionaries(_compressedReferencePath).refIndexToChromosome;
            
            TranscriptCacheData     cache            = TranscriptCacheHelper.GetCache(cachePath, refIndexToChromosome);
            Dictionary<IGene, int> geneToInternalId = InternalGenes.CreateDictionary(cache.Genes);

            using (var writer = new GffWriter(GZipUtilities.GetStreamWriter(_outputFileName)))
            {
                var creator = new GffCreator(writer, geneToInternalId, transcriptSource);
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
                    "source|s=",
                    "transcript {source}",
                    v => _transcriptSource = v
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
