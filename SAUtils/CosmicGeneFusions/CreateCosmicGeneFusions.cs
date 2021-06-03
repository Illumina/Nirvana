using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using Genome;
using IO;
using SAUtils.CosmicGeneFusions.Cache;
using SAUtils.CosmicGeneFusions.Conversion;
using SAUtils.CosmicGeneFusions.IO;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.CosmicGeneFusions
{
    public static class CreateCosmicGeneFusions
    {
        private static string _transcriptCachePath;
        private static string _cosmicGeneFusionsPath;
        private static string _referencePath;
        private static string _outputDirectory;
        private static string _releaseDate;
        private static string _cosmicVersion;

        private static ExitCodes ProgramExecution()
        {
            Console.Write("- loading reference sequence... ");
            IDictionary<ushort, IChromosome> refIndexToChromosome = ReferenceLoader.GetRefIndexToChromosome(_referencePath);
            Console.WriteLine("finished.");

            Console.Write("- loading transcript cache... ");
            using FileStream cacheStream     = FileUtilities.GetReadStream(_transcriptCachePath);
            var              transcriptCache = TranscriptCache.Create(cacheStream, refIndexToChromosome);
            Console.WriteLine("finished.");
            
            Console.Write("- parsing COSMIC gene fusions... ");
            using StreamReader                            cosmicReader      = GZipUtilities.GetAppropriateStreamReader(_cosmicGeneFusionsPath);
            Dictionary<int, HashSet<RawCosmicGeneFusion>> fusionIdToEntries = CosmicGeneFusionParser.Parse(cosmicReader);
            Console.WriteLine($"{fusionIdToEntries.Count:N0} fusion IDs loaded");

            Console.Write("- converting COSMIC entries... ");
            Dictionary<ulong, string[]> fusionKeyToJson = CosmicConverter.Convert(fusionIdToEntries, transcriptCache);
            Console.WriteLine($"{fusionKeyToJson.Count:N0} gene pairs converted");
            
            DataSourceVersion version = CreateDataSourceVersion(_cosmicVersion, _releaseDate);
            WriteGeneFusions(_outputDirectory, fusionKeyToJson, version);

            Console.WriteLine();
            Console.WriteLine($"Total: {fusionKeyToJson.Count:N0} gene pairs in database.");

            return ExitCodes.Success;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void WriteGeneFusions(string outputDirectory, Dictionary<ulong, string[]> geneKeyToJson, DataSourceVersion version)
        {
            Console.Write("- writing gene fusions SA file... ");
            string    outputPath = Path.Combine(outputDirectory, $"COSMIC_GeneFusions_{version.Version}{SaCommon.GeneFusionJsonSuffix}");
            using var writer     = new GeneFusionJsonWriter(FileUtilities.GetCreateStream(outputPath), "cosmicGeneFusions", version);
            writer.Write(geneKeyToJson);
            Console.WriteLine("finished.");
        }

        internal static DataSourceVersion CreateDataSourceVersion(string version, string releaseDate)
        {
            long releaseTicks = DateTime.Parse(releaseDate).Ticks;
            return new DataSourceVersion("COSMIC gene fusions", version, releaseTicks, "manually curated somatic gene fusions");
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "cache|c=",
                    "transcript cache {path}",
                    v => _transcriptCachePath = v
                },
                {
                    "in|i=",
                    "COSMIC gene fusions {path}",
                    v => _cosmicGeneFusionsPath = v
                },
                {
                    "out|o=",
                    "output {directory}",
                    v => _outputDirectory = v
                },
                {
                    "ref|r=",
                    "input reference sequence {path}",
                    v => _referencePath = v
                },
                {
                    "releaseDate=",
                    "release {date} (YYYY-MM-dd)",
                    v => _releaseDate = v
                },
                {
                    "cosmicVersion=",
                    "COSMIC {version} (e.g. 92)",
                    v => _cosmicVersion = v
                }
            };

            var commandLineExample = $"{command} [options]";

            ExitCodes exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_referencePath,         "reference sequence",  "--ref")
                .CheckInputFilenameExists(_transcriptCachePath,   "transcript cache",    "--cache")
                .CheckInputFilenameExists(_cosmicGeneFusionsPath, "COSMIC gene fusions", "--in")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .HasRequiredDate(_releaseDate, "COSMIC release date", "--date")
                .HasRequiredParameter(_cosmicVersion, "COSMIC version", "--version")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database with COSMIC gene fusion annotations", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }
    }
}