using System;
using System.Collections.Generic;
using System.Text;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using Compression.FileHandling;
using ErrorHandling;
using Jasix;
using Jasix.DataStructures;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.IO.VcfWriter;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;

namespace Nirvana
{
    public sealed class Nirvana
    {
        private static readonly string AnnotatorVersionTag      = "Illumina Annotation Engine " + CommandLineUtilities.Version;
        private readonly PerformanceMetrics _performanceMetrics = PerformanceMetrics.Instance;
        private readonly VcfConversion _conversion              = new VcfConversion();

        private ExitCodes ProgramExecution()
        {
            var sequenceProvider             = ProviderUtilities.GetSequenceProvider(ConfigurationSettings.RefSequencePath);
            var transcriptAnnotationProvider = ProviderUtilities.GetTranscriptAnnotationProvider(ConfigurationSettings.InputCachePrefix, sequenceProvider);
            var saProvider                   = ProviderUtilities.GetSaProvider(ConfigurationSettings.SupplementaryAnnotationDirectories);
            var conservationProvider         = ProviderUtilities.GetConservationProvider(ConfigurationSettings.SupplementaryAnnotationDirectories);
            var refMinorProvider             = ProviderUtilities.GetRefMinorProvider(ConfigurationSettings.SupplementaryAnnotationDirectories);
            var geneAnnotationProvider      = ProviderUtilities.GetGeneAnnotationProviders(ConfigurationSettings.SupplementaryAnnotationDirectories);
            var annotator                    = ProviderUtilities.GetAnnotator(transcriptAnnotationProvider, sequenceProvider, saProvider, conservationProvider, geneAnnotationProvider);

            var dataSourceVersions = new List<IDataSourceVersion>();
            dataSourceVersions.AddRange(transcriptAnnotationProvider.DataSourceVersions);
            if (saProvider != null) dataSourceVersions.AddRange(saProvider.DataSourceVersions);
            if (geneAnnotationProvider != null )
            {
                dataSourceVersions.AddRange(geneAnnotationProvider.DataSourceVersions);
            }

            if (conservationProvider != null) dataSourceVersions.AddRange(conservationProvider.DataSourceVersions);

            var vepDataVersion = CacheConstants.VepVersion + "." + CacheConstants.DataVersion + "." + SaDataBaseCommon.DataVersion;
            var jasixFileName  = ConfigurationSettings.OutputFileName + ".json.gz" + JasixCommons.FileExt;

            using (var outputWriter      = ReadWriteUtilities.GetOutputWriter(ConfigurationSettings.OutputFileName))
            using (var vcfReader         = ReadWriteUtilities.GetVcfReader(ConfigurationSettings.VcfPath, sequenceProvider.GetChromosomeDictionary(), refMinorProvider, ConfigurationSettings.ReportAllSvOverlappingTranscripts))
            using (var jsonWriter        = new JsonWriter(outputWriter, AnnotatorVersionTag, Date.CurrentTimeStamp, vepDataVersion, dataSourceVersions, sequenceProvider.GenomeAssembly.ToString(), vcfReader.GetSampleNames()))
            using (var vcfWriter         = ConfigurationSettings.Vcf ? new LiteVcfWriter(ReadWriteUtilities.GetVcfOutputWriter(ConfigurationSettings.OutputFileName), vcfReader.GetHeaderLines(), AnnotatorVersionTag, vepDataVersion, dataSourceVersions) : null)
            using (var gvcfWriter        = ConfigurationSettings.Gvcf ? new LiteVcfWriter(ReadWriteUtilities.GetGvcfOutputWriter(ConfigurationSettings.OutputFileName), vcfReader.GetHeaderLines(), AnnotatorVersionTag, vepDataVersion, dataSourceVersions) : null)
            using (var jasixIndexCreator = new OnTheFlyIndexCreator(FileUtilities.GetCreateStream(jasixFileName)))
            {
                var bgzipTextWriter = outputWriter as BgzipTextWriter;

                try
                {
                    jasixIndexCreator.SetHeader(jsonWriter.Header);

                    if (vcfReader.IsRcrsMitochondrion && annotator.GenomeAssembly == GenomeAssembly.GRCh37
                        || annotator.GenomeAssembly == GenomeAssembly.GRCh38
                        || ConfigurationSettings.ForceMitochondrialAnnotation)
                        annotator.EnableMitochondrialAnnotation();

                    int previousChromIndex = -1;
                    IPosition position;
                    var sortedVcfChecker = new SortedVcfChecker();


                    while ((position = vcfReader.GetNextPosition()) != null)
                    {
                        sortedVcfChecker.CheckVcfOrder(position.Chromosome.UcscName);
                        previousChromIndex = UpdatePerformanceMetrics(previousChromIndex, position.Chromosome);

                        var annotatedPosition = annotator.Annotate(position);

                        var jsonOutput = annotatedPosition.GetJsonString();
                        if (jsonOutput != null)
                        {
                            if (bgzipTextWriter != null)
                                jasixIndexCreator.Add(annotatedPosition.Position, bgzipTextWriter.Position);
                        }
                        jsonWriter.WriteJsonEntry(jsonOutput);

                        if (annotatedPosition.AnnotatedVariants?.Length > 0) vcfWriter?.Write(_conversion.Convert(annotatedPosition));
                        if (annotatedPosition.AnnotatedVariants?.Length > 0)
                        {
                            gvcfWriter?.Write(_conversion.Convert(annotatedPosition));
                        }
                        else
                        {
                            gvcfWriter?.Write(string.Join("\t", position.VcfFields));
                        }

                        _performanceMetrics.Increment();
                    }

                    if (previousChromIndex != -1) _performanceMetrics.StopReference();

                    WriteGeneAnnotations(annotator.GetAnnotatedGenes(), jsonWriter);
                }
                catch (Exception e)
                {
                    e.Data[ExitCodeUtilities.VcfLine] = vcfReader.VcfLine;
                    throw;
                }
            }

            return ExitCodes.Success;
        }

        private static void WriteGeneAnnotations(IList<IAnnotatedGene> annotatedGenes, JsonWriter writer)
        {
            if (annotatedGenes.Count == 0) return;
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);
            jsonObject.AddObjectValues("genes", annotatedGenes, true);
            writer.WriteAnnotatedGenes(sb.ToString());
        }

        private int UpdatePerformanceMetrics(int previousChromIndex, IChromosome chromosome)
        {
            if (chromosome.Index != previousChromIndex)
            {
                if (previousChromIndex != -1) _performanceMetrics.StopReference();
                _performanceMetrics.StartReference(chromosome.UcscName);

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
                    v => ConfigurationSettings.InputCachePrefix = v
                },
                {
                    "in|i=",
                    "input VCF {path}",
                    v => ConfigurationSettings.VcfPath = v
                },
                {
                    "loftee",
                    "enables loftee",
                    v => ConfigurationSettings.EnableLoftee = v != null
                },
                {
                    "gvcf",
                    "enables genome vcf output",
                    v => ConfigurationSettings.Gvcf = v != null
                },
                {
                    "vcf",
                    "enables vcf output",
                    v => ConfigurationSettings.Vcf = v != null
                },
                {
                    "out|o=",
                    "output {file path}",
                    v => ConfigurationSettings.OutputFileName = v
                },
                {
                    "ref|r=",
                    "input compressed reference sequence {path}",
                    v => ConfigurationSettings.RefSequencePath = v
                },
                {
                    "sd=",
                    "input supplementary annotation {directory}",
                    v => ConfigurationSettings.SupplementaryAnnotationDirectories.Add(v)
                },
                {
                    "force-mt",
                    "forces to annotate mitochondrial variants",
                    v => ConfigurationSettings.ForceMitochondrialAnnotation = v != null
                },
                {
                    "verbose-transcripts",
                    "reports all overlapping transcripts for structural variants",
                    v => ConfigurationSettings.ReportAllSvOverlappingTranscripts = v != null
                }
            };

            var exitCode = new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in", true, "-")
                .CheckInputFilenameExists(ConfigurationSettings.RefSequencePath, "reference sequence", "--ref")
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(ConfigurationSettings.InputCachePrefix), "transcript cache", "--cache")
                .CheckInputFilenameExists(CacheConstants.SiftPath(ConfigurationSettings.InputCachePrefix), "SIFT cache", "--cache")
                .CheckInputFilenameExists(CacheConstants.PolyPhenPath(ConfigurationSettings.InputCachePrefix), "PolyPhen cache", "--cache")
                .CheckEachDirectoryContainsFiles(ConfigurationSettings.SupplementaryAnnotationDirectories, "supplementary annotation", "--sd", "*.nsa")
                .HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out")
                .Enable(ConfigurationSettings.OutputFileName == "-", () =>
                {
                    ConfigurationSettings.Vcf = false;
                    ConfigurationSettings.Gvcf = false;
                })
                .DisableOutput(ConfigurationSettings.OutputFileName == "-")
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Annotates a set of variants", "-i <vcf path> -c <cache prefix> --sd <sa dir> -r <ref path> -o <base output filename>")
                .ShowErrors()
                .Execute(nirvana.ProgramExecution);

            return (int)exitCode;
        }
    }
}