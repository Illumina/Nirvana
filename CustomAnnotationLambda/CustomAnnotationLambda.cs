using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.Lambda.Core;
using Cloud;
using Cloud.Messages;
using Cloud.Messages.Custom;
using Cloud.Notifications;
using Cloud.Utilities;
using CommandLine.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.Custom;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CustomAnnotationLambda
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CustomAnnotationLambda
    {
        
        // ReSharper disable once UnusedMember.Global
        public CustomResult Run(CustomConfig config, ILambdaContext context)
        {
            var result = new CustomResult { id = config.id };
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

                config.CheckRequiredFieldsNotNull();
                var s3Client = config.outputDir.GetS3Client(context.RemainingTime);
                config.CheckResourcesExist();

                LambdaUtilities.DeleteTempOutput();

                string inputFileName = config.tsvUrl.TrimEndFromFirst("?").TrimStartToLast("/");
                Logger.WriteLine($"input file name is: {inputFileName}");

                return IsGeneAnnotationTsv(config.tsvUrl) 
                    ? GeneAnnotationCreator.Create(config, inputFileName, result, s3Client) 
                    : VariantAnnotationCreator.Create(config, inputFileName, result, s3Client);
            }
            catch (Exception e)
            {
                return HandleException(runLog, result, e, snsTopicArn);
            }
        }

        private static bool IsGeneAnnotationTsv(string tsvUrl)
        {
            using (var customTsvStream = (PersistentStream) PersistentStreamUtils.GetReadStream(tsvUrl))
            using (var reader = new StreamReader(customTsvStream))
            {
                reader.ReadLine();
                string secondLine = reader.ReadLine();
                if (secondLine == null) throw new UserErrorException("The input TSV file has less than two lines");

                return secondLine.StartsWith("#geneSymbol");
            }
        }

        public static CustomResult GetSuccessResult(CustomConfig customSaConfig, CustomResult result, List<string> outputFiles)
        {
            Logger.WriteLine("All files uploaded.");

            result.created = new FileList
            {
                bucketName = customSaConfig.outputDir.bucketName,
                outputDir = customSaConfig.outputDir.path,
                files = outputFiles.ToArray()
            };

            result.status = LambdaUtilities.SuccessMessage;

            LogUtilities.LogObject("Result", result);
            LambdaUtilities.DeleteTempOutput();

            return result;
        }

        private static CustomResult HandleException(StringBuilder runLog, CustomResult result, Exception e, string snsTopicArn)
        {
            Logger.Log(e);

            var errorCategory = ExceptionUtilities.ExceptionToErrorCategory(e);

            result.status = $"{errorCategory}: {e.Message}";
            result.noValidEntries = e.Message == GeneAnnotationsParser.NoValidEntriesErrorMessage;

            if (errorCategory != ErrorCategory.UserError)
            {
                string snsMessage = SNS.CreateMessage(runLog.ToString(), result.status, e.StackTrace);
                SNS.SendMessage(snsTopicArn, snsMessage);
            }

            LogUtilities.LogObject("Result", result);
            LambdaUtilities.DeleteTempOutput();

            return result;
        }
    }
}
