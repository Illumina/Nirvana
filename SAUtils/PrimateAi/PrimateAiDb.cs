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

namespace SAUtils.PrimateAi
{
    public static class PrimateAiDb
    {
        private static string _inputFile;
        private static string _compressedReference;
        private static string _outputDirectory;
        private static string _transcriptCachePrefix;

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
                    "in|i=",
                    "input VCF file path",
                    v => _inputFile = v
                },
                {
                    "cache|c=",
                    "Transcript cache prefix",
                    v => _transcriptCachePrefix = v
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
                .HasRequiredParameter(_inputFile, "PrimateAI VCF file", "--in")
                .HasRequiredParameter(_transcriptCachePrefix, "transcript cache file", "--cache")
                .CheckInputFilenameExists(_inputFile, "PrimateAI VCF file", "--in")
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
            var version = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            string outFileName = $"{version.Name}_{version.Version}";

            TranscriptCacheData transcriptData;
            using (var transcriptCacheReader = new TranscriptCacheReader(FileUtilities.GetReadStream(CacheConstants.TranscriptPath(_transcriptCachePrefix))))
            {
                transcriptData = transcriptCacheReader.Read(referenceProvider.RefIndexToChromosome);
            }

            var (entrezToHgnc, ensemblToHgnc) = PrimateAiUtilities.GetIdToSymbols(transcriptData);

            using (var primateAiParser = new PrimateAiParser(GZipUtilities.GetAppropriateReadStream(_inputFile),referenceProvider, entrezToHgnc, ensemblToHgnc))
            using (var nsaStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix)))
            using (var indexStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.SaFileSuffix + SaCommon.IndexSufix)))
            using (var nsaWriter = new NsaWriter(nsaStream, indexStream, version, referenceProvider, SaCommon.PrimateAiTag, true, true, SaCommon.SchemaVersion, false))
            {
                nsaWriter.Write(primateAiParser.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}