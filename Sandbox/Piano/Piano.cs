
using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using VariantAnnotation;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;
using Vcf;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Providers;

namespace Piano
{
    sealed class Piano
    {
        #region members

        private const string OutHeader =
            "#Chrom\tPos\tRefAllele\tAltAllele\tGeneSymbol\tGeneId\tTranscriptID\tProteinID\tProteinPos\tUpstream\tAAchange\tDownstream\tConsequences";
        private readonly PerformanceMetrics _performanceMetrics = PerformanceMetrics.Instance;

        #endregion


        private  ExitCodes ProgramExecution()
        {
            var sequenceProvider = ProviderUtilities.GetSequenceProvider(ConfigurationSettings.RefSequencePath);
            var transcriptAnnotationProvider =
                ProviderUtilities.GetTranscriptAnnotationProvider(ConfigurationSettings.InputCachePrefix, sequenceProvider);

           


            var annotator = ProviderUtilities.GetAnnotator(transcriptAnnotationProvider, sequenceProvider);

            var dataSourceVesions = new List<IDataSourceVersion>();
            dataSourceVesions.AddRange(transcriptAnnotationProvider.DataSourceVersions);


            using (var outputWriter = new StreamWriter(ConfigurationSettings.OutputFileName))
            using (var vcfReader =new VcfReader(GZipUtilities.GetAppropriateReadStream(ConfigurationSettings.VcfPath), sequenceProvider.GetChromosomeDictionary(), null, false))
            {
                try
                {

                    if (vcfReader.IsRcrsMitochondrion && annotator.GenomeAssembly == GenomeAssembly.GRCh37
                        || annotator.GenomeAssembly == GenomeAssembly.GRCh38
                        || ConfigurationSettings.ForceMitochondrialAnnotation)
                        annotator.EnableMitochondrialAnnotation();

                    int previousChromIndex = -1;
                    IPosition position;
                   // var sortedVcfChecker = new SortedVcfChecker();
                   outputWriter.WriteLine(OutHeader);

                    while ((position = vcfReader.GetNextPosition()) != null)
                    {
                        // sortedVcfChecker.CheckVcfOrder(position.Chromosome.UcscName);
                         previousChromIndex = UpdatePerformanceMetrics(previousChromIndex, position.Chromosome);

                        var annotatedPosition = annotator.Annotate(position);
                        WriteAnnotatedPostion(annotatedPosition, outputWriter);

                    }
                }
                catch (Exception e)
                {
                    e.Data[ExitCodeUtilities.VcfLine] = vcfReader.VcfLine;
                    throw;
                }
            }

            return ExitCodes.Success;
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
        private static void WriteAnnotatedPostion(IAnnotatedPosition annotatedPosition, StreamWriter writer)
        {
            //"#Chrom\tPos\tRefAllele\tAltAllele\tGeneSymbol\tGeneId\tTranscriptID\tProteinID\tProteinPos\tUpstream\tAAchange\tDownstream\tConsequences";
            if (annotatedPosition.AnnotatedVariants == null || annotatedPosition.AnnotatedVariants.Length == 0) return;

            for (int i = 0; i < annotatedPosition.AnnotatedVariants.Length; i++)
            {

                var annotatedVariant = annotatedPosition.AnnotatedVariants[i];
                var chromosome = annotatedPosition.Position.VcfFields[VcfCommon.ChromIndex];
                var position = annotatedPosition.Position.Start;
                var refAllele = annotatedPosition.Position.RefAllele;
                var altAllele = annotatedPosition.Position.AltAlleles[i];

                foreach (var ensemblTranscript in annotatedVariant.EnsemblTranscripts)
                {
                    var transcript = ensemblTranscript;

                    if(transcript.ToString()==null) continue;

                    var line = chromosome + "\t" + position + "\t" + refAllele +
                               "\t" + altAllele + "\t" + transcript;
                    writer.WriteLine(line);
                }
                foreach (var refSeqTranscript in annotatedVariant.RefSeqTranscripts)
                {
                    var transcript = refSeqTranscript ;
                    if (transcript.ToString() == null) continue;

                    var line = chromosome + "\t" + position + "\t" + refAllele +
                               "\t" + altAllele + "\t" + transcript;
                    writer.WriteLine(line);
                }
            }
            
        }

        static int Main(string[] args)
        {
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
                    "out|o=",
                    "output {file path} ",
                    v => ConfigurationSettings.OutputFileName = v
                },
                {
                    "ref|r=",
                    "input compressed reference sequence {path}",
                    v => ConfigurationSettings.RefSequencePath = v
                },
                {
                    "force-mt",
                    "forces to annotate mitochondria variants",
                    v => ConfigurationSettings.ForceMitochondrialAnnotation = v != null
                }
            };

            var commandLineExample = "-i <vcf path> -d <cache dir> -r <ref path> -o <base output filename>";

            var piano = new Piano();
            var exitCode = new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in", true, "-")
                .CheckInputFilenameExists(ConfigurationSettings.RefSequencePath, "reference sequence", "--ref")
                .CheckInputFilenameExists(CacheConstants.TranscriptPath(ConfigurationSettings.InputCachePrefix), "transcript cache", "--cache")
                .HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out")
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("peptide annotation", commandLineExample)
                .ShowErrors()
                .Execute(piano.ProgramExecution);

            return (int)exitCode;
        }

       
    }
}