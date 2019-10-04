using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Cloud;
using Cloud.Messages.Gene;
using Cloud.Notifications;
using Cloud.Utilities;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;
using IO;
using Jasix.DataStructures;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace GeneAnnotationLambda
{
    // ReSharper disable once UnusedMember.Global
    public class GeneAnnotationLambda
    {
        private readonly string _saPathPrefix = NirvanaHelper.S3Url;
        private readonly string _saManifestUrl = $"{NirvanaHelper.S3Url}latest_SA_GRCh37.txt";

        // ReSharper disable once UnusedMember.Global
        public Stream Run(GeneConfig config, ILambdaContext context)
        {
            string snsTopicArn = null;
            var runLog = new StringBuilder();

            try
            {
                LogUtilities.UpdateLogger(context.Logger, runLog);
                LogUtilities.LogLambdaInfo(context, CommandLineUtilities.InformationalVersion);
                LogUtilities.LogObject("Config", config);
                LogUtilities.Log(new[] { NirvanaHelper.UrlBaseEnvironmentVariableName, LambdaUtilities.SnsTopicKey });

                LambdaUtilities.GarbageCollect();

                snsTopicArn = LambdaUtilities.GetEnvironmentVariable(LambdaUtilities.SnsTopicKey);

                config.Validate();

                string result = GetGeneAnnotation(config, _saManifestUrl, _saPathPrefix);
                
                return LambdaResponse.Create(config.id, NirvanaHelper.SuccessMessage, result);
            }
            catch (Exception e)
            {
                return HandleException(config.id, snsTopicArn, e);
            }
        }

        private Stream HandleException(string id, string snsTopicArn, Exception e)
        {
            string snsMessage = SNS.CreateMessage(e.Message, "exception", e.StackTrace);
            SNS.SendMessage(snsTopicArn, snsMessage);

            return LambdaResponse.Create(id, e.Message, null);
        }

        public static string GetGeneAnnotation(GeneConfig input, string saManifestFilePath, string saPathPrefix)
        {
            var geneAnnotationProvider = new GeneAnnotationProvider(PersistentStreamUtils.GetStreams(
                                        GetNgaFileList(saManifestFilePath, saPathPrefix, input.ngaUrls).ToList()));

            var sb = new StringBuilder(1024 * 1024);
            var jsonObject = new JsonObject(sb);
            
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue(JasixCommons.HeaderSectionTag, GetHeader(geneAnnotationProvider), false);
            
            //not all gene symbols have annotations. So, we need to check and only output the ones that are not null
            var geneAnnotations = input.geneSymbols.Select(geneSymbol => geneAnnotationProvider.Annotate(geneSymbol))
                                                   .Where(annotation => !string.IsNullOrEmpty(annotation))
                                                   .ToList();

            jsonObject.AddStringValues("genes", geneAnnotations, false);
            sb.Append(JsonObject.CloseBrace);

            // AWS lambda response message can not be larger than 6MB
            if (sb.Length > 6_000_000)
                throw new UserErrorException("Too many genes provided in the request. Please decrease the number of genes and try again later.");
            
            return sb.ToString();
        }

        private static string GetHeader(IProvider geneAnnotationProvider)
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("annotator", "Nirvana " + CommandLineUtilities.Version);
            jsonObject.AddStringValue("creationTime", Date.CurrentTimeStamp);
            jsonObject.AddIntValue("schemaVersion", SaCommon.SchemaVersion);
            jsonObject.AddObjectValues("dataSources", geneAnnotationProvider.DataSourceVersions);
            sb.Append(JsonObject.CloseBrace);

            return sb.ToString();
        }

        public static IEnumerable<string> GetNgaFileList(string saManifestPath, string saPathPrefix, string[] ngaFiles)
        {
            using (var reader = new StreamReader(PersistentStreamUtils.GetReadStream(saManifestPath)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string filePath = saPathPrefix + line;
                    string suffix = filePath.GetFileSuffix(true);
                    if (suffix == SaCommon.NgaFileSuffix) yield return filePath;
                }
            }

            if (ngaFiles == null) yield break;
            
            foreach (string ngaFile in ngaFiles) yield return ngaFile;
        }
    }
}