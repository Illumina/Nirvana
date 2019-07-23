using System;
using System.IO;
using Compression.FileHandling;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.IO;
using VariantAnnotation.Logger;
using VariantAnnotation.Utilities;
using Vcf;

namespace Nirvana
{
    public static class StreamAnnotation
    {
        public static ExitCodes Annotate(Stream headerStream, Stream inputVcfStream, Stream outputJsonStream,
            Stream outputJsonIndexStream, AnnotationResources annotationResources, IVcfFilter vcfFilter, bool ignoreEmptyChromosome = false)
        {
            var logger = outputJsonStream is BlockGZipStream ? new ConsoleLogger() : (ILogger)new NullLogger();
            var metrics = new PerformanceMetrics(logger);

            using(annotationResources)
            using (var vcfReader = GetVcfReader(headerStream, inputVcfStream, annotationResources, vcfFilter))
            using (var jsonWriter = new JsonWriter(outputJsonStream, outputJsonIndexStream, annotationResources, Date.CurrentTimeStamp, vcfReader.GetSampleNames(), false))
            {
                try
                {
                    CheckGenomeAssembly(annotationResources, vcfReader);
                    SetMitochondrialAnnotationBehavior(annotationResources, vcfReader);

                    int previousChromIndex = -1;
                    IPosition position;

                    while ((position = vcfReader.GetNextPosition()) != null)
                    {
                        if (ignoreEmptyChromosome && position.Chromosome.IsEmpty()) continue;
                        if (previousChromIndex != position.Chromosome.Index)
                            annotationResources.PreLoad(position.Chromosome);
                        previousChromIndex = UpdatePerformanceMetrics(previousChromIndex, position.Chromosome, metrics);

                        var annotatedPosition = position.Variants != null ? annotationResources.Annotator.Annotate(position) : null;

                        string json = annotatedPosition?.GetJsonString();
                        if (json != null) jsonWriter.WriteJsonEntry(annotatedPosition.Position, json);

                        metrics.Increment();
                    }

                    jsonWriter.WriteAnnotatedGenes(annotationResources.Annotator.GetGeneAnnotations());

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

        private static void CheckGenomeAssembly(IAnnotationResources annotationResources, VcfReader vcfReader)
        {
            if (vcfReader.InferredGenomeAssembly != GenomeAssembly.Unknown && vcfReader.InferredGenomeAssembly != annotationResources.Annotator.Assembly)
                throw new UserErrorException($"A mismatch between genome assemblies was found. The input VCF uses {vcfReader.InferredGenomeAssembly} whereas annotation was configured for {annotationResources.Annotator.Assembly}.");
        }

        private static void SetMitochondrialAnnotationBehavior(IAnnotationResources annotationResources, IVcfReader vcfReader)
        {
            if (vcfReader.IsRcrsMitochondrion && annotationResources.Annotator.Assembly == GenomeAssembly.GRCh37
                || annotationResources.Annotator.Assembly == GenomeAssembly.GRCh38
                || annotationResources.ForceMitochondrialAnnotation)
                annotationResources.Annotator.EnableMitochondrialAnnotation();
        }

        private static VcfReader GetVcfReader(Stream headerStream, Stream vcfStream, IAnnotationResources annotationResources,
            IVcfFilter vcfFilter)
        {
            var vcfReader = FileUtilities.GetStreamReader(vcfStream);

            StreamReader headerReader;
            if (headerStream == null)
                headerReader = vcfReader;
            else
            {
                headerReader = FileUtilities.GetStreamReader(headerStream);
                vcfStream.Position = Tabix.VirtualPosition.From(annotationResources.InputStartVirtualPosition).BlockOffset;
            }

            return VcfReader.Create(headerReader, vcfReader, annotationResources.SequenceProvider,
                annotationResources.RefMinorProvider, annotationResources.Recomposer, vcfFilter);
        }

        private static int UpdatePerformanceMetrics(int previousChromIndex, IChromosome chromosome,
            PerformanceMetrics metrics)
        {
            // ReSharper disable once InvertIf
            if (chromosome.Index != previousChromIndex)
            {
                metrics.StartAnnotatingReference(chromosome);
                previousChromIndex = chromosome.Index;
            }

            return previousChromIndex;
        }
    }
}