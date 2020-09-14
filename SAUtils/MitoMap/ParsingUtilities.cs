using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAUtils.MitoMap
{
    public static class ParsingUtilities
    {
        private const string EmptyString = "\\N";
        public static List<string> GetPubMedIds(string field, MitoMapInputDb mitoMapInputDb)
        {
            if (field == "0") return default;

            var internalIds = ExtractInternalIds(field);
            var pubMedIds = new List<string>();
            foreach (string internalId in internalIds)
            {
                if (mitoMapInputDb.InternalReferenceIdToPubmedId.TryGetValue(internalId, out string pubMedId))
                {
                    if (pubMedId != EmptyString) pubMedIds.Add(pubMedId);
                }
                else
                    throw new InvalidDataException($"Can't find PubMedID corresponding to internal reference ID {internalId} when parsing {field}");
            }

            return pubMedIds.Distinct().ToList();
        }

        public static string[] ExtractInternalIds(string field)
        {
            //"?refs=4,140,189,91687,91737&title="
            const string leadingString = "refs=";
            const string trailingString = "&title=";
            var leadingStringIndex = field.IndexOf(leadingString, StringComparison.Ordinal);
            var trailingStringIndex = field.IndexOf(trailingString, StringComparison.Ordinal);
            var startIndex = leadingStringIndex + leadingString.Length;
            var idStringLength = trailingStringIndex - startIndex;
            if (leadingStringIndex == -1 || trailingStringIndex == -1 || idStringLength == 0)
                throw new InvalidDataException($"Failed to extract reference IDs from {field}");

            return field.Substring(startIndex, idStringLength).Split(',');
        }
    }
}