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
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace CustomAnnotationLambda
{
    public static class VariantAnnotationCreator
    {
        public static CustomResult Create(CustomConfig config, string inputFileName, CustomResult result, IS3Client s3Client)
        {
            string tempPath        = Path.GetTempPath();
            string inputBaseName   = inputFileName.TrimEndFromFirst(".tsv");
            string nsaFileName     = inputBaseName + SaCommon.SaFileSuffix;
            string localNsaPath    = Path.Combine(tempPath, nsaFileName);
            string localIndexPath  = localNsaPath + SaCommon.IndexSuffix;
            string localSchemaPath = localNsaPath + SaCommon.JsonSchemaSuffix;
            int    variantCount    = 0;

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
                ReportFor reportFor;

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
                    genomeAssembly        = parser.Assembly;
                    result.genomeAssembly = genomeAssembly.ToString();
                    reportFor             = parser.ReportFor;
                    result.jwtFields      = config.jwtFields;

                    using (var nsaWriter    = CaUtilities.GetNsaWriter(nsaMd5Stream, indexMd5Stream, parser, inputFileName, parser.SequenceProvider, out version, config.skipRefBaseValidation))
                    using (var schemaWriter = new StreamWriter(schemaMd5Stream))
                    {
                        (jsonTag, nsaItemsCount, intervalJsonSchema, intervals) = CaUtilities.WriteSmallVariants(parser, nsaWriter, schemaWriter);
                    }

                    variantCount += nsaItemsCount;
                    variantCount += intervals?.Count ?? 0;

                    nsaMetadata    = nsaMd5Stream.GetFileMetadata();
                    indexMetadata  = indexMd5Stream.GetFileMetadata();
                    schemaMetadata = schemaMd5Stream.GetFileMetadata();
                }

                result.variantCount = variantCount;
                if (nsaItemsCount > 0)
                {
                    string nsaS3Path    = string.Join('/', config.outputDir.path.Trim('/'), nsaFileName);
                    string indexS3Path  = nsaS3Path + SaCommon.IndexSuffix;
                    string schemaS3Path = nsaS3Path + SaCommon.JsonSchemaSuffix;

                    s3Client.DecryptUpload(config.outputDir.bucketName, nsaS3Path, localNsaPath, aes, nsaMetadata);
                    s3Client.DecryptUpload(config.outputDir.bucketName, indexS3Path, localIndexPath, aes,
                        indexMetadata);
                    s3Client.DecryptUpload(config.outputDir.bucketName, schemaS3Path, localSchemaPath, aes,
                        schemaMetadata);

                    outputFiles.Add(nsaFileName);
                    outputFiles.Add(nsaFileName + SaCommon.IndexSuffix);
                    outputFiles.Add(nsaFileName + SaCommon.JsonSchemaSuffix);
                }

                if (intervals == null) return CustomAnnotationLambda.GetSuccessResult(config, result, outputFiles);

                FileMetadata nsiMetadata, nsiSchemaMetadata;
                string nsiFileName = inputBaseName + SaCommon.IntervalFileSuffix;
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
                    using (var nsiWriter = CaUtilities.GetNsiWriter(nsiMd5Stream, version, genomeAssembly, jsonTag, reportFor))
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

            parser.SequenceProvider = new ReferenceSequenceProvider(PersistentStreamUtils.GetReadStream(LambdaUrlHelper.GetRefUrl(parser.Assembly)));

            return parser;
        }
    }
}