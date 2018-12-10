using System.Linq;

namespace SAUtils
{
    public static class SaUtilsCommon
    {
        public static bool IsNumberNullOrZero(int? item)
        {
            return item == null || item.Value == 0;
        }

        /// <summary>
        /// Returns a regular alternate allele when a provided with one have SA format.
        /// In case of long insertions or InsDel, where the saAltAllele contains an MD5 hash, the hash is returned.
        /// </summary>
        /// <param name="saAltAllele"> supplementary annotation alternate allele</param>
        /// <param name="emptyAllele">The way the calling function wants to represent an empty allele</param>
        /// <returns>regular alternate allele</returns>
        public static string ReverseSaReducedAllele(string saAltAllele, string emptyAllele = "-")
        {
            if (saAltAllele == null) return null;
            if (saAltAllele.All(char.IsDigit)) return emptyAllele; // this was a deletion

            int firstBaseIndex;
            for (firstBaseIndex = 0; firstBaseIndex < saAltAllele.Length; firstBaseIndex++)
            {
                if (saAltAllele[firstBaseIndex] != 'i' && saAltAllele[firstBaseIndex] != '<' &&
                    !char.IsDigit(saAltAllele[firstBaseIndex]))
                    break;
            }

            if (saAltAllele.Substring(firstBaseIndex) == "") return emptyAllele;

            return firstBaseIndex > 0 && firstBaseIndex < saAltAllele.Length
                ? saAltAllele.Substring(firstBaseIndex)
                : saAltAllele;
        }
    }
}
