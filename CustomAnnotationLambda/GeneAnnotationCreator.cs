using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cloud;
using Cloud.Messages.Custom;
using Cloud.Utilities;
using Compression.Utilities;
using IO;
using SAUtils.Custom;
using SAUtils.GeneIdentifiers;
using VariantAnnotation.SA;

namespace CustomAnnotationLambda
{
    public static class GeneAnnotationCreator
    {
        private const string LogFileName = "unrecognizedGeneIds.txt";
        public static CustomResult Create(CustomConfig config, string inputBaseName, CustomResult result, IS3Client s3Client)
        {
            string ngaFileName = inputBaseName + SaCommon.NgaFileSuffix;
            string localNgaPath = Path.Combine(Path.GetTempPath(), ngaFileName);
            string localSchemaPath = localNgaPath + SaCommon.JsonSchemaSuffix;
            string localLogPath = Path.Combine(Path.GetTempPath(), LogFileName);
            
            HttpUtilities.ValidateUrl(LambdaUrlHelper.GetUgaUrl());
            var outputFiles = new List<string>();
            using (var aes = new AesCryptoServiceProvider())
            {
                FileMetadata ngaMetadata, schemaMetadata, logMetaData;
                using (var logStream = FileUtilities.GetCreateStream(localLogPath))
                using (var logCryptoStream = new CryptoStream(logStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var logMd5Stream = new MD5Stream(logCryptoStream))
                //
                using (var customTsvStream = (PersistentStream)PersistentStreamUtils.GetReadStream(config.tsvUrl))
                using (var parser = GetGeneAnnotationsParserFromCustomTsvStream(customTsvStream))
                //
                using (var ngaStream = FileUtilities.GetCreateStream(localNgaPath))
                using (var ngaCryptoStream = new CryptoStream(ngaStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var ngaMd5Stream = new MD5Stream(ngaCryptoStream))
                //
                using (var schemaStream = FileUtilities.GetCreateStream(localSchemaPath))
                using (var schemaCryptoStream = new CryptoStream(schemaStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var schemaMd5Stream = new MD5Stream(schemaCryptoStream))
                {
                    using (var ngaWriter    = CaUtilities.GetNgaWriter(ngaMd5Stream, parser, config.tsvUrl))
                    using (var schemaWriter = new StreamWriter(schemaMd5Stream))
                    using (var logWriter    = new StreamWriter(logMd5Stream))
                    {
                        ngaWriter.Write(parser.GetItems(config.skipGeneIdValidation, logWriter));
                        schemaWriter.Write(parser.JsonSchema);
                    }
                    //all the writers have to be disposed before GetFileMetaData is called

                    ngaMetadata = ngaMd5Stream.GetFileMetadata();
                    schemaMetadata = schemaMd5Stream.GetFileMetadata();
                    logMetaData = logMd5Stream.GetFileMetadata();
                }

                if (config.skipGeneIdValidation)
                {
                    string logS3Key = string.Join('/', config.outputDir.path.Trim('/'), LogFileName);
                    Logger.WriteLine("uploading log file to " + logS3Key);
                    s3Client.DecryptUpload(config.outputDir.bucketName, logS3Key, localLogPath, aes, logMetaData);
                }

                string nsaS3Path = string.Join('/', config.outputDir.path.Trim('/'), ngaFileName);
                string schemaS3Path = nsaS3Path + SaCommon.JsonSchemaSuffix;

                s3Client.DecryptUpload(config.outputDir.bucketName, nsaS3Path, localNgaPath, aes, ngaMetadata);
                s3Client.DecryptUpload(config.outputDir.bucketName, schemaS3Path, localSchemaPath, aes, schemaMetadata);

                
                outputFiles.Add(ngaFileName);
                outputFiles.Add(ngaFileName + SaCommon.JsonSchemaSuffix);

                LambdaUtilities.DeleteTempOutput();

                return CustomAnnotationLambda.GetSuccessResult(config, result, outputFiles);
            }
        }

        private static GeneAnnotationsParser GetGeneAnnotationsParserFromCustomTsvStream(PersistentStream customTsvStream)
        {
            var (entrezGeneIdToSymbol, ensemblGeneIdToSymbol) = GeneUtilities.ParseUniversalGeneArchive(null, LambdaUrlHelper.GetUgaUrl());
            return GeneAnnotationsParser.Create(new StreamReader(GZipUtilities.GetAppropriateStream(customTsvStream)), entrezGeneIdToSymbol, ensemblGeneIdToSymbol);
        }
    }
}