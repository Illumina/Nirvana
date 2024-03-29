using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Amazon.Lambda.Core;
using Cloud;
using Cloud.Messages.Single;
using Cloud.Utilities;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using Nirvana;
using OptimizedCore;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Utilities;
using JsonWriter = VariantAnnotation.IO.JsonWriter;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SingleAnnotationLambda
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedType.Global
    public class SingleAnnotationLambda
    {
        private const int MaxNumCacheConfigurations = 2;
        private readonly Dictionary<CacheConfiguration, AnnotationResources> _cacheConfigurationToAnnotationResources = new Dictionary<CacheConfiguration, AnnotationResources>();
        private readonly LinkedList<CacheConfiguration> _recentCacheConfigurations = new LinkedList<CacheConfiguration>();

        // ReSharper disable once UnusedMember.Global
        public Stream Run(SingleConfig config, ILambdaContext context)
        {
            string snsTopicArn = null;
            Stream response;

            try
            {
                LogUtilities.UpdateLogger(context.Logger, null);
                LogUtilities.LogLambdaInfo(context, CommandLineUtilities.InformationalVersion);
                LogUtilities.LogObject("Config", config);
                LogUtilities.Log(new[] { LambdaUrlHelper.UrlBaseEnvironmentVariableName, LambdaUtilities.SnsTopicKey });

                LambdaUtilities.GarbageCollect();

                snsTopicArn = LambdaUtilities.GetEnvironmentVariable(LambdaUtilities.SnsTopicKey);

                config.Validate();

                GenomeAssembly genomeAssembly = GenomeAssemblyHelper.Convert(config.genomeAssembly);
                
                var cacheConfiguration        = new CacheConfiguration(genomeAssembly, config.supplementaryAnnotations, config.vepVersion);
                bool preloadRequired          = !string.IsNullOrEmpty(config.supplementaryAnnotations);
                AnnotationResources annotationResources = GetAndCacheAnnotationResources(config, cacheConfiguration);

                if (genomeAssembly!=GenomeAssembly.hg19) annotationResources.Annotator.EnableMitochondrialAnnotation();

                (IPosition position, string[] sampleNames) = config.GetPositionAndSampleNames(annotationResources.SequenceProvider, annotationResources.RefMinorProvider);
                if (position.Chromosome.IsEmpty()) throw new UserErrorException($"An unknown chromosome was specified ({config.variant.chromosome})");

                string annotationResult = GetPositionAnnotation(position, annotationResources, sampleNames, preloadRequired);
                response = SingleResult.Create(config.id, LambdaUrlHelper.SuccessMessage, annotationResult);
            }
            catch (Exception exception)
            {
                response = ExceptionHandler.GetStream(config.id, snsTopicArn, exception);
            }

            return response;
        }

        private AnnotationResources GetAndCacheAnnotationResources(SingleConfig input, CacheConfiguration cacheConfiguration)
        {
            if (_cacheConfigurationToAnnotationResources.TryGetValue(cacheConfiguration, out AnnotationResources annotationResources))
            {
                if (!_recentCacheConfigurations.Last.Value.Equals(cacheConfiguration))
                {
                    _recentCacheConfigurations.Remove(cacheConfiguration);
                    _recentCacheConfigurations.AddLast(cacheConfiguration);
                    Logger.WriteLine($"Cached configurations: {string.Join("; ", _recentCacheConfigurations)}");
                }

                return annotationResources;
            }

            if (_recentCacheConfigurations.Count == MaxNumCacheConfigurations)
            {
                CacheConfiguration configurationToRemove = _recentCacheConfigurations.First.Value;
                _recentCacheConfigurations.RemoveFirst();
                _cacheConfigurationToAnnotationResources.Remove(configurationToRemove);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Logger.WriteLine($"Creating annotation resources for {cacheConfiguration}");
            annotationResources = GetAnnotationResources(input);
            _cacheConfigurationToAnnotationResources[cacheConfiguration] = annotationResources;
            _recentCacheConfigurations.AddLast(cacheConfiguration);
            Logger.WriteLine($"Cached configurations: {string.Join("; ", _recentCacheConfigurations)}");

            return annotationResources;
        }

        private static AnnotationResources GetAnnotationResources(SingleConfig lambdaConfig)
        {
            GenomeAssembly genomeAssembly = GenomeAssemblyHelper.Convert(lambdaConfig.genomeAssembly);
            string cachePathPrefix        = CacheUtilities.GetCachePathPrefix(lambdaConfig.vepVersion, genomeAssembly);
            string nirvanaS3Ref           = LambdaUrlHelper.GetRefUrl(genomeAssembly);

            string annotatorVersion     = "Nirvana " + CommandLineUtilities.GetVersion(Assembly.GetAssembly(typeof(SingleAnnotationLambda)));
            var metrics = new PerformanceMetrics();

            Logger.WriteLine($"Cache prefix: {cachePathPrefix}");
            //todo: get customStrTsv from lambdaConfig
            var annotationResources = new AnnotationResources(nirvanaS3Ref, cachePathPrefix,
                null, lambdaConfig.customAnnotations, null, false, false, metrics)
            {
                AnnotatorVersionTag = annotatorVersion
            };

            return annotationResources;
        }

        private static string GetPositionAnnotation(IPosition position, IAnnotationResources resources, string[] sampleNames, bool preloadRequired)
        {
            if (preloadRequired) resources.SingleVariantPreLoad(position);
            IAnnotatedPosition annotatedPosition = resources.Annotator.Annotate(position);
            var                sb                = annotatedPosition?.GetJsonStringBuilder();
            if (sb == null) throw new UserErrorException("No variant is provided for annotation");
            
            string json = StringBuilderPool.GetStringAndReturn(sb);
            if (json == null) throw new UserErrorException("No variant is provided for annotation");

            var outputJsonStream = new MemoryStream();
            using (var jsonWriter = new JsonWriter(outputJsonStream, null, resources, Date.CurrentTimeStamp, sampleNames, true))
            {
                WriteAnnotatedPosition(annotatedPosition, jsonWriter, json);
                jsonWriter.WriteGenes(resources.Annotator.GetGeneAnnotations());
            }

            outputJsonStream.Position = 0;
            return Encoding.UTF8.GetString(outputJsonStream.ToArray());
        }

        private static void WriteAnnotatedPosition(IAnnotatedPosition annotatedPosition, IJsonWriter jsonWriter,
            string jsonOutput) => jsonWriter.WritePosition(annotatedPosition.Position, jsonOutput);
    }
}
