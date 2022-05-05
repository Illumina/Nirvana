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
using OptimizedCore;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Providers;
using Vcf;

namespace Nirvana
{
    public static class Nirvana
    {
        private static          string       _inputCachePrefix;
        private static readonly List<string> SupplementaryAnnotationDirectories = new List<string>();
        private static          string       _vcfPath;
        private static          string       _refSequencePath;
        private static          string       _outputFileName;
        private static          string       _customStrTsv;
        private static          string       _customInfoKeysString;
        private static          string       _customSampleInfoKeysString;
        
        private static          bool         _forceMitochondrialAnnotation;
        private static          bool         _disableRecomposition;
        private static          bool         _useLegacyVids;
        private static          bool         _enableDq;
        

        private static ExitCodes ProgramExecution()
        {
            var    annotationResources = GetAnnotationResources();
            string jasixFileName       = _outputFileName == "-" ? null : _outputFileName + ".json.gz" + JasixCommons.FileExt;
            
            var    customInfoKeys = string.IsNullOrEmpty(_customInfoKeysString) ?
                null: 
                new HashSet<string>(_customInfoKeysString.OptimizedSplit(','));

            var customSampleInfoKeys = string.IsNullOrEmpty(_customSampleInfoKeysString) ?
                null: 
                new HashSet<string>(_customSampleInfoKeysString.OptimizedSplit(','));

            using (var inputVcfStream        = _vcfPath        == "-"  ? Console.OpenStandardInput() : GZipUtilities.GetAppropriateReadStream(_vcfPath))
            using (var outputJsonStream      = _outputFileName == "-"  ? Console.OpenStandardOutput() : new BlockGZipStream(FileUtilities.GetCreateStream(_outputFileName + ".json.gz"), CompressionMode.Compress))
            using (var outputJsonIndexStream = jasixFileName   == null ? null : FileUtilities.GetCreateStream(jasixFileName))
                return StreamAnnotation.Annotate(null, inputVcfStream, outputJsonStream, outputJsonIndexStream, annotationResources, 
                    new NullVcfFilter(), false, _enableDq, customInfoKeys, customSampleInfoKeys).exitCode;
        }

        private static AnnotationResources GetAnnotationResources()
        {
            if (_outputFileName == "-") Logger.Silence();
            var metrics = new PerformanceMetrics();
            
            var annotationResources = new AnnotationResources(_refSequencePath, _inputCachePrefix, 
                SupplementaryAnnotationDirectories, null, _customStrTsv,
                _disableRecomposition, _forceMitochondrialAnnotation, _useLegacyVids, metrics);
            
            if (SupplementaryAnnotationDirectories.Count == 0) return annotationResources;

            using (var preloadVcfStream = GZipUtilities.GetAppropriateStream(PersistentStreamUtils.GetReadStream(_vcfPath)))
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
                },
                {
                    "legacy-vids",
                    "enables support for legacy VIDs",
                    v => _useLegacyVids = v != null
                },
                {
                    "enable-dq",
                    "report DQ from VCF samples field",
                    v => _enableDq = v != null
                },
                {
                    "str=",
                    "user provided STR annotation TSV file",
                    v => _customStrTsv = v
                },
                {
                    "vcf-info=",
                    "additional vcf info field keys (comma separated) desired in the output",
                    v => _customInfoKeysString = v
                },
                {
                    "vcf-sample-info=",
                    "additional vcf format field keys (comma separated) desired in the output",
                    v => _customSampleInfoKeysString = v
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
                .CheckInputFilenameExists(_customStrTsv, "custom STR annotation TSV", "--str", false)
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