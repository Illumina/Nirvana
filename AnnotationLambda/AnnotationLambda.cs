using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Amazon.Lambda.Core;
using Cloud;
using Cloud.Messages.Annotation;
using Cloud.Notifications;
using Cloud.Utilities;
using CommandLine.Utilities;
using Compression.FileHandling;
using ErrorHandling;
using Genome;
using IO;
using Nirvana;
using Vcf;
using Tabix;
using VariantAnnotation;
using VariantAnnotation.SA;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AnnotationLambda
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AnnotationLambda
    {
        // ReSharper disable once UnusedMember.Global
        public AnnotationResult Run(AnnotationConfig config, ILambdaContext context)
        {
            var result = new AnnotationResult { id = config.id };
            string snsTopicArn = null;
            var runLog = new StringBuilder();

            try
            {
                LogUtilities.UpdateLogger(context.Logger, runLog);
                LogUtilities.LogLambdaInfo(context, CommandLineUtilities.InformationalVersion);
                LogUtilities.LogObject("Config", config);
                LogUtilities.Log(new[] { LambdaUrlHelper.UrlBaseEnvironmentVariableName, LambdaUtilities.SnsTopicKey });

                LambdaUtilities.GarbageCollect();
                LambdaUtilities.DeleteTempOutput();

                snsTopicArn = LambdaUtilities.GetEnvironmentVariable(LambdaUtilities.SnsTopicKey);

                string vcfUrl       = config.vcfUrl;
                int    variantCount = 0;
                using (var annotationResources = GetAnnotationResources(config))
                {
                    if (annotationResources.InputStartVirtualPosition == -1) return GetSuccessOutput(result);

                    long fileOffset = VirtualPosition.From(annotationResources.InputStartVirtualPosition).FileOffset;

                    using (var preloadVcfStream = PersistentStreamUtils.GetReadStream(vcfUrl, fileOffset))
                    {
                        var annotationRange = config.annotationRange?.ToGenomicRange(annotationResources.SequenceProvider.RefNameToChromosome);
                        annotationResources.GetVariantPositions(new BlockGZipStream(preloadVcfStream, CompressionMode.Decompress), annotationRange);
                    }

                    Logger.WriteLine("Scan for positions to preload complete.");
                    
                    using (var aes = new AesCryptoServiceProvider())
                    {
                        FileMetadata jsonMetadata, jasixMetadata;
                        string jsonPath = Path.GetTempPath() + LambdaUrlHelper.JsonSuffix;
                        string jasixPath = jsonPath + LambdaUrlHelper.JsonIndexSuffix;

                        using (var inputVcfStream = new BlockGZipStream(PersistentStreamUtils.GetReadStream(vcfUrl, fileOffset), CompressionMode.Decompress))
                        using (var headerStream = config.annotationRange == null ? null : new BlockGZipStream(PersistentStreamUtils.GetReadStream(vcfUrl), CompressionMode.Decompress))
                        //
                        using (var jsonFileStream = FileUtilities.GetCreateStream(jsonPath))
                        using (var jsonCryptoStream = new CryptoStream(jsonFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (var jsonMd5Stream = new MD5Stream(jsonCryptoStream))
                        //
                        using (var jasixFileStream = FileUtilities.GetCreateStream(jasixPath))
                        using (var jasixCryptoStream = new CryptoStream(jasixFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (var jasixMd5Stream = new MD5Stream(jasixCryptoStream))
                        {
                            IVcfFilter vcfFilter = config.annotationRange == null
                                ? new NullVcfFilter()
                                : new VcfFilter(config.annotationRange.ToGenomicRange(annotationResources.SequenceProvider.RefNameToChromosome));

                            using (var jsonCompressStream = new BlockGZipStream(jsonMd5Stream, CompressionMode.Compress))
                            {
                                variantCount = StreamAnnotation.Annotate(headerStream, inputVcfStream, jsonCompressStream, 
                                    jasixMd5Stream, annotationResources, vcfFilter, true, false,
                                    config.desiredVcfInfo == null? null: new HashSet<string>(config.desiredVcfInfo),
                                    config.desiredVcfSampleInfo == null? null: new HashSet<string>(config.desiredVcfSampleInfo)).variantCount;
                            }

                            Logger.WriteLine("Annotation done.");

                            jsonMetadata  = jsonMd5Stream.GetFileMetadata();
                            jasixMetadata = jasixMd5Stream.GetFileMetadata();
                        }

                        result.filePath = S3Utilities.GetKey(config.outputDir.path, config.outputPrefix + LambdaUrlHelper.JsonSuffix);
                        string jasixKey = result.filePath + LambdaUrlHelper.JsonIndexSuffix;

                        var s3Client = config.outputDir.GetS3Client(context.RemainingTime);
                        s3Client.DecryptUpload(config.outputDir.bucketName, jasixKey, jasixPath, aes, jasixMetadata);
                        s3Client.DecryptUpload(config.outputDir.bucketName, result.filePath, jsonPath, aes, jsonMetadata);

                        Logger.WriteLine("Nirvana result files uploaded.");
                    }
                }

                LambdaUtilities.DeleteTempOutput();
                if (string.IsNullOrEmpty(result.filePath)) throw new FileNotFoundException();

                result.variantCount = variantCount;
                return GetSuccessOutput(result);
            }
            catch (Exception exception)
            {
                LambdaUtilities.DeleteTempOutput();
                return HandleException(runLog, result, exception, snsTopicArn);
            }
        }

        private static AnnotationResult GetSuccessOutput(AnnotationResult result)
        {
            result.status = LambdaUtilities.SuccessMessage;
            LogUtilities.LogObject("Result", result);
            return result;
        }

        private static AnnotationResult HandleException(StringBuilder runLog, AnnotationResult result, Exception e, string snsTopicArn)
        {
            Logger.Log(e);

            result.status = e.Message;
            result.errorCategory = ExceptionUtilities.ExceptionToErrorCategory(e);
            Logger.WriteLine($"Error Category: {result.errorCategory}");

            if (result.errorCategory != ErrorCategory.UserError)
            {
                string snsMessage = SNS.CreateMessage(runLog.ToString(), result.status, e.StackTrace);
                SNS.SendMessage(snsTopicArn, snsMessage);
            }

            LogUtilities.LogObject("Result", result);
            return result;
        }

        internal static long GetTabixVirtualPosition(AnnotationRange annotationRange, Stream stream, Dictionary<string, Chromosome> refNameToChromosome)
        {
            // process the entire file if no range specified
            if (annotationRange == null) return 0;

            var tabixIndex = Reader.GetTabixIndex(stream, refNameToChromosome);
            return tabixIndex.GetOffset(annotationRange.Start.Chromosome, annotationRange.Start.Position);
        }

        private static AnnotationResources GetAnnotationResources(AnnotationConfig annotationConfig)
        {
            var genomeAssembly      = GenomeAssemblyHelper.Convert(annotationConfig.genomeAssembly);
            string cachePathPrefix  = LambdaUrlHelper.GetCacheFolder().UrlCombine(genomeAssembly.ToString()).UrlCombine(LambdaUrlHelper.DefaultCacheSource);
            string nirvanaS3Ref     = LambdaUrlHelper.GetRefUrl(genomeAssembly);
            // SaVersion will be provided as an environment variable. Defaults to "latest"
            string saVersion = Environment.GetEnvironmentVariable("SaVersion");
            string saManifestUrl    = LambdaUtilities.GetManifestUrl(string.IsNullOrEmpty(saVersion)? "latest": saVersion, genomeAssembly, SaCommon.SchemaVersion);
            var metrics = new PerformanceMetrics();

            var annotationResources = new AnnotationResources(nirvanaS3Ref, cachePathPrefix,
                saManifestUrl == null? null: new List<string> {saManifestUrl}, 
                annotationConfig.customAnnotations, 
                annotationConfig.customStrUrl, 
                false, 
                false, 
                metrics);

            using (var tabixStream = PersistentStreamUtils.GetReadStream(annotationConfig.tabixUrl))
            {
                annotationResources.InputStartVirtualPosition = GetTabixVirtualPosition(annotationConfig.annotationRange, tabixStream, annotationResources.SequenceProvider.RefNameToChromosome);
            }

            Logger.WriteLine($"Tabix position :{annotationResources.InputStartVirtualPosition}");

            return annotationResources;
        }
    }
}
