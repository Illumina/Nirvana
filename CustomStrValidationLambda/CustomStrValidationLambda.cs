using System;
using System.IO;
using Amazon.Lambda.Core;
using Cloud;
using Cloud.Messages.StrValidation;
using Cloud.Notifications;
using Cloud.Utilities;
using CommandLine.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using Nirvana;
using RepeatExpansions.IO;
using VariantAnnotation.Interface.Providers;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CustomStrValidationLambda
{
    public class CustomStrValidationLambda
    {
        public ValidationResult Run(ValidationConfig config, ILambdaContext context)
        {
            string snsTopicArn = null;

            try
            {
                LogUtilities.UpdateLogger(context.Logger, null);
                LogUtilities.LogLambdaInfo(context, CommandLineUtilities.InformationalVersion);
                LogUtilities.LogObject("Config", config);
                LogUtilities.Log(new[] { LambdaUrlHelper.UrlBaseEnvironmentVariableName, LambdaUtilities.SnsTopicKey });
                LambdaUtilities.GarbageCollect();
                snsTopicArn = LambdaUtilities.GetEnvironmentVariable(LambdaUtilities.SnsTopicKey);

                config.Validate();
                GenomeAssembly genomeAssembly = GenomeAssemblyHelper.Convert(config.genomeAssembly);

                string nirvanaS3Ref = LambdaUrlHelper.GetRefUrl(genomeAssembly);
                var refProvider = ProviderUtilities.GetSequenceProvider(nirvanaS3Ref);

                using (var stream = PersistentStreamUtils.GetReadStream(config.customStrUrl))
                    TryLoadStrFile(stream, genomeAssembly, refProvider);
            }
            catch (Exception exception)
            {
                return HandleException(config.id, exception, snsTopicArn);
            }

            return GetSuccessOutput(config.id);
        }

        private static void TryLoadStrFile(Stream stream, GenomeAssembly genomeAssembly, ISequenceProvider refProvider)
        {
            try
            {
                RepeatExpansionReader.Load(stream, genomeAssembly, refProvider.RefNameToChromosome,
                    refProvider.RefIndexToChromosome.Count);
            }
            catch (Exception exception)
            {
                throw new UserErrorException(exception.Message);
            }
        }

        private ValidationResult HandleException(string id, Exception exception, string snsTopicArn)
        {
            Logger.Log(exception);

            string snsMessage = SNS.CreateMessage(exception.Message, "exception", exception.StackTrace);
            SNS.SendMessage(snsTopicArn, snsMessage);

            ErrorCategory errorCategory = ExceptionUtilities.ExceptionToErrorCategory(exception);
            var errorMessagePrefix = errorCategory == ErrorCategory.UserError ? "User error" : "Nirvana error";
            return new ValidationResult
            {
                id = id,
                status = $"{errorMessagePrefix}: {exception.Message}"
            };
        }


        private static ValidationResult GetSuccessOutput(string id) =>
            new ValidationResult
            {
                id = id,
                status = LambdaUtilities.SuccessMessage
            };
    }
}