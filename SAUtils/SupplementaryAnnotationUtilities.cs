using System.Globalization;
using System.Linq;
using CommonUtilities;

namespace SAUtils
{
    public static class SupplementaryAnnotationUtilities
    {
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

        public static bool ValidateRefAllele(string refAllele, string refBases)
        {
            if (refBases == null) return true;
            if (refAllele == ".") return true; //ref base is unknown
            if (refBases.All(x => x == 'N')) return true;

            return refAllele.Length < refBases.Length ? refBases.StartsWith(refAllele) : refAllele.StartsWith(refBases);

            // in rare cases the refAllele will be too large for our refBases string that is limited in length
        }

        public static (int Start, string RefAllele, string AltAllele) GetReducedAlleles(int start, string refAllele, string altAllele)
        {
            // we have a deletion
            if (refAllele == "-") refAllele = "";
            if (altAllele == "-") altAllele = "";
            if (!NeedsReduction(refAllele, altAllele))
                return (start, refAllele, altAllele);

            var trimmedTuple = BiDirectionalTrimmer.Trim(start, refAllele, altAllele);

            start = trimmedTuple.Start;
            refAllele = trimmedTuple.RefAllele;
            altAllele = trimmedTuple.AltAllele;

            // we have detected a deletion after trimming
            if (string.IsNullOrEmpty(altAllele))
                return (start, refAllele, refAllele.Length.ToString(CultureInfo.InvariantCulture));

            // we have an insertion and we indicate that with an i at the beginning
            if (string.IsNullOrEmpty(refAllele))
                return (start, refAllele, 'i' + altAllele);

            if (refAllele.Length == altAllele.Length) //SNV or CNV
                return (start, refAllele, altAllele);

            // its a delins 
            altAllele = refAllele.Length.ToString(CultureInfo.InvariantCulture) + altAllele;

            return (start, refAllele, altAllele);
        }

        private static bool NeedsReduction(string refAllele, string altAllele)
        {
            if (string.IsNullOrEmpty(altAllele)) return true;

            if (!string.IsNullOrEmpty(refAllele) && altAllele.All(x => x == 'N')) return false;

            return !(altAllele[0] == 'i' || altAllele[0] == '<' || char.IsDigit(altAllele[0]));
        }

        public static string ConvertToVcfInfoString(string s)
        {
            //characters such as comma, space, etc. are not allowed in vcfinfo strings.
            s = s.Replace(" ", "_");
            return s.Replace(",", "\\x2c");
        }

    }
}
