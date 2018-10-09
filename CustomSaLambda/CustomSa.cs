using System;
using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.Core;
using Cloud;
using Genome;
using IO;
using IO.StreamSource;
using SAUtils;
using SAUtils.Custom;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CustomSaLambda
{
    public sealed class CustomSa
    {
        public string LocalTempOutputPath = "/tmp/";
        private const string CreationSuccessMessage = "Custom Annotation Created";

        private readonly Dictionary<GenomeAssembly, string> _assemblyReferencePath = new Dictionary<GenomeAssembly, string>()
        {
            {GenomeAssembly.GRCh37, "https://illumina-annotation.s3.amazonaws.com/References/5/Homo_sapiens.GRCh37.Nirvana.dat"},
            {GenomeAssembly.GRCh38, "https://illumina-annotation.s3.amazonaws.com/References/5/Homo_sapiens.GRCh38.Nirvana.dat"}
        };

        public ApiResponse Create(CustomAnnotationConfig customSaConfig, ILambdaContext context)
        {
            ApiResponse result = new ApiResponse { id = customSaConfig.id };

            try
            {
                var genomeAssembly = GenomeAssemblyHelper.Convert(customSaConfig.genomeAssembly);

                if (!_assemblyReferencePath.TryGetValue(genomeAssembly, out string referencePath))
                {
                    throw new ArgumentOutOfRangeException($"Unsupported assembly: {customSaConfig.genomeAssembly}");
                }

                var referenceProvider = new ReferenceSequenceProvider(StreamSourceUtils.GetStream(NirvanaHelper.GetS3RefLocation(genomeAssembly)));

                var refNameToChromosome =
                    CacheUtils.Helpers.SequenceHelper.GetDictionaries(referencePath).refNameToChromosome;

                var inputS3Client = S3Utilities.GetS3ClientWrapperFromEnvironment(customSaConfig.inputTsv.bucketName);
                var customTsvStream = new S3StreamSource(inputS3Client, customSaConfig.inputTsv).GetStream();

                List<CustomInterval> intervals;
                string jsonTag;
                DataSourceVersion version;
                string localNsaPath;
                var outputFiles = new List<string>();

                using (var customReader = new CustomAnnotationsParser(FileUtilities.GetStreamReader(customTsvStream),
                    refNameToChromosome))
                using (var nsaStream = FileUtilities.GetCreateStream(localNsaPath = Path.Combine(LocalTempOutputPath, customReader.JsonTag.TrimEnd() + SaCommon.SaFileSuffix)))
                using (var indexStream = FileUtilities.GetCreateStream(localNsaPath + SaCommon.IndexSufix))
                using (var nsaWriter = new NsaWriter(new ExtendedBinaryWriter(nsaStream), new ExtendedBinaryWriter(indexStream),
                    version = new DataSourceVersion(customReader.JsonTag, customSaConfig.inputTsv.path, DateTime.Now.Ticks), referenceProvider, customReader.JsonTag, true, false,
                    SaCommon.SchemaVersion, false))
                {
                    jsonTag = customReader.JsonTag.TrimEnd();
                    nsaWriter.Write(customReader.GetItems());
                    intervals = customReader.GetCustomIntervals();
                }

                var outputS3Client = S3Utilities.GetS3ClientWrapperFromEnvironment(customSaConfig.outputDir.bucketName);

                outputFiles.Add(S3Utilities.UploadBaseAndIndexFiles(outputS3Client, customSaConfig.outputDir,
                    jsonTag + SaCommon.SaFileSuffix, jsonTag + SaCommon.SaFileSuffix, SaCommon.IndexSufix));


                if (intervals != null)
                {
                    string nsiFileName = jsonTag + SaCommon.SiFileSuffix;
                    string localNsiPath = Path.Combine(LocalTempOutputPath, nsiFileName);
                    using (var nsiStream = FileUtilities.GetCreateStream(localNsiPath))
                    using (var nsiWriter = new NsiWriter(new ExtendedBinaryWriter(nsiStream), version,
                        genomeAssembly, jsonTag, ReportFor.StructuralVariants, SaCommon.SchemaVersion))
                    {
                        nsiWriter.Write(intervals);
                    }
                    outputFiles.Add(outputS3Client.Upload(S3Utilities.CombineS3DirAndFileName(customSaConfig.outputDir, nsiFileName), localNsiPath));
                }

                result.created = new FileList
                {
                    bucketName = customSaConfig.outputDir.bucketName,
                    files = outputFiles.ToArray()
                };


                result.status = CreationSuccessMessage;
            }
            catch (Exception e)
            {
                result.status = e.Message;
            }

            return result;
        }

    }
}
