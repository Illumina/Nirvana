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

namespace SAUtils.GERP
{
    public class GerpMain
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
                    "input file path",
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
                .HasRequiredParameter(_inputFile, "GERP wiggle or TSV file", "--in")
                .CheckInputFilenameExists(_inputFile, "GERP wiggle or TSV file", "--in")
                .HasRequiredParameter(_outputDirectory, "output directory", "--out")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("create Ancestral allele database from 1000Genomes data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            var               referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
            DataSourceVersion version           = DataSourceVersionReader.GetSourceVersion(_inputFile + ".version");
            var               outFileName       = $"{version.Name}_{version.Version}";

            var nucleotides = new[] {"N"};

            var  wigColumnIndex = new ColumnIndex(0, 2, null, null, 3, null);
            var  tsvColumnIndex = new ColumnIndex(0, 1, null, null, 2, null);
            bool isWig          = _inputFile.EndsWith("wig.gz");

            var parserSettings = new ParserSettings(
                isWig ? wigColumnIndex : tsvColumnIndex,
                nucleotides,
                GenericScoreParser.NonConflictingScore
            );

            var writerSettings = new WriterSettings(
                1_000_000,
                nucleotides,
                true,
                EncoderType.Generic,
                new GenericScoreEncoder(),
                new ScoreJsonEncoder(SaCommon.GerpTag + SaCommon.Score, null),
                new SaItemValidator(null, null)
            );


            using (var streamReader = new StreamReader(GZipUtilities.GetAppropriateReadStream(_inputFile)))
            using (var parser = new GenericScoreParser(parserSettings, streamReader, referenceProvider.RefNameToChromosome))
            using (var saStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GsaFileSuffix)))
            using (var indexStream =
                   FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.GsaFileSuffix + SaCommon.IndexSuffix)))
            using (var saWriter = new ScoreFileWriter(writerSettings, saStream, indexStream, version, referenceProvider,
                       SaCommon.SchemaVersion, skipIncorrectRefEntries: true, leaveOpen: false))
            {
                saWriter.Write(parser.GetItems());
            }

            return ExitCodes.Success;
        }
    }
}