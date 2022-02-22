using System;
using System.Collections.Generic;
using System.IO.Compression;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.FileHandling;
using Compression.Utilities;
using ErrorHandling;
using IO;
using Jasix.DataStructures;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Providers;
using Vcf;

namespace Nirvana
{
    public static class Nirvana
    {
        private static          string       _inputCacheDir;
        private static readonly List<string> SupplementaryAnnotationDirectories = new();
        private static          string       _vcfPath;
        private static          string       _refSequencePath;
        private static          string       _outputFileName;

        private static bool _vcf;
        private static bool _gvcf;
        private static bool _forceMitochondrialAnnotation;

        private static ExitCodes ProgramExecution()
        {
            var annotationResources = GetAnnotationResources();

            string jasixFileName = _outputFileName == "-" ? null : _outputFileName + ".json.gz" + JasixCommons.FileExt;
            using (var inputVcfStream = _vcfPath == "-" ? Console.OpenStandardInput() : GZipUtilities.GetAppropriateReadStream(_vcfPath))
            using (var outputJsonStream = _outputFileName == "-" ? Console.OpenStandardOutput() : new BlockGZipStream(FileUtilities.GetCreateStream(_outputFileName + ".json.gz"), CompressionMode.Compress))
            using (var outputJsonIndexStream = jasixFileName == null ? null : FileUtilities.GetCreateStream(jasixFileName))
            using (var outputVcfStream = !_vcf ? null : _outputFileName == "-" ? Console.OpenStandardOutput() : GZipUtilities.GetWriteStream(_outputFileName + ".vcf.gz"))
            using (var outputGvcfStream = !_gvcf ? null : _outputFileName == "-" ? Console.OpenStandardOutput() : GZipUtilities.GetWriteStream(_outputFileName + ".genome.vcf.gz"))
                return StreamAnnotation.Annotate(null, inputVcfStream, outputJsonStream, outputJsonIndexStream, outputVcfStream,
                    outputGvcfStream, annotationResources, new NullVcfFilter());
        }

        private static AnnotationResources GetAnnotationResources()
        {            
            var annotationResources = new AnnotationResources(_refSequencePath, _inputCacheDir, SupplementaryAnnotationDirectories, null, null, _vcf, _gvcf, true, _forceMitochondrialAnnotation);
            if (SupplementaryAnnotationDirectories.Count == 0) return annotationResources;

            using (var preloadVcfStream = GZipUtilities.GetAppropriateStream(
                new PersistentStream(PersistentStreamUtils.GetReadStream(_vcfPath),
                    ConnectUtilities.GetFileConnectFunc(_vcfPath), 0)))
            {
                annotationResources.GetVariantPositions(preloadVcfStream, null);
            }
            return annotationResources;
        }

        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "cache|c=",
                    "input cache {dir}",
                    v => _inputCacheDir = v
                },
                {
                    "in|i=",
                    "input VCF {path}",
                    v => _vcfPath = v
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
                }
            };

            var exitCode = new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckInputFilenameExists(_vcfPath, "vcf", "--in", true, "-")
                .CheckInputFilenameExists(_refSequencePath, "reference sequence", "--ref")
                .CheckDirectoryExists(_inputCacheDir, "cache", "--cache")
                .HasRequiredParameter(_outputFileName, "output file stub", "--out")
                .Enable(_outputFileName == "-", () =>
                {
                    _vcf  = false;
                    _gvcf = false;
                })
                .DisableOutput(_outputFileName == "-")
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Annotates a set of variants", "-i <vcf path> -c <cache dir> --sd <sa dir> -r <ref path> -o <base output filename>")
                .ShowErrors()
                .Execute(ProgramExecution);

            return (int)exitCode;
        }
    }
}