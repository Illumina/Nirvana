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
        public static CustomResult Create(CustomConfig config, string inputBaseName, CustomResult result, IS3Client s3Client)
        {
            string ngaFileName = inputBaseName + SaCommon.NgaFileSuffix;
            string localNgaPath = Path.Combine(Path.GetTempPath(), ngaFileName);
            string localSchemaPath = localNgaPath + SaCommon.JsonSchemaSuffix;

            var outputFiles = new List<string>();
            using (var aes = new AesCryptoServiceProvider())
            {
                FileMetadata ngaMetadata, schemaMetadata;

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
                    using (var ngaWriter = CaUtilities.GetNgaWriter(ngaMd5Stream, parser, config.tsvUrl))
                    using (var schemaWriter = new StreamWriter(schemaMd5Stream))
                    {
                        ngaWriter.Write(parser.GetItems());
                        schemaWriter.Write(parser.JsonSchema);
                    }

                    ngaMetadata = ngaMd5Stream.GetFileMetadata();
                    schemaMetadata = schemaMd5Stream.GetFileMetadata();
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
            var (entrezGeneIdToSymbol, ensemblGeneIdToSymbol) = GeneUtilities.ParseUniversalGeneArchive(null, NirvanaHelper.S3UgaPath);
            return GeneAnnotationsParser.Create(new StreamReader(GZipUtilities.GetAppropriateStream(customTsvStream)), entrezGeneIdToSymbol, ensemblGeneIdToSymbol);
        }
    }
}