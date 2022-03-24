using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using MitoHeteroplasmy;
using OptimizedCore;
using VariantAnnotation;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using VariantAnnotation.Pools;
using VariantAnnotation.Utilities;
using Variants;
using Vcf;

namespace Nirvana
{
    public static class StreamAnnotation
    {
        public static (int variantCount, ExitCodes exitCode) Annotate(Stream headerStream, Stream inputVcfStream, Stream outputJsonStream,
            Stream outputJsonIndexStream, AnnotationResources annotationResources, IVcfFilter vcfFilter,
            bool ignoreEmptyChromosome, bool enableDq = false, HashSet<string> customInfoKeys=null)
        {
            var metrics = annotationResources.Metrics;
            PerformanceMetrics.ShowAnnotationHeader();

            Chromosome                currentChromosome        = Chromosome.GetEmptyChromosome("dummy");
            int                       numVariants              = 0;
            int                       variantCount             = 0;
            IMitoHeteroplasmyProvider mitoHeteroplasmyProvider = MitoHeteroplasmyReader.GetProvider();
            using (var vcfReader  = GetVcfReader(headerStream, inputVcfStream, annotationResources, vcfFilter, mitoHeteroplasmyProvider, enableDq, customInfoKeys))
            using (var jsonWriter = new JsonWriter(outputJsonStream, outputJsonIndexStream, annotationResources, Date.CurrentTimeStamp, vcfReader.GetSampleNames(), false))
            {
                try
                {
                    CheckGenomeAssembly(annotationResources, vcfReader);
                    SetMitochondrialAnnotationBehavior(annotationResources, vcfReader);
                    
                    IPosition position;

                    while ((position = vcfReader.GetNextPosition()) != null)
                    {
                        Chromosome chromosome = position.Chromosome;
                        if (ignoreEmptyChromosome && chromosome.IsEmpty()) continue;
                        
                        if (chromosome.Index != currentChromosome.Index)
                        {
                            if (!currentChromosome.IsEmpty())
                                metrics.ShowAnnotationEntry(currentChromosome, numVariants);
                            
                            numVariants = 0;
                            
                            metrics.Preload.Start();
                            annotationResources.PreLoad(chromosome);
                            metrics.Preload.Stop();
                            
                            metrics.Annotation.Start();
                            currentChromosome = chromosome;
                        }

                        var annotatedPosition = position.Variants != null ? annotationResources.Annotator.Annotate(position) : null;

                        var jsb = annotatedPosition?.GetJsonStringBuilder();
                        if (jsb != null) jsonWriter.WritePosition(annotatedPosition.Position, jsb);
                        StringBuilderPool.Return(jsb);
                        
                        ReturnPoolObjects(annotatedPosition);

                        numVariants++;
                        variantCount += position.Variants?.Length ?? 0;
                    }

                    jsonWriter.WriteGenes(annotationResources.Annotator.GetGeneAnnotations());

                }
                catch (Exception e)
                {
                    e.Data[ExitCodeUtilities.VcfLine] = vcfReader.VcfLine;
                    throw;
                }
            }
            
            if (!currentChromosome.IsEmpty())
                metrics.ShowAnnotationEntry(currentChromosome, numVariants);

            metrics.ShowSummaryTable();

            return (variantCount, ExitCodes.Success);
        }

        private static void ReturnPoolObjects(IAnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition?.AnnotatedVariants != null)
                foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
                {
                    if (annotatedVariant.Transcripts != null)
                    {
                        foreach (IAnnotatedTranscript annotatedTranscript in annotatedVariant.Transcripts)
                        {
                            AnnotatedTranscriptPool.Return((AnnotatedTranscript) annotatedTranscript);
                        }
                    }

                    var variant = annotatedVariant.Variant;
                    if (variant is Variant) VariantPool.Return((Variant) annotatedVariant.Variant);
                    AnnotatedVariantPool.Return((AnnotatedVariant) annotatedVariant);
                }

            PositionPool.Return((Position) annotatedPosition?.Position);
            AnnotatedPositionPool.Return((AnnotatedPosition) annotatedPosition);
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
            IVcfFilter vcfFilter, IMitoHeteroplasmyProvider mitoHeteroplasmyProvider, bool enableDq = false, HashSet<string> customInfoKeys=null)
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
                annotationResources.RefMinorProvider, annotationResources.Recomposer, vcfFilter, annotationResources.VidCreator, 
                mitoHeteroplasmyProvider, enableDq, customInfoKeys);
        }
    }
}