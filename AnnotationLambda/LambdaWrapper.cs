using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Reflection;
using Amazon.Lambda.Core;
using Amazon.S3.Model;
using Cloud;
using Compression.FileHandling;
using ErrorHandling;
using Genome;
using IO;
using Nirvana;
using Vcf;
using Tabix;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AnnotationLambda
{
    public sealed class LambdaWrapper
    {
        public string LocalTempOutputPath = "/tmp/Nirvana_temp";
        private const string AnnotationSuccessMessage = "Annotation Complete";


        public AnnotationResult RunNirvana(AnnotationConfig annotationConfig, ILambdaContext context)
        {
            var output = new AnnotationResult { id = annotationConfig.id };
            try
            {
                //may not needed in future
                var tempFolder = new DirectoryInfo(LocalTempOutputPath).Parent;
                NirvanaHelper.CleanOutput(tempFolder == null ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : tempFolder.FullName);

                var inputS3Client = S3Utilities.GetS3ClientWrapperFromEnvironment(annotationConfig.inputVcf.bucketName);
                var outputS3Client = S3Utilities.GetS3ClientWrapperFromEnvironment(annotationConfig.outputDir.bucketName);

                var annotationResources = GetAnnotationResources(inputS3Client, annotationConfig, annotationConfig.genomeAssembly);

                var byteRange = new ByteRange(VirtualPosition.From(annotationResources.InputStartVirtualPosition).FileOffset, long.MaxValue);

                if (annotationConfig.annotationRange != null)
                {
                    Console.WriteLine($"Annotation range: {annotationConfig.annotationRange.chromosome} {annotationConfig.annotationRange.start} {annotationConfig.annotationRange.end}");
                }

                using (var preloadVcfStream = new S3StreamSource(inputS3Client, annotationConfig.inputVcf).GetStream(byteRange))
                {
                    annotationResources.GetVariantPositions(new BlockGZipStream(preloadVcfStream, CompressionMode.Decompress), annotationConfig.annotationRange);
                }
                Console.WriteLine("Variant preloading done.");

                using (var inputVcfStream = new BlockGZipStream(new S3StreamSource(inputS3Client, annotationConfig.inputVcf).GetStream(byteRange),
                    CompressionMode.Decompress))
                using (var headerStream = annotationConfig.annotationRange == null ? null : new BlockGZipStream(new S3StreamSource(inputS3Client, annotationConfig.inputVcf).GetStream(),
                    CompressionMode.Decompress))
                using (var outputJsonStream = new BlockGZipStream(FileUtilities.GetCreateStream(LocalTempOutputPath + NirvanaHelper.JsonSuffix),
                            CompressionMode.Compress))
                using (var outputJsonIndexStream = FileUtilities.GetCreateStream(LocalTempOutputPath + NirvanaHelper.JsonSuffix + NirvanaHelper.JsonIndexSuffix))
                {

                    IVcfFilter vcfFilter = annotationConfig.annotationRange == null
                        ? new NullVcfFilter() as IVcfFilter
                        : new VcfFilter(AnnotationRangeToChromosomeInterval(annotationConfig.annotationRange, annotationResources.SequenceProvider.RefNameToChromosome));


                    StreamAnnotation.Annotate(headerStream, inputVcfStream, outputJsonStream, outputJsonIndexStream, null, null,
                        annotationResources, vcfFilter);
                    Console.WriteLine("Annotation done.");
                }
                
                output.filePath = S3Utilities.UploadBaseAndIndexFiles(outputS3Client, annotationConfig.outputDir,
                    LocalTempOutputPath + NirvanaHelper.JsonSuffix,
                    annotationConfig.outputPrefix + NirvanaHelper.JsonSuffix, NirvanaHelper.JsonIndexSuffix);
                Console.WriteLine("Nirvana output files uploaded.");

                File.Delete(LocalTempOutputPath + NirvanaHelper.JsonSuffix);
                File.Delete(LocalTempOutputPath + NirvanaHelper.JsonSuffix + NirvanaHelper.JsonIndexSuffix);
                Console.WriteLine("Temp Nirvana output deleted.");

                output.status = AnnotationSuccessMessage;
                output.exitCode = ExitCodes.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                output.status = e.Message;
                output.exitCode = ExitCodeUtilities.GetExitCode(e.GetType());
                throw;
            }

            return output;
        }

        internal static long GetTabixVirtualPosition(AnnotationConfig annotationConfig, IS3Client s3Client, IDictionary<string, IChromosome> refNameToChromosome)
        {
            // process the entire file if no range specified
            if (annotationConfig.annotationRange == null) return 0;

            var tabixStream = new S3StreamSource(s3Client, annotationConfig.inputVcf).GetAssociatedStreamSource(NirvanaHelper.TabixSuffix).GetStream();
            var tabixIndex = Reader.GetTabixIndex(tabixStream, refNameToChromosome);
            var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, annotationConfig.annotationRange.chromosome);

            return tabixIndex.GetOffset(chromosome, annotationConfig.annotationRange.start);
        }

        private static AnnotationResources  GetAnnotationResources(IS3Client s3Client, AnnotationConfig annotationConfig, string genomeAssembly)
        {
            string cachePathPrefix = UrlCombine(NirvanaHelper.S3CacheFoler, genomeAssembly + "/" + NirvanaHelper.DefaultCacheSource);
            string nirvanaS3Ref = NirvanaHelper.GetS3RefLocation(GenomeAssemblyHelper.Convert(genomeAssembly));
            var saConfigFileLocation = GetSaConfigFileLocation(annotationConfig.supplementaryAnnotations, genomeAssembly);

            var annotationResources = new AnnotationResources(nirvanaS3Ref, cachePathPrefix, saConfigFileLocation , null, false, false, true, false, false);

            annotationResources.InputStartVirtualPosition = GetTabixVirtualPosition(annotationConfig, s3Client, annotationResources.SequenceProvider.RefNameToChromosome);

            return annotationResources;
        }

        private static List<string> GetSaConfigFileLocation(string versionTag, string genomeAssembly) => versionTag == null ? null :
            new List<string> { NirvanaHelper.S3Url + string.Join("_", versionTag, "SA", NirvanaHelper.ProjectName, genomeAssembly) + ".txt"};

        private static string UrlCombine(string baseUrl, string relativeUrl) => baseUrl.TrimEnd('/') + '/' + relativeUrl.TrimStart('/');

        private static IChromosomeInterval AnnotationRangeToChromosomeInterval(AnnotationRange annotationRange,
            IDictionary<string, IChromosome> refnameToChromosome) => new ChromosomeInterval(ReferenceNameUtilities.GetChromosome(refnameToChromosome, annotationRange.chromosome),
            annotationRange.start, annotationRange.end);
    }
}
