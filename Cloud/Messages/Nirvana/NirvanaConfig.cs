using System.Collections.Generic;
using ErrorHandling.Exceptions;
using IO;

namespace Cloud.Messages.Nirvana
{
    public sealed class NirvanaConfig
    {
        // ReSharper disable InconsistentNaming
        public string id;
        public string genomeAssembly;
        public string vcfUrl;
        public string tabixUrl;
        public S3Path outputDir;
        public string supplementaryAnnotations;
        // ReSharper disable once UnassignedField.Global
        public List<SaUrls> customAnnotations;
        // ReSharper restore InconsistentNaming

        internal void CheckRequiredFieldsNotNull()
        {
            static string BuildErrorMessage(string message) => message + " cannot be null.";

            if (id                       == null) throw new UserErrorException(BuildErrorMessage("id"));
            if (genomeAssembly           == null) throw new UserErrorException(BuildErrorMessage("genomeAssembly"));
            if (vcfUrl                   == null) throw new UserErrorException(BuildErrorMessage("vcfUrl"));
            if (tabixUrl                 == null) throw new UserErrorException(BuildErrorMessage("tabixUrl"));
            if (outputDir                == null) throw new UserErrorException(BuildErrorMessage("outputDir"));
            if (outputDir.bucketName     == null) throw new UserErrorException(BuildErrorMessage("bucketName of outputDir"));
            if (outputDir.region         == null) throw new UserErrorException(BuildErrorMessage("region of outputDir"));
            if (outputDir.path           == null) throw new UserErrorException(BuildErrorMessage("path of outputDir"));
            if (outputDir.accessKey      == null) throw new UserErrorException(BuildErrorMessage("accessKey of outputDir"));
            if (outputDir.secretKey      == null) throw new UserErrorException(BuildErrorMessage("secretKey of outputDir"));
            if (outputDir.sessionToken   == null) throw new UserErrorException(BuildErrorMessage("sessionToken of outputDir"));
            if (supplementaryAnnotations == null) throw new UserErrorException(BuildErrorMessage("supplementaryAnnotations"));
        }

        public void Validate()
        {
            CheckRequiredFieldsNotNull();

            HttpUtilities.ValidateUrl(vcfUrl);
            HttpUtilities.ValidateUrl(tabixUrl);
            outputDir.Validate(true);

            customAnnotations?.ForEach(x => x.Validate());
        }
    }
}