// ReSharper disable InconsistentNaming

using ErrorHandling.Exceptions;
using IO;

namespace Cloud
{
    public class SaUrls
    {
        public string nsaUrl;
        public string idxUrl;
        public string nsiUrl;

        public void Validate()
        {
            if (nsaUrl != null)
            {
                if (idxUrl == null) throw new UserErrorException($"Index file is not provided for the NSA file {nsaUrl}.");
                if (nsiUrl != null) throw new UserErrorException($"NSI {nsaUrl} file should not be provided when NSA file is provided.");

                HttpUtilities.ValidateUrl(nsaUrl);
                HttpUtilities.ValidateUrl(idxUrl);
                return;
            }
            if (idxUrl != null) throw new UserErrorException($"Index file {idxUrl} should not be provided when NSA file is not provided.");
            if (nsiUrl == null) throw new UserErrorException("No custom annotation file is provided.");

            HttpUtilities.ValidateUrl(nsiUrl);
        }

        public override string ToString()
        {
            return nsaUrl == null ? $"{{\"nsiUrl\":\"{nsiUrl}\"}}" : $"{{\"nsaUrl\":\"{nsaUrl}\", \"idxUrl\":\"{idxUrl}\"}}";
        }

        public bool IsNsa() => nsaUrl != null;
    }
}
