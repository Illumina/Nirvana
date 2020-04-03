using IO;

namespace Cloud.Messages.StrValidation
{
    public sealed class ValidationConfig
    {
        // ReSharper disable InconsistentNaming
        public string id;
        public string genomeAssembly;
        public string customStrUrl;
        // ReSharper restore InconsistentNaming

        public void Validate() => HttpUtilities.ValidateUrl(customStrUrl);
    }
}