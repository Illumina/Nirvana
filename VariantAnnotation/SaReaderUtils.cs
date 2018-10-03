using System.Globalization;
using System.Linq;

namespace VariantAnnotation
{
    public static class SaReaderUtils
    {
        public static string GetReducedAllele(string refAllele, string altAllele)
        {
            if (!NeedsReduction(refAllele, altAllele)) return altAllele;

            if (string.IsNullOrEmpty(altAllele))
                return refAllele.Length.ToString(CultureInfo.InvariantCulture);


            if (string.IsNullOrEmpty(refAllele))
                return 'i' + altAllele;

            if (refAllele.Length == altAllele.Length) return altAllele;

            // its a delins 
            return refAllele.Length.ToString(CultureInfo.InvariantCulture) + altAllele;

        }
        private static bool NeedsReduction(string refAllele, string altAllele)
        {
            if (string.IsNullOrEmpty(altAllele)) return true;
            if (!string.IsNullOrEmpty(refAllele) && altAllele.All(x => x == 'N')) return false;

            return !(altAllele[0] == 'i' || altAllele[0] == '<' || char.IsDigit(altAllele[0]));
        }
        
    }
}