using ErrorHandling.Exceptions;
using IO;

namespace Cloud.Messages.Gene
{
    public sealed class GeneConfig
    {
        // ReSharper disable InconsistentNaming
        public string id;
        public string[] geneSymbols;
        public string[] ngaUrls;
        // ReSharper restore InconsistentNaming

        public void Validate()
        {
            if (string.IsNullOrEmpty(id)) throw new UserErrorException("Please provide the id of the job.");
            if (geneSymbols == null || geneSymbols.Length == 0)
                throw new UserErrorException("Please provide at lease one gene symbol.");
            if (ngaUrls == null) return;

            foreach (string ngaUrl in ngaUrls) HttpUtilities.ValidateUrl(ngaUrl);
        }
    }
}