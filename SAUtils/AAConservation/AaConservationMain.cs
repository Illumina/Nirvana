using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Caches;
using VariantAnnotation.ProteinConservation;
using VariantAnnotation.Providers;

namespace SAUtils.AAConservation
{
    public static class AaConservationMain
    {
        private static string _scoresFile;
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
                    "scr|s=",
                    "input file path with conservation scores",
                    v => _scoresFile = v
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
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(_transcriptCachePrefix), "transcript cache prefix", "--cache")
                .CheckInputFilenameExists(_scoresFile, "input file path with conservation scores", "--src")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a supplementary database containing 1000 Genomes allele frequencies", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            using var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            TranscriptCacheData transcriptData = AaConservationUtilities.GetTranscriptData(referenceProvider.RefIndexToChromosome, _transcriptCachePrefix);// we will use the transcript data to validate the protein sequence
            
            var    version     = DataSourceVersionReader.GetSourceVersion(_scoresFile + ".version");
            string outFileName = $"{version.Name}_{version.Version}";

            //read multi-alignments
            using (var stream = GZipUtilities.GetAppropriateReadStream(_scoresFile))
            using(var parser = new ProteinConservationParser(stream))
            using(var outStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName+ProteinConservationCommon.FileSuffix)))
            using(var writer = new ProteinConservationWriter(outStream, transcriptData, version))    
            {
                writer.Write(parser.GetItems());
            }

            return ExitCodes.Success;
        }

    }
}