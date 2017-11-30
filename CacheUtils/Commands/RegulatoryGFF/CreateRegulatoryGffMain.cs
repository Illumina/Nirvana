using System;
using System.IO;
using CacheUtils.Helpers;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;

namespace CacheUtils.Commands.RegulatoryGFF
{
    public static class CreateRegulatoryGffMain
    {
        private static string _referencePath;
        private static string _inputPrefix;
        private static string _outputFileName;

        private static ExitCodes ProgramExecution()
        {
            using (var writer = GZipUtilities.GetStreamWriter(_outputFileName))
            {
                var cachePath    = CacheConstants.TranscriptPath(_inputPrefix);
                var sequenceData = SequenceHelper.GetDictionaries(_referencePath);

                // load the cache
                Console.Write("- reading {0}... ", Path.GetFileName(cachePath));
                var cache = TranscriptCacheHelper.GetCache(cachePath, sequenceData.refIndexToChromosome);
                Console.WriteLine("found {0:N0} reference sequences. ", cache.RegulatoryRegionIntervalArrays.Length);

                Console.Write("- writing GFF entries... ");
                foreach (var intervalArray in cache.RegulatoryRegionIntervalArrays)
                {
                    if (intervalArray == null) continue;
                    foreach (var interval in intervalArray.Array) WriteRegulatoryFeature(writer, interval.Value);
                }                
                Console.WriteLine("finished.");
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
                    v => _referencePath = v
                }
            };

            var commandLineExample = $"{command} --in <cache prefix> --out <GFF path>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_inputPrefix, "input cache prefix", "--in")
                .CheckOutputFilenameSuffix(_outputFileName, ".gz", "GFF")
                .SkipBanner()
                .ShowHelpMenu("Outputs regulatory regions in a database.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }

        private static void WriteRegulatoryFeature(TextWriter writer, IRegulatoryRegion regulatoryRegion)
        {
            writer.Write($"{regulatoryRegion.Chromosome.UcscName}\t.\tregulatory feature\t{regulatoryRegion.Start}\t{regulatoryRegion.End}\t.\t.\t.\t");
            WriteGeneralAttributes(writer, regulatoryRegion);
            writer.WriteLine();
        }

        private static void WriteGeneralAttributes(TextWriter writer, IRegulatoryRegion regulatoryRegion)
        {
            if (!regulatoryRegion.Id.IsEmpty()) writer.Write($"regulatory_feature_id \"{regulatoryRegion.Id}\"; ");
            writer.Write($"regulatory_feature_type \"{regulatoryRegion.Type}\"; ");
        }
    }
}
