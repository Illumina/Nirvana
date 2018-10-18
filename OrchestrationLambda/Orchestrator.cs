using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Genome;
using Cloud;
using OptimizedCore;
using VariantAnnotation.Sequence;

// Assembly attribute to enable the Lambda function's JSON runConfig to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace OrchestrationLambda
{
    public sealed class Orchestrator
    {
        private const string RunSuccessStatus = "Annotation Complete";
        private const string RunFailedStatus = "One or more annotation Lambdas failed";
        private readonly HashSet<GenomeAssembly> _supportedAssemblies = new HashSet<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };
        private readonly string _functionArn;

        public Orchestrator()
        {
            _functionArn = Environment.GetEnvironmentVariable("annotation_lambda_arn");
        }

        public ApiResponse Run(NirvanaConfig runConfig, ILambdaContext context)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {


                var genomeAssembly = GenomeAssemblyHelper.Convert(runConfig.genomeAssembly);

                if (!_supportedAssemblies.Contains(genomeAssembly))
                {
                    throw new ArgumentOutOfRangeException($"Unsupported assembly: {runConfig.genomeAssembly}");
                }

                var s3Client = S3Utilities.GetS3ClientWrapperFromEnvironment(runConfig.inputVcf.bucketName);

                bool isGvcf = runConfig.inputVcf.path.EndsWith("genome.vcf.gz");
                long fileSize = s3Client.GetFileSize(runConfig.inputVcf);
                var tabxiStream = new S3StreamSource(s3Client, runConfig.inputVcf).GetAssociatedStreamSource(NirvanaHelper.TabixSuffix)
                    .GetStream();
                var chromosomesInVcf = GetChromesomeInVcf(tabxiStream, genomeAssembly);
                var annotationIntervals = Partition.GetChromosomeIntervals(fileSize, isGvcf, genomeAssembly, chromosomesInVcf);
                cancellationTokenSource.CancelAfter(NirvanaHelper.AnnotationTimeOut);
                return GetRunResult(annotationIntervals, runConfig, cancellationTokenSource);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception.Message}");

                return new ApiResponse
                {
                    id = runConfig.id,
                    status = RunFailedStatus
                };
            }
        }



        private ApiResponse GetRunResult(IEnumerable<IChromosomeInterval> annotationIntervals, NirvanaConfig runConfig, CancellationTokenSource cancellationTokenSource)
        {

            var annotationTasks = CallAnnotationLambdas(runConfig, cancellationTokenSource, annotationIntervals);

            var annotationLambdaResults = Task.WhenAll(annotationTasks).Result;
            var jsonSerializer = new JsonSerializer();
            var annotationResults = annotationLambdaResults
                .Select(jsonSerializer.Deserialize<AnnotationResult>).ToArray();

            var failedJobs = annotationResults.Select((x, i) => (Result: x, Index: i))
                .Where(x => x.Result.exitCode != 0 || x.Result.filePath == null).ToList();

            if (failedJobs.Count > 0)
            {
                Console.WriteLine("Failed jobs:");
                failedJobs.ForEach(x => Console.WriteLine($"Job #{x.Index} failed: {x.Result.status}"));

                throw new Exception(RunFailedStatus);
            }

            var filePaths = annotationResults.Select(x => x.filePath).ToArray();

            return new ApiResponse
            {
                id = runConfig.id,
                status = RunSuccessStatus,
                created = new FileList
                {
                    bucketName = runConfig.outputDir.bucketName,
                    files = filePaths
                }
            };
        }


        private Task<MemoryStream>[] CallAnnotationLambdas(NirvanaConfig runConfig, CancellationTokenSource cancellationTokenSource, IEnumerable<IChromosomeInterval> annotationIntervals) =>
            annotationIntervals?.Select((x, i) => RunAnnotationJob(runConfig, cancellationTokenSource, x, i + 1)).ToArray() ?? new[] { RunAnnotationJob(runConfig, cancellationTokenSource, null, 1) };

        private Task<MemoryStream> RunAnnotationJob(NirvanaConfig runConfig,
            CancellationTokenSource cancellationTokenSource, IChromosomeInterval interval, int jobIndex)
        {
            var jsonSerializer = new JsonSerializer();
            var memoryStream = new MemoryStream();
            var annotationConfig = GetAnnotationConfig(runConfig, interval, jobIndex);
            Console.Write($"Job Index: {jobIndex}, Annotation region: ");
            Console.WriteLine(annotationConfig.annotationRange != null
                ? $"{annotationConfig.annotationRange.chromosome}:{annotationConfig.annotationRange.start}-{annotationConfig.annotationRange.end}"
                : "Whole VCF");

            jsonSerializer.Serialize(annotationConfig, memoryStream);
            memoryStream.Position = 0;

            return Task.Factory.StartNew(
                () => AnnotationJob.Invoke(_functionArn, memoryStream, cancellationTokenSource.Token),
                cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private static AnnotationConfig GetAnnotationConfig(NirvanaConfig runConfig, IChromosomeInterval interval, int jobIndex) => new AnnotationConfig
        {
            id = runConfig.id,
            genomeAssembly = runConfig.genomeAssembly,
            inputVcf = runConfig.inputVcf,
            outputDir = runConfig.outputDir,
            outputPrefix = GetIndexedPrefix(runConfig.inputVcf.path, jobIndex),
            supplementaryAnnotations = runConfig.supplementaryAnnotations,
            annotationRange = interval == null ?
                null : new AnnotationRange
                {
                    chromosome = interval.Chromosome.UcscName,
                    start = interval.Start,
                    end = interval.End
                }
        };

        internal static string GetIndexedPrefix(string inputVcfPath, int jobIndex) =>
            inputVcfPath.TrimStartToLast("/").TrimEndFromFirst(".vcf") + "_" + jobIndex.ToString("00000");

        private static IEnumerable<IChromosome> GetChromesomeInVcf(Stream tabixStream, GenomeAssembly genomeAssembly)
        {
            var refFileLocation = NirvanaHelper.GetS3RefLocation(genomeAssembly);
            var refNameToChromosome = SequenceHelper.GetDictionaries(refFileLocation).refNameToChromosome;
            return Tabix.Reader.GetTabixIndex(tabixStream, refNameToChromosome).ReferenceSequences.Select(x => x.Chromosome);
        }
    }
}
