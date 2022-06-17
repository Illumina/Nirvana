using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using SAUtils.InputFileParsers;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.Dann
{
    public static class Create
    {
        private static string _inputFile;
        private static string _compressedReference;
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
                    "in|i=",
                    "input DANN file path",
                    v => _inputFile = v
                },
                {
                    "out|o=",
                    "output directory",
                    v => _outputDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
                .CheckInputFilenameExists(_inputFile,           "input DANN file Path",                    "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Create a supplementary database from DANN input file ", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var nucleotides = new[] {"A", "C", "G", "T"};

            var dannParserSettings = new ParserSettings(
                new ColumnIndex(0, 2, 3, 4, 5, null),
                nucleotides,
                GenericScoreParser.MaxRepresentativeScores
            );

            var dannWriterSettings = new WriterSettings(
                1_000_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder(SaCommon.DannTag + SaCommon.Score, null),
                new SaItemValidator(true, false)
            );

            DataSourceVersion version     = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            var               outFileName = $"{version.Name}_{version.Version}";
            using (var referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference)))
            using (var streamReader = GZipUtilities.GetAppropriateStreamReader(_inputFile))
            using (var dannParser = new GenericScoreParser(dannParserSettings, streamReader, referenceProvider.RefNameToChromosome))
            using (var saStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GsaFileSuffix)))
            using (var indexStream =
                   FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GsaFileSuffix + SaCommon.IndexSuffix)))
            using (var saWriter = new ScoreFileWriter(dannWriterSettings, saStream, indexStream, version, referenceProvider,
                       SaCommon.SchemaVersion, skipIncorrectRefEntries: true, leaveOpen: false))
            {
                saWriter.Write(dannParser.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}