// ReSharper disable InconsistentNaming

using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using IO;

namespace Cloud
{
    public class SaUrls
    {
        public string nsaUrl;
        public string idxUrl;
        public string nsiUrl;
        public string ngaUrl;

        public CustomSaType SaType => GetSaType();
        private CustomSaType _saType;

        public void Validate()
        {
            switch (SaType) {
                case CustomSaType.Nsa:
                    HttpUtilities.ValidateUrl(nsaUrl);
                    HttpUtilities.ValidateUrl(idxUrl);
                    break;
                case CustomSaType.Nsi:
                    HttpUtilities.ValidateUrl(nsiUrl);
                    break;
                case CustomSaType.Nga:
                    HttpUtilities.ValidateUrl(ngaUrl);
                    break;
                default:
                    throw new InvalidDataException("Unknown custom SA type.");
            }
        }

        internal CustomSaType GetSaType()
        {
            if (_saType != default) return _saType;

            bool[] checkSaTypes = {nsaUrl != null, nsiUrl != null, ngaUrl != null};
            var providedTypes = checkSaTypes.Select((x, i) => (Provided: x, SaTypeIndex: i + 1)).Where(y => y.Provided)
                .Select(y => (CustomSaType) y.SaTypeIndex).ToArray();

            if (providedTypes.Length == 0) throw new UserErrorException("No custom annotation file provided.");
            if (providedTypes.Length > 1)
                throw new UserErrorException(
                    $"Multiple types of annotation files found: {providedTypes.Select(x => x.ToString())}. Please just provide one type of custom annotation file(s)");

            if (providedTypes[0] == CustomSaType.Nsa && idxUrl == null)
                throw new UserErrorException($"Index file is not provided for the NSA file {nsaUrl}.");

            _saType = providedTypes[0];
            return _saType;
        }

        public override string ToString()
        {
            switch (SaType)
            {
                case CustomSaType.Nsa:
                    return $"{{\"nsaUrl\":\"{nsaUrl}\", \"idxUrl\":\"{idxUrl}\"}}";
                case CustomSaType.Nsi:
                    return $"{{\"nsiUrl\":\"{nsiUrl}\"}}";
                case CustomSaType.Nga:
                    return $"{{\"ngaUrl\":\"{ngaUrl}\"}}";
                default:
                    throw new InvalidDataException("Unknown custom SA type.");
            }
        }
    }

    public enum CustomSaType
    {
        Nsa = 1,
        Nsi,
        Nga
    }
}
