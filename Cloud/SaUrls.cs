// ReSharper disable InconsistentNaming

using System.Text;
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
            if (nsiUrl == null) throw new UserErrorException("No SA file is provided.");

            HttpUtilities.ValidateUrl(nsiUrl);
        }

        public override string ToString()
        {
            return nsaUrl == null ? $"{{\"nsiUrl\":\"{nsiUrl}\"}}" : $"{{\"nsaUrl\":\"{nsaUrl}\", \"idxUrl\":\"{idxUrl}\"}}";
        }

        public (string DataFile, string IndexFile) ToDataAndIndexFiles() => nsaUrl == null ? (nsiUrl, null) : (nsaUrl, idxUrl);
    }
}