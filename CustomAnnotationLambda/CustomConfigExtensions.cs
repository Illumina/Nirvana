using Cloud;
using Cloud.Messages.Custom;
using ErrorHandling.Exceptions;
using IO;

namespace CustomAnnotationLambda
{
    public static class CustomConfigExtensions
    {
        public static void CheckRequiredFieldsNotNull(this CustomConfig config)
        {
            string BuildErrorMessage(string message) => message + " cannot be null.";

            if (config.id                     == null) throw new UserErrorException(BuildErrorMessage("id"));
            if (config.tsvUrl                 == null) throw new UserErrorException(BuildErrorMessage("tsvUrl"));
            if (config.outputDir              == null) throw new UserErrorException(BuildErrorMessage("outputDir"));
            if (config.outputDir.bucketName   == null) throw new UserErrorException(BuildErrorMessage("bucketName of outputDir"));
            if (config.outputDir.path         == null) throw new UserErrorException(BuildErrorMessage("path of outputDir"));
            if (config.outputDir.region       == null) throw new UserErrorException(BuildErrorMessage("region of outputDir"));
            if (config.outputDir.accessKey    == null) throw new UserErrorException(BuildErrorMessage("accessKey of outputDir"));
            if (config.outputDir.secretKey    == null) throw new UserErrorException(BuildErrorMessage("secretKey of outputDir"));
            if (config.outputDir.sessionToken == null) throw new UserErrorException(BuildErrorMessage("sessionToken of outputDir"));
        }

        public static void CheckResourcesExist(this CustomConfig config)
        {
            HttpUtilities.ValidateUrl(config.tsvUrl);
            HttpUtilities.ValidateUrl(LambdaUrlHelper.GetUgaUrl(), false);
            config.outputDir.Validate(true);
        }
    }
}
