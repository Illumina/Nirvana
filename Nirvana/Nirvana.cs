using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using ErrorHandling;
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
using VariantAnnotation.Utilities;

namespace Nirvana
{
    public sealed class Nirvana
    {
        private readonly string _nirvanaVersion = "Illumina Annotation Engine " + CommandLineUtilities.Version;
        private readonly PerformanceMetrics _performanceMetrics = PerformanceMetrics.Instance;
        private VcfConversion _conversion = new VcfConversion();

        private ExitCodes ProgramExecution()
        {
            var sequenceProvider = ProviderUtilities.GetSequenceProvider(ConfigurationSettings.RefSequencePath);
            var transcriptAnnotationProvider =
                ProviderUtilities.GetTranscriptAnnotationProvider(ConfigurationSettings.InputCachePrefix, sequenceProvider);
            var saProvider = ProviderUtilities.GetSaProvider(ConfigurationSettings.SupplementaryAnnotationDirectories);
            var conservationProvider =
                ProviderUtilities.GetConservationProvider(ConfigurationSettings.SupplementaryAnnotationDirectories);

            var refMinorProvider =
                ProviderUtilities.GetRefMinorProvider(ConfigurationSettings.SupplementaryAnnotationDirectories);


            var geneAnnotationProviders = ProviderUtilities.GetGeneAnnotationProviders(ConfigurationSettings.SupplementaryAnnotationDirectories);
            var annotator = ProviderUtilities.GetAnnotator(transcriptAnnotationProvider, sequenceProvider, saProvider,
                conservationProvider, geneAnnotationProviders);

            var dataSourceVesions = new List<IDataSourceVersion>();
            dataSourceVesions.AddRange(transcriptAnnotationProvider.DataSourceVersions);
            if(saProvider!=null)dataSourceVesions.AddRange(saProvider.DataSourceVersions);
            if (geneAnnotationProviders != null && geneAnnotationProviders.Length > 0)
            {
                dataSourceVesions.AddRange(geneAnnotationProviders.SelectMany(x=>x.DataSourceVersions));
            }


            if(conservationProvider != null) dataSourceVesions.AddRange(conservationProvider.DataSourceVersions);

            var vepDataVersion = CacheConstants.VepVersion + "." + CacheConstants.DataVersion + "." + SupplementaryAnnotationCommon.DataVersion;

            using (var vcfReader  = ReadWriteUtilities.GetVcfReader(ConfigurationSettings.VcfPath, sequenceProvider.GetChromosomeDictionary(), refMinorProvider,ConfigurationSettings.ReportAllSvOverlappingTranscripts))
            using (var jsonWriter = new JsonWriter(ReadWriteUtilities.GetOutputWriter(ConfigurationSettings.OutputFileName), _nirvanaVersion, Date.CurrentTimeStamp, vepDataVersion, dataSourceVesions, sequenceProvider.GenomeAssembly.ToString(), vcfReader.GetSampleNames()))
           using(var vcfWriter = ConfigurationSettings.Vcf ? new LiteVcfWriter(ReadWriteUtilities.GetVcfOutputWriter(ConfigurationSettings.OutputFileName), vcfReader.GetHeaderLines(), _nirvanaVersion, vepDataVersion, dataSourceVesions) : null)
            using (var gvcfWriter = ConfigurationSettings.Gvcf ? new LiteVcfWriter(ReadWriteUtilities.GetGvcfOutputWriter(ConfigurationSettings.OutputFileName), vcfReader.GetHeaderLines(), _nirvanaVersion, vepDataVersion, dataSourceVesions) : null)
            {
                try
                {
                    //WriteHeader(vcfWriter, vcfReader.GetHeaderLines());
                    //WriteHeader(gvcfWriter, vcfReader.GetHeaderLines());

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

                        jsonWriter.WriteJsonEntry(annotatedPosition.GetJsonString());

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

                    _performanceMetrics.StopReference();

                    //add gene annotation
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
            if(annotatedGenes.Count==0) return;
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);
            jsonObject.AddObjectValues("genes",annotatedGenes,true);
            writer.WriteAnnotatedGenes(sb.ToString());
        }
        private void WriteHeader(StreamWriter vcfWriter, IEnumerable<string> headerLines)
        {
            if (vcfWriter == null) return;
            foreach (var headerLine in headerLines) vcfWriter.WriteLine(headerLine);
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
                .CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in",true, "-")
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