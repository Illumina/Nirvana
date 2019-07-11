using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.GnomadGeneScores
{
    public static class GnomadGenesMain
    {
        private static string _outputDirectory;
        private static string _inputFile;
        private static string _cachePrefix;
        private static string _referenceSequncePath;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "cache|c=",
                    "Cache prefix",
                    v => _cachePrefix = v
                },
                {
                    "ref|r=",
                    "Reference sequence path",
                    v => _referenceSequncePath = v
                },
                {
                    "in|i=",
                    "input tsv file",
                    v => _inputFile = v
                },
                {
                    "out|o=",
                    "output directory",
                    v => _outputDirectory = v
                }
            };

            string commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .HasRequiredParameter(_cachePrefix, "transcript cache prefix", "--cache")
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(_cachePrefix), "transcript cache prefix", "--cache")
                .HasRequiredParameter(_referenceSequncePath, "reference sequence path", "--ref")
                .CheckInputFilenameExists(_referenceSequncePath, "reference sequence path", "--ref")
                .CheckInputFilenameExists(_inputFile, "input TSV file", "--in")
                .SkipBanner()
                .ShowHelpMenu("Creates a gene annotation database from gnomAD data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            Dictionary<string, string> geneIdToSymbols;
            using (var cacheStream = FileUtilities.GetReadStream(CacheConstants.TranscriptPath(_cachePrefix)))
            using (var transcriptCacheReader = new TranscriptCacheReader(cacheStream))
            using (var refProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_referenceSequncePath)))
            {
                geneIdToSymbols = LoadGenesFromCache(refProvider, transcriptCacheReader);
                Console.WriteLine($"Loaded {geneIdToSymbols.Count} gene symbols from cache.");
            }

            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            var outFileName = $"{version.Name}_{version.Version}";

            using (var gnomadGeneParser = new GnomadGeneParser(GZipUtilities.GetAppropriateStreamReader(_inputFile), geneIdToSymbols))
            using (var stream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.NgaFileSuffix)))
            using (var ngaWriter = new NgaWriter(stream, version, SaCommon.GnomadGeneScoreTag, SaCommon.SchemaVersion, false))
            {
                ngaWriter.Write(gnomadGeneParser.GetItems());
            }

            return ExitCodes.Success;
        }

        private static Dictionary<string, string> LoadGenesFromCache(ReferenceSequenceProvider refProvider, TranscriptCacheReader cacheReader)
        {
            var transcriptData = cacheReader.Read(refProvider.RefIndexToChromosome);

            var geneIdToSymbols = new Dictionary<string, string>(transcriptData.Genes.Length);
            foreach (var gene in transcriptData.Genes)
            {
                var geneId = gene.EnsemblId.WithoutVersion;
                //if(geneId == "ENSG00000272962" || geneId == "ENSG00000198743")
                //    Console.WriteLine("bug");
                if (string.IsNullOrEmpty(geneId)) continue;

                if (! geneIdToSymbols.TryAdd(geneId, gene.Symbol))
                {
                    if(geneIdToSymbols[geneId] != gene.Symbol)
                        throw new DataMisalignedException($"Multiple symbols found for {geneId}");
                };
            }

            return geneIdToSymbols;
        }
    }
}