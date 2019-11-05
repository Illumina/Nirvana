using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cloud;
using Cloud.Messages.Custom;
using Cloud.Utilities;
using Compression.Utilities;
using Genome;
using IO;
using SAUtils.Custom;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace CustomAnnotationLambda
{
    public static class VariantAnnotationCreator
    {
        public static CustomResult Create(CustomConfig config, string inputBaseName, CustomResult result, IS3Client s3Client)
        {
            string tempPath        = Path.GetTempPath();
            string nsaFileName     = inputBaseName + SaCommon.SaFileSuffix;
            string localNsaPath    = Path.Combine(tempPath, nsaFileName);
            string localIndexPath  = localNsaPath + SaCommon.IndexSufix;
            string localSchemaPath = localNsaPath + SaCommon.JsonSchemaSuffix;

            var outputFiles = new List<string>();
            using (var aes = new AesCryptoServiceProvider())
            {
                FileMetadata nsaMetadata, indexMetadata, schemaMetadata;

                List<CustomInterval> intervals;
                string jsonTag;
                SaJsonSchema intervalJsonSchema;
                DataSourceVersion version;
                GenomeAssembly genomeAssembly;
                int nsaItemsCount;

                using (var customTsvStream = (PersistentStream) PersistentStreamUtils.GetReadStream(config.tsvUrl))
                using (var parser = GetVariantAnnotationsParserFromCustomTsvStream(customTsvStream))
                    //
                using (var nsaStream = FileUtilities.GetCreateStream(localNsaPath))
                using (var nsaCryptoStream = new CryptoStream(nsaStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var nsaMd5Stream = new MD5Stream(nsaCryptoStream))
                    //
                using (var indexStream = FileUtilities.GetCreateStream(localIndexPath))
                using (var indexCryptoStream = new CryptoStream(indexStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var indexMd5Stream = new MD5Stream(indexCryptoStream))
                    //
                using (var schemaStream       = FileUtilities.GetCreateStream(localSchemaPath))
                using (var schemaCryptoStream = new CryptoStream(schemaStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var schemaMd5Stream    = new MD5Stream(schemaCryptoStream))
                {
                    genomeAssembly = parser.Assembly;
                    result.genomeAssembly = genomeAssembly.ToString();

                    using (var nsaWriter    = CaUtilities.GetNsaWriter(nsaMd5Stream, indexMd5Stream, parser, config.tsvUrl, parser.SequenceProvider, out version))
                    using (var schemaWriter = new StreamWriter(schemaMd5Stream))
                    {
                        (jsonTag, nsaItemsCount, intervalJsonSchema, intervals) = CaUtilities.WriteSmallVariants(parser, nsaWriter, schemaWriter);
                    }

                    nsaMetadata    = nsaMd5Stream.GetFileMetadata();
                    indexMetadata  = indexMd5Stream.GetFileMetadata();
                    schemaMetadata = schemaMd5Stream.GetFileMetadata();
                }

                if (nsaItemsCount > 0)
                {
                    string nsaS3Path    = string.Join('/', config.outputDir.path.Trim('/'), nsaFileName);
                    string indexS3Path  = nsaS3Path + SaCommon.IndexSufix;
                    string schemaS3Path = nsaS3Path + SaCommon.JsonSchemaSuffix;

                    s3Client.DecryptUpload(config.outputDir.bucketName, nsaS3Path, localNsaPath, aes, nsaMetadata);
                    s3Client.DecryptUpload(config.outputDir.bucketName, indexS3Path, localIndexPath, aes,
                        indexMetadata);
                    s3Client.DecryptUpload(config.outputDir.bucketName, schemaS3Path, localSchemaPath, aes,
                        schemaMetadata);

                    outputFiles.Add(nsaFileName);
                    outputFiles.Add(nsaFileName + SaCommon.IndexSufix);
                    outputFiles.Add(nsaFileName + SaCommon.JsonSchemaSuffix);
                }

                if (intervals == null) return CustomAnnotationLambda.GetSuccessResult(config, result, outputFiles);

                FileMetadata nsiMetadata, nsiSchemaMetadata;
                string nsiFileName = inputBaseName + SaCommon.SiFileSuffix;
                string localNsiPath = Path.Combine(tempPath, nsiFileName);
                string localNsiSchemaPath = localNsiPath + SaCommon.JsonSchemaSuffix;
                //
                using (var nsiStream = FileUtilities.GetCreateStream(localNsiPath))
                using (var nsiCryptoStream = new CryptoStream(nsiStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var nsiMd5Stream = new MD5Stream(nsiCryptoStream))
                    //
                using (var nsiSchemaSteam = FileUtilities.GetCreateStream(localNsiSchemaPath))
                using (var nsiSchemaCryptoStream =
                    new CryptoStream(nsiSchemaSteam, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var nsiSchemaMd5Stream = new MD5Stream(nsiSchemaCryptoStream))
                {
                    using (var nsiWriter = CaUtilities.GetNsiWriter(nsiMd5Stream, version, genomeAssembly, jsonTag))
                    using (var schemaWriter = new StreamWriter(nsiSchemaMd5Stream))
                    {
                        nsiWriter.Write(intervals);
                        schemaWriter.Write(intervalJsonSchema);
                    }

                    nsiMetadata = nsiMd5Stream.GetFileMetadata();
                    nsiSchemaMetadata = nsiSchemaMd5Stream.GetFileMetadata();
                }

                string nsiS3Path = string.Join('/', config.outputDir.path.Trim('/'), nsiFileName);
                string nsiSchemaS3PathFile = nsiS3Path + SaCommon.JsonSchemaSuffix;

                s3Client.DecryptUpload(config.outputDir.bucketName, nsiS3Path, localNsiPath, aes, nsiMetadata);
                s3Client.DecryptUpload(config.outputDir.bucketName, nsiSchemaS3PathFile, localNsiSchemaPath, aes,
                    nsiSchemaMetadata);

                outputFiles.Add(nsiFileName);
                outputFiles.Add(nsiFileName + SaCommon.JsonSchemaSuffix);
            }

            LambdaUtilities.DeleteTempOutput();

            return CustomAnnotationLambda.GetSuccessResult(config, result, outputFiles);
        }

        private static VariantAnnotationsParser GetVariantAnnotationsParserFromCustomTsvStream(PersistentStream customTsvStream)
        {
            var parser = VariantAnnotationsParser.Create(new StreamReader(GZipUtilities.GetAppropriateStream(customTsvStream)));

            LambdaUtilities.ValidateCoreData(parser.Assembly);

            parser.SequenceProvider = new ReferenceSequenceProvider(PersistentStreamUtils.GetReadStream(LambdaUrlHelper.GetRefUrl(parser.Assembly)));

            return parser;
        }
    }
}