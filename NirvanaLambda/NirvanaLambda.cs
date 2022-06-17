using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Cloud;
using Cloud.Messages;
using Cloud.Messages.Annotation;
using Cloud.Messages.Nirvana;
using Cloud.Notifications;
using Cloud.Utilities;
using CommandLine.Utilities;
using Compression.FileHandling;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
using IO;
using Tabix;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Providers;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace NirvanaLambda
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class NirvanaLambda
    {
        private const string AnnotationLambdaFailedStatus = "One or more annotation Lambdas failed";
        private const string AnnotationLambdaKey          = "annotation_lambda_arn";
        private const string TryAgainMessage              = "Please try again later.";
        private const int MaxNumPartitions                = 30;
        private const int MinNumPartitions                = 6;
        private const int MinPartitionSize                = 10_000_000;

        private readonly HashSet<GenomeAssembly> _supportedAssemblies = new HashSet<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };

        // ReSharper disable once UnusedMember.Global
        public NirvanaResult Run(NirvanaConfig config, ILambdaContext context)
        {
            NirvanaResult result;
            string snsTopicArn = null;
            var runLog = new StringBuilder();

            try
            {
                LogUtilities.UpdateLogger(context.Logger, runLog);
                LogUtilities.LogLambdaInfo(context, CommandLineUtilities.InformationalVersion);
                LogUtilities.LogObject("Config", config);
                LogUtilities.Log(new[] { LambdaUrlHelper.UrlBaseEnvironmentVariableName, LambdaUtilities.SnsTopicKey, "annotation_lambda_arn" });

                LambdaUtilities.GarbageCollect();

                snsTopicArn                = LambdaUtilities.GetEnvironmentVariable(LambdaUtilities.SnsTopicKey);
                string annotationLambdaArn = LambdaUtilities.GetEnvironmentVariable(AnnotationLambdaKey);
                
                config.Validate();

                var genomeAssembly = GenomeAssemblyHelper.Convert(config.genomeAssembly);

                if (!_supportedAssemblies.Contains(genomeAssembly))
                    throw new UserErrorException($"Unsupported assembly: {config.genomeAssembly}");

                AnnotationRange[] annotationRanges = GetAnnotationRanges(config, genomeAssembly);
                result = GetNirvanaResult(annotationRanges, config, annotationLambdaArn, context, runLog, snsTopicArn);
            }
            catch (Exception exception)
            {
                result = HandleException(runLog, config, exception, snsTopicArn);
            }

            LogUtilities.LogObject("Result", result);

            return result;
        }
        
        private static AnnotationRange[] GetAnnotationRanges(NirvanaConfig config, GenomeAssembly genomeAssembly)
        {
            string cachePathPrefix = LambdaUtilities.GetCachePathPrefix(genomeAssembly);

            using Stream tabixStream      = PersistentStreamUtils.GetReadStream(config.tabixUrl);
            using var    tabixReader      = new BinaryReader(new BlockGZipStream(tabixStream, CompressionMode.Decompress));
            using Stream referenceStream  = PersistentStreamUtils.GetReadStream(LambdaUrlHelper.GetRefUrl(genomeAssembly));
            using var    sequenceProvider = new ReferenceSequenceProvider(referenceStream);
            
            long         vcfSize          = HttpUtilities.GetLength(config.vcfUrl);
            int          numPartitions    = Math.Max(Math.Min((int) ((vcfSize - 1) / MinPartitionSize + 1), MaxNumPartitions), MinNumPartitions);

            Tabix.Index tabixIndex   = Reader.Read(tabixReader, sequenceProvider.RefNameToChromosome);
            List<long>  blockOffsets = PartitionUtilities.GetFileOffsets(config.vcfUrl, numPartitions, tabixIndex);
            
            // stop early if we're going to annotate the entire file
            if (blockOffsets.Count == 1 && blockOffsets[0] == 0) return null;

            using var                        taProvider               = new TranscriptAnnotationProvider(cachePathPrefix, sequenceProvider, null);
            IntervalArray<ITranscript>[]     transcriptIntervalArrays = taProvider.TranscriptIntervalArrays;
            IntervalForest<IGene>            geneIntervalForest       = GeneForestGenerator.GetGeneForest(transcriptIntervalArrays);
            Dictionary<string, Chromosome> refNameToChromosome      = sequenceProvider.RefNameToChromosome;

            return PartitionUtilities.GenerateAnnotationRanges(blockOffsets, config.vcfUrl, geneIntervalForest, refNameToChromosome);
        }

        private static NirvanaResult HandleException(StringBuilder runLog, NirvanaConfig config, Exception e, string snsTopicArn)
        {
            Logger.Log(e);
            var errorCategory = ExceptionUtilities.ExceptionToErrorCategory(e);
            return GetNirvanaFailResult(runLog, config, errorCategory, e.Message, e.StackTrace, snsTopicArn);
        }

        private static NirvanaResult GetNirvanaFailResult(StringBuilder runLog, NirvanaConfig config, ErrorCategory errorCategory, string errorMessage, string stackTrace, string snsTopicArn)
        {
            string status = GetFailedRunStatus(errorCategory, errorMessage);

            if (errorCategory != ErrorCategory.UserError)
            {
                string snsMessage = SNS.CreateMessage(runLog.ToString(), status, stackTrace);
                SNS.SendMessage(snsTopicArn, snsMessage);
            }

            return new NirvanaResult
            {
                id           = config.id,
                status       = status,
                variantCount = 0,
                jwtFields    =  config.jwtFields

            };
        }

        internal static string GetFailedRunStatus(ErrorCategory errorCategory, string errorMessage)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (errorCategory)
            {
                case ErrorCategory.TimeOutError:
                    return "Timeout error: annotation of the VCF was not finished on time due to network congestion. " + 
                           TryAgainMessage;
                case ErrorCategory.InvocationThrottledError:
                    return "Invocation throttled error: there are too many lambdas currently running in this account. " +
                           TryAgainMessage;
                case ErrorCategory.UserError:
                    return "User error: " + FirstCharToLower(errorMessage);
                default:
                    return "Nirvana error: an unexpected annotation error occurred while annotating this VCF.";
            }
        }

        private static NirvanaResult GetNirvanaResult(AnnotationRange[] annotationRanges, NirvanaConfig config, string annotationLambdaArn, ILambdaContext context, StringBuilder runLog, string snsTopicArn)
        {
            Task<AnnotationResultSummary>[] annotationTasks = CallAnnotationLambdas(config, annotationLambdaArn, context, annotationRanges);
            AnnotationResultSummary[] processedAnnotationResults = Task.WhenAll(annotationTasks).Result;

            (ErrorCategory? errorCategory, string errorMessage) = GetMostSevereErrorCategoryAndMessage(processedAnnotationResults);
            if (errorCategory != null) return GetNirvanaFailResult(runLog, config, errorCategory.Value, errorMessage, null, snsTopicArn);

            string[] fileNames    = processedAnnotationResults.Select(x => x.FileName).ToArray();
            int      variantCount = processedAnnotationResults.Sum(x => x.VariantCount);

            return new NirvanaResult
            {
                id      = config.id,
                status  = LambdaUrlHelper.SuccessMessage,
                created = new FileList
                {
                    bucketName = config.outputDir.bucketName,
                    outputDir  = config.outputDir.path,
                    files      = fileNames
                },
                variantCount    = variantCount,
                jwtFields =  config.jwtFields

            };
        }

        private static (ErrorCategory?, string) GetMostSevereErrorCategoryAndMessage(IEnumerable<AnnotationResultSummary> annotationResultSummaries)
        {
            List<(AnnotationResultSummary Item, int Index)> failedJobs = annotationResultSummaries
                .Select(x => x ?? AnnotationResultSummary.Create(null, ErrorCategory.NirvanaError, "No result summary available for the annotation job."))
                .Select((x, i) => (Item: x, Index: i)).Where(x => x.Item.ErrorCategory != null).ToList();

            if (failedJobs.Count == 0) return (null, null);

            Logger.WriteLine(AnnotationLambdaFailedStatus);
            failedJobs.ForEach(x => Logger.WriteLine($"Job {x.Index + 1}: {x.Item.ErrorCategory} {x.Item.ErrorMessage}"));

            ErrorCategory? mostSevereError = failedJobs.Select(x => x.Item.ErrorCategory).Min();
            string errorMessage = mostSevereError == ErrorCategory.UserError 
                ? string.Join(";", failedJobs.Where(x => x.Item.ErrorCategory == mostSevereError).Select(x => x.Item.ErrorMessage).Distinct())
                : "";

            return (mostSevereError, errorMessage);
        }

        private static Task<AnnotationResultSummary>[] CallAnnotationLambdas(NirvanaConfig config, string annotationLambdaArn, ILambdaContext context,
            IEnumerable<AnnotationRange> annotationRanges) =>
            annotationRanges?.Select((x, i) => RunAnnotationJob(config, annotationLambdaArn, context, x, i + 1)).ToArray() ??
            new[] {RunAnnotationJob(config, annotationLambdaArn, context, null, 1)};

        private static Task<AnnotationResultSummary> RunAnnotationJob(NirvanaConfig config, string annotationLambdaArn, ILambdaContext context, AnnotationRange range, int jobIndex)
        {
            var annotationConfig = GetAnnotationConfig(config, range, jobIndex);
            Logger.WriteLine($"Job: {jobIndex}, Annotation region: {DescribeAnnotationRegion(range)}");

            string configString = JsonUtilities.Stringify(annotationConfig);

            var annotationJob = new AnnotationJob(context, jobIndex);
            return Task.Run(() => annotationJob.Invoke(annotationLambdaArn, configString));
        }

        private static string DescribeAnnotationRegion(AnnotationRange ar)
        {
            if (ar == null) return "Whole VCF";
            string ret = $"{ar.Start.Chromosome}:{ar.Start.Position}-";
            return ar.End == null ? ret : $"{ret}{ar.End?.Chromosome}:{ar.End?.Position}";
        }

        private static AnnotationConfig GetAnnotationConfig(NirvanaConfig config, AnnotationRange annotationRange, int jobIndex) => new()
        {
            id                = config.id + $"_job{jobIndex}",
            genomeAssembly    = config.genomeAssembly,
            vcfUrl            = config.vcfUrl,
            tabixUrl          = config.tabixUrl,
            outputDir         = config.outputDir,
            outputPrefix      = GetIndexedPrefix(config.vcfUrl, jobIndex),
            customAnnotations = config.customAnnotations,
            desiredVcfInfo    = config.desiredVcfInfo,
            desiredVcfSampleInfo     = config.desiredVcfSampleInfo,
            customStrUrl      = config.customStrUrl,
            annotationRange   = annotationRange
        };

        internal static string GetIndexedPrefix(string inputVcfPath, int jobIndex) =>
            inputVcfPath.TrimEndFromFirst("?").TrimStartToLast("/").TrimEndFromFirst(".vcf") + "_" + jobIndex.ToString("00000");

        private static string FirstCharToLower(string input) => string.IsNullOrEmpty(input) || char.IsLower(input[0])
            ? input
            : char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}
