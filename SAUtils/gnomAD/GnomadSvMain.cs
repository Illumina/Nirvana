using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.gnomAD;

public static class GnomadSvMain
{
    private static string _inputFileName;
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
                "gnomADV2 BED or TSV file",
                v => _inputFileName = v
            },
            {
                "out|o=",
                "output directory",
                v => _outputDirectory = v
            }
        };

        var commandLineExample = $"{command} [options]";

        ExitCodes exitCode = new ConsoleAppBuilder(commandArgs, ops)
            .Parse()
            .CheckInputFilenameExists(_compressedReference, "compressed reference sequence file name", "--ref")
            .CheckInputFilenameExists(_inputFileName,       "gnomADV2 BED or TSV file",                "--in")
            .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
            .SkipBanner()
            .ShowHelpMenu("Creates a supplementary database from gnomAD v2 structural variant annotations", commandLineExample)
            .ShowErrors()
            .Execute(ProgramExecution);

        return exitCode;
    }

    private static ExitCodes ProgramExecution()
    {
        var               referenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReference));
        DataSourceVersion version           = DataSourceVersionReader.GetSourceVersion(_inputFileName + ".version");

        string             outFileName = $"{version.Name}_{version.Version}".Replace(' ', '_');
        using StreamReader reader      = GZipUtilities.GetAppropriateStreamReader(_inputFileName);
        using GnomadSvParser gnomadSvParser = _inputFileName.Substring(_inputFileName.Length - 6) switch
        {
            "tsv.gz" => new GnomadSvTsvParser(reader, referenceProvider.RefNameToChromosome),
            "bed.gz" => new GnomadSvBedParser(reader, referenceProvider.RefNameToChromosome),
            _        => throw new InvalidFileFormatException("Input file should end in '.tsv.gz' or '.bed.gz'")
        };

        using FileStream nsiStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName + SaCommon.IntervalFileSuffix));
        using var nsiWriter = new NsiWriter(
            nsiStream,
            version,
            referenceProvider.Assembly,
            SaCommon.GnomadStructuralVariant,
            ReportFor.StructuralVariants,
            SaCommon.SchemaVersion
        );
        nsiWriter.Write(gnomadSvParser.GetItems());

        return ExitCodes.Success;
    }
}