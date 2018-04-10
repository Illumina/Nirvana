using System;
using System.Collections.Generic;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using CommonUtilities;
using Compression.FileHandling;
using ErrorHandling;
using Jasix;
using Jasix.DataStructures;
using Phantom.Workers;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.IO.VcfWriter;
using VariantAnnotation.Logger;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;
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
            var sequenceProvider             = ProviderUtilities.GetSequenceProvider(_refSequencePath);
            var transcriptAnnotationProvider = ProviderUtilities.GetTranscriptAnnotationProvider(_inputCachePrefix, sequenceProvider);
            var saProvider                   = ProviderUtilities.GetSaProvider(SupplementaryAnnotationDirectories);
            var conservationProvider         = ProviderUtilities.GetConservationProvider(SupplementaryAnnotationDirectories);
            var refMinorProvider             = ProviderUtilities.GetRefMinorProvider(SupplementaryAnnotationDirectories);
            var geneAnnotationProvider       = ProviderUtilities.GetGeneAnnotationProvider(SupplementaryAnnotationDirectories);
            var plugins                      = PluginUtilities.LoadPlugins(_pluginDirectory);
            var annotator                    = ProviderUtilities.GetAnnotator(transcriptAnnotationProvider, sequenceProvider, saProvider, conservationProvider, geneAnnotationProvider, plugins);
            var recomposer = _disableRecomposition ? new NullRecomposer() : Recomposer.Create(sequenceProvider, _inputCachePrefix);
            var logger  = _outputFileName == "-" ? (ILogger) new NullLogger() : new ConsoleLogger();
            var metrics = new PerformanceMetrics(logger);

            var dataSourceVersions = GetDataSourceVersions(plugins, transcriptAnnotationProvider, saProvider,
                geneAnnotationProvider, conservationProvider);
 
            var vepDataVersion = transcriptAnnotationProvider.VepVersion + "." + CacheConstants.DataVersion + "." + SaDataBaseCommon.DataVersion;
            var jasixFileName  = _outputFileName + ".json.gz" + JasixCommons.FileExt;

            using (var outputWriter      = ReadWriteUtilities.GetOutputWriter(_outputFileName))
            using (var vcfReader         = ReadWriteUtilities.GetVcfReader(_vcfPath, sequenceProvider.RefNameToChromosome, refMinorProvider, _reportAllSvOverlappingTranscripts, recomposer))
            using (var jsonWriter        = new JsonWriter(outputWriter, _annotatorVersionTag, Date.CurrentTimeStamp, vepDataVersion, dataSourceVersions, sequenceProvider.GenomeAssembly.ToString(), vcfReader.GetSampleNames()))
            using (var vcfWriter         = _vcf ? new LiteVcfWriter(ReadWriteUtilities.GetVcfOutputWriter(_outputFileName), vcfReader.GetHeaderLines(), _annotatorVersionTag, vepDataVersion, dataSourceVersions) : null)
            using (var gvcfWriter        = _gvcf ? new LiteVcfWriter(ReadWriteUtilities.GetGvcfOutputWriter(_outputFileName), vcfReader.GetHeaderLines(), _annotatorVersionTag, vepDataVersion, dataSourceVersions) : null)
            using (var jasixIndexCreator = new OnTheFlyIndexCreator(FileUtilities.GetCreateStream(jasixFileName)))
            {
                if (!(outputWriter is BgzipTextWriter bgzipTextWriter)) throw new NullReferenceException("Unable to create the bgzip text writer.");

                try
                {
                    jasixIndexCreator.SetHeader(jsonWriter.Header);

                    if (vcfReader.IsRcrsMitochondrion && annotator.GenomeAssembly == GenomeAssembly.GRCh37
                        || annotator.GenomeAssembly == GenomeAssembly.GRCh38
                        || _forceMitochondrialAnnotation)
                        annotator.EnableMitochondrialAnnotation();

                    int previousChromIndex = -1;
                    IPosition position;
                    var sortedVcfChecker = new SortedVcfChecker();

                    while ((position = vcfReader.GetNextPosition()) != null)
                    {
                        sortedVcfChecker.CheckVcfOrder(position.Chromosome.UcscName);
                        previousChromIndex = UpdatePerformanceMetrics(previousChromIndex, position.Chromosome, metrics);

                        var annotatedPosition = annotator.Annotate(position);

                        string json = annotatedPosition.GetJsonString();

                        if (json != null) WriteOutput(annotatedPosition, bgzipTextWriter.Position, jasixIndexCreator, jsonWriter, vcfWriter, gvcfWriter, json);
                        else gvcfWriter?.Write(string.Join("\t", position.VcfFields));

                        metrics.Increment();
                    }

                    WriteGeneAnnotations(annotator.GetAnnotatedGenes(), jsonWriter);
                }
                catch (Exception e)
                {
                    e.Data[ExitCodeUtilities.VcfLine] = vcfReader.VcfLine;
                    throw;
                }
            }

            metrics.ShowAnnotationTime();

            return ExitCodes.Success;
        }

        private void WriteOutput(IAnnotatedPosition annotatedPosition, long textWriterPosition,
            OnTheFlyIndexCreator jasixIndexCreator, IJsonWriter jsonWriter, LiteVcfWriter vcfWriter,
            LiteVcfWriter gvcfWriter, string jsonOutput)
        {
            jasixIndexCreator.Add(annotatedPosition.Position, textWriterPosition);
            jsonWriter.WriteJsonEntry(jsonOutput);

            if (vcfWriter == null && gvcfWriter == null || annotatedPosition.Position.IsRecomposed) return;

            string vcfLine = _conversion.Convert(annotatedPosition);
            vcfWriter?.Write(vcfLine);
            gvcfWriter?.Write(vcfLine);
        }

        private static List<IDataSourceVersion> GetDataSourceVersions(IEnumerable<IPlugin> plugins,
            params IProvider[] providers)
        {
            var dataSourceVersions = new List<IDataSourceVersion>();
            if (plugins != null) foreach (var provider in plugins) if (provider.DataSourceVersions != null) dataSourceVersions.AddRange(provider.DataSourceVersions);
            foreach (var provider in providers) if (provider != null) dataSourceVersions.AddRange(provider.DataSourceVersions);
            return dataSourceVersions;
        }

        private static void WriteGeneAnnotations(ICollection<IAnnotatedGene> annotatedGenes, JsonWriter writer)
        {
            if (annotatedGenes.Count == 0) return;
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);
            jsonObject.AddObjectValues("genes", annotatedGenes, true);
            writer.WriteAnnotatedGenes(StringBuilderCache.GetStringAndRelease(sb));
        }

        private static int UpdatePerformanceMetrics(int previousChromIndex, IChromosome chromosome, PerformanceMetrics metrics)
        {
            // ReSharper disable once InvertIf
            if (chromosome.Index != previousChromIndex)
            {
                metrics.StartAnnotatingReference(chromosome);
                previousChromIndex = chromosome.Index;
            }

            return previousChromIndex;
        }

        private static int Main(string[] args)
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
                .CheckEachDirectoryContainsFiles(SupplementaryAnnotationDirectories, "supplementary annotation", "--sd", "*.nsa")
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