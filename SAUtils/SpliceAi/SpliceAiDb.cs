using System;
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

namespace SAUtils.SpliceAi
{
    public static class SpliceAiDb
    {
        private static string _inputFile;
        private static string _compressedReference;
        private static string _transcriptCachePrefix;
        private static string _outputDirectory;
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
                .CheckInputFilenameExists(_inputFile, "dbSNP VCF file", "--in")
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
            var spliceIntervals   = SpliceUtilities.GetSpliceIntervals(referenceProvider, FileUtilities.GetReadStream(CacheConstants.TranscriptPath(_transcriptCachePrefix)));
            Console.WriteLine("Loaded transcripts and generated splice intervals.");
            var version           = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            
            string outFileName = $"{version.Name}_{version.Version}";
            using (var spliceAiParser = new SpliceAiParser(GZipUtilities.GetAppropriateReadStream(_inputFile), referenceProvider, spliceIntervals))
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