using System;
using System.Collections.Generic;
using System.IO.Compression;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using Compression.FileHandling;
using Compression.Utilities;
using ErrorHandling;
using IO;
using IO.StreamSource;
using Jasix.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.IO.VcfWriter;
using VariantAnnotation.Providers;
using Vcf;

namespace Nirvana
{
    public sealed class Nirvana
    {
        private static string _inputCachePrefix;
        private static readonly List<string> SupplementaryAnnotationDirectories = new List<string>();
        private static string _vcfPath;
        private static string _refSequencePath;
        private static string _outputFileName;
        private static string _pluginDirectory;

        private static bool _vcf;
        private static bool _gvcf;
        private static bool _forceMitochondrialAnnotation;
        private static bool _reportAllSvOverlappingTranscripts;
        private static bool _disableRecomposition;

        private readonly string _annotatorVersionTag = "Nirvana " + CommandLineUtilities.Version;
        private readonly VcfConversion _conversion   = new VcfConversion();

        private ExitCodes ProgramExecution()
        {

            var annotationResources = GetAnnotationResources();

            string jasixFileName = _outputFileName == "-" ? null : _outputFileName + ".json.gz" + JasixCommons.FileExt;
            using (var inputVcfStream = _vcfPath == "-" ? Console.OpenStandardInput() : GZipUtilities.GetAppropriateReadStream(new FileStreamSource(_vcfPath)))
            using (var outputJsonStream = _outputFileName == "-" ? Console.OpenStandardOutput() : new BlockGZipStream(FileUtilities.GetCreateStream(_outputFileName + ".json.gz"), CompressionMode.Compress))
            using (var outputJsonIndexStream = jasixFileName == null ? null : FileUtilities.GetCreateStream(jasixFileName))
            using (var outputVcfStream = !_vcf ? null : _outputFileName == "-" ? Console.OpenStandardOutput() : GZipUtilities.GetWriteStream(_outputFileName + ".vcf.gz"))
            using (var outputGvcfStream = !_gvcf ? null : _outputFileName == "-" ? Console.OpenStandardOutput() : GZipUtilities.GetWriteStream(_outputFileName + ".genome.vcf.gz"))
                return StreamAnnotation.Annotate(null, inputVcfStream, outputJsonStream, outputJsonIndexStream, outputVcfStream,
                    outputGvcfStream, annotationResources, new NullVcfFilter());
        }

        private AnnotationResources GetAnnotationResources()
        {
            var annotationResources = new AnnotationResources(_refSequencePath, _inputCachePrefix, SupplementaryAnnotationDirectories[0], _pluginDirectory, _vcf, _gvcf, _disableRecomposition, _reportAllSvOverlappingTranscripts, _forceMitochondrialAnnotation);
            using (var preloadVcfStream = new BlockGZipStream(FileUtilities.GetReadStream(_vcfPath), CompressionMode.Decompress))
            {
                annotationResources.GetVariantPositions(preloadVcfStream, null);
            }
            return annotationResources;
        }

        private void WriteOutput(IAnnotatedPosition annotatedPosition, IJsonWriter jsonWriter, LiteVcfWriter vcfWriter, LiteVcfWriter gvcfWriter, string jsonOutput)
        {
            jsonWriter.WriteJsonEntry(annotatedPosition.Position, jsonOutput);

            if (vcfWriter == null && gvcfWriter == null || annotatedPosition.Position.IsRecomposed) return;

            string vcfLine = _conversion.Convert(annotatedPosition);
            vcfWriter?.Write(vcfLine);
            gvcfWriter?.Write(vcfLine);
        }

        public static int Main(string[] args)
        {
            var nirvana = new Nirvana();
            var ops = new OptionSet
            {
                {
                    "cache|c=",
                    "input cache {prefix}",
                    v => _inputCachePrefix = v
                },
                {
                    "in|i=",
                    "input VCF {path}",
                    v => _vcfPath = v
                },
                {
                    "plugin|p=",
                    "plugin {directory}",
                    v => _pluginDirectory = v
                },
                {
                    "gvcf",
                    "enables genome vcf output",
                    v => _gvcf = v != null
                },
                {
                    "vcf",
                    "enables vcf output",
                    v => _vcf = v != null
                },
                {
                    "out|o=",
                    "output {file path}",
                    v => _outputFileName = v
                },
                {
                    "ref|r=",
                    "input compressed reference sequence {path}",
                    v => _refSequencePath = v
                },
                {
                    "sd=",
                    "input supplementary annotation {directory}",
                    v => SupplementaryAnnotationDirectories.Add(v)
                },
                {
                    "force-mt",
                    "forces to annotate mitochondrial variants",
                    v => _forceMitochondrialAnnotation = v != null
                },
                {
                    "verbose-transcripts",
                    "reports all overlapping transcripts for structural variants",
                    v => _reportAllSvOverlappingTranscripts = v != null
                },
                {
                    "disable-recomposition",
                    "don't recompose function relevant variants",
                    v => _disableRecomposition = v != null
                }
            };

            var exitCode = new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckInputFilenameExists(_vcfPath, "vcf", "--in", true, "-")
                .CheckInputFilenameExists(_refSequencePath, "reference sequence", "--ref")
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(_inputCachePrefix), "transcript cache", "--cache")
                .CheckInputFilenameExists(CacheConstants.SiftPath(_inputCachePrefix), "SIFT cache", "--cache")
                .CheckInputFilenameExists(CacheConstants.PolyPhenPath(_inputCachePrefix), "PolyPhen cache", "--cache")
                //.CheckEachDirectoryContainsFiles(SupplementaryAnnotationDirectories, "supplementary annotation", "--sd", "*.nsa")
                .HasRequiredParameter(_outputFileName, "output file stub", "--out")
                .Enable(_outputFileName == "-", () =>
                {
                    _vcf  = false;
                    _gvcf = false;
                })
                .DisableOutput(_outputFileName == "-")
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Annotates a set of variants", "-i <vcf path> -c <cache prefix> --sd <sa dir> -r <ref path> -o <base output filename>")
                .ShowErrors()
                .Execute(nirvana.ProgramExecution);

            return (int)exitCode;
        }
    }
}