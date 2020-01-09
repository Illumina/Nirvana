using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Caches;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.SpliceAi
{
    public static class SpliceAiDb
    {
        private static string _inputFile;
        private static string _compressedReference;
        private static string _transcriptCachePrefix;
        private static string _outputDirectory;
        private static string _geneInfoFile;
        public static ExitCodes Run(string command, string[] commandArgs)

        {
            var ops = new OptionSet
            {
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _compressedReference = v
                },
                {
                    "cache|c=",
                    "Transcript cache prefix",
                    v => _transcriptCachePrefix = v
                },
                {
                    "gene|g=",
                    "SpliceAi gene data",
                    v => _geneInfoFile = v
                },
                {
                    "in|i=",
                    "input VCF file path",
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
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_transcriptCachePrefix, "transcript cache file", "--cache")
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(_transcriptCachePrefix), "transcript cache prefix", "--cache")
                .HasRequiredParameter(_inputFile, "SpliceAI VCF file", "--in")
                .CheckInputFilenameExists(_inputFile, "SpliceAI VCF file", "--in")
                .HasRequiredParameter(_geneInfoFile, "SpliceAi gene data", "--gene")
                .CheckInputFilenameExists(_geneInfoFile, "SpliceAi gene data", "--gene")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database containing 1000 Genomes allele frequencies", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            TranscriptCacheData transcriptData;
            using (var transcriptCacheReader = new TranscriptCacheReader(FileUtilities.GetReadStream(CacheConstants.TranscriptPath(_transcriptCachePrefix))))
            {
                transcriptData = transcriptCacheReader.Read(referenceProvider.RefIndexToChromosome);
            }

            var spliceIntervals    = SpliceUtilities.GetSpliceIntervals(referenceProvider, transcriptData);
            var nirEnstToGeneSymbols  = SpliceUtilities.GetEnstToGeneSymbols(referenceProvider, transcriptData);

            Dictionary<string, string> spliceAiEnstToGeneSymbols;
            using (var reader = new StreamReader(GZipUtilities.GetAppropriateReadStream(_geneInfoFile)))
            {
                spliceAiEnstToGeneSymbols = SpliceUtilities.GetSpliceAiGeneSymbols(reader);
            }

            var spliceAiToNirvanaGeneSymbols =
                SpliceUtilities.GetSymbolMapping(spliceAiEnstToGeneSymbols, nirEnstToGeneSymbols);

            Console.WriteLine($"Mapped {spliceAiToNirvanaGeneSymbols.Count} spliceAI gene symbols to Nirvana gene symbols (out of {spliceAiEnstToGeneSymbols.Count})");

            var version        = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            string outFileName = $"{version.Name}_{version.Version}";

            using (var spliceAiParser = new SpliceAiParser(
                GZipUtilities.GetAppropriateReadStream(_inputFile), 
                referenceProvider, spliceIntervals, spliceAiToNirvanaGeneSymbols))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            {
                var nsaWriter = new NsaWriter(new ExtendedBinaryWriter(nsaStream), new ExtendedBinaryWriter(indexStream),  version, referenceProvider, SaCommon.SpliceAiTag, true, true, SaCommon.SchemaVersion,false);
                nsaWriter.Write(spliceAiParser.GetItems());
            }

            Console.WriteLine($"Total number of entries from Splice AI: {SpliceAiParser.Count}");
            return ExitCodes.Success;
        }

        
    }
}