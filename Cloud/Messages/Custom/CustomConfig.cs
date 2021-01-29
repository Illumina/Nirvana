// ReSharper disable InconsistentNaming

namespace Cloud.Messages.Custom
{
    public sealed class CustomConfig
    {
        public string    id;
        public string    tsvUrl;
        public S3Path    outputDir;
        public JwtFields jwtFields;
        
        public bool skipGeneIdValidation;
    }
}