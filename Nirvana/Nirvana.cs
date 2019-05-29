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
using VariantAnnotation.Interface;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using Vcf;

namespace Nirvana
{
    public static class Nirvana
    {
        private static string _inputCachePrefix;
        private static readonly List<string> SupplementaryAnnotationDirectories = new List<string>();
        private static string _vcfPath;
        private static string _refSequencePath;
        private static string _outputFileName;
        private static string _pluginDirectory;

        private static bool _forceMitochondrialAnnotation;
        private static bool _disableRecomposition;

        private static ExitCodes ProgramExecution()
        {
            var annotationResources = GetAnnotationResources();

            string jasixFileName = _outputFileName == "-" ? null : _outputFileName + ".json.gz" + JasixCommons.FileExt;
            using (var inputVcfStream = _vcfPath == "-" ? Console.OpenStandardInput() : GZipUtilities.GetAppropriateReadStream(_vcfPath))
            using (var outputJsonStream = _outputFileName == "-" ? Console.OpenStandardOutput() : new BlockGZipStream(FileUtilities.GetCreateStream(_outputFileName + ".json.gz"), CompressionMode.Compress))
            using (var outputJsonIndexStream = jasixFileName == null ? null : FileUtilities.GetCreateStream(jasixFileName))
                return StreamAnnotation.Annotate(null, inputVcfStream, outputJsonStream, outputJsonIndexStream, annotationResources, new NullVcfFilter());
        }

        private static AnnotationResources GetAnnotationResources()
        {            
            var annotationResources = new AnnotationResources(_refSequencePath, _inputCachePrefix, SupplementaryAnnotationDirectories, null, _pluginDirectory, _disableRecomposition, _forceMitochondrialAnnotation);
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
                    "disable-recomposition",
                    "don't recompose function relevant variants",
                    v => _disableRecomposition = v != null
                }
            };

            var exitCode = new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckInputFilenameExists(_vcfPath, "vcf", "--in", true, "-")
                //.CheckInputFilenameExists(_vcfPath + ".tbi", "tabix index file", "--in")
                .CheckInputFilenameExists(_refSequencePath, "reference sequence", "--ref")
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(_inputCachePrefix), "transcript cache", "--cache")
                .CheckInputFilenameExists(CacheConstants.SiftPath(_inputCachePrefix), "SIFT cache", "--cache")
                .CheckInputFilenameExists(CacheConstants.PolyPhenPath(_inputCachePrefix), "PolyPhen cache", "--cache")
                .HasRequiredParameter(_outputFileName, "output file stub", "--out")
                .DisableOutput(_outputFileName == "-")
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Annotates a set of variants", "-i <vcf path> -c <cache prefix> --sd <sa dir> -r <ref path> -o <base output filename>")
                .ShowErrors()
                .Execute(ProgramExecution);

            return (int)exitCode;
        }
    }
}