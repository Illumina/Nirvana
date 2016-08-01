using System.Collections.Generic;
using VariantAnnotation.DataStructures;

namespace VariantAnnotation.Algorithms
{
    public class BiDirectionalTrimmer : IAlleleTrimmer
    {
        /// <summary>
        /// trims the prefix and suffix of the variant alleles (Nirvana style)
        /// </summary>
        public void Trim(List<VariantAlternateAllele> alternateAlleles)
        {
            foreach (var altAllele in alternateAlleles)
            {
                // there is nothing to trim
                if (altAllele.AlternateAllele.Length == 0 || altAllele.ReferenceAllele.Length == 0) continue;

                // handle symbolic alleles
                if (altAllele.IsSymbolicAllele)
                {
                    altAllele.ReferenceBegin++;
                    continue;
                }

                // trimming at the start
                int numPrefixBasesToTrim = 0;
                while (numPrefixBasesToTrim < altAllele.ReferenceAllele.Length && numPrefixBasesToTrim < altAllele.AlternateAllele.Length
                    && altAllele.ReferenceAllele[numPrefixBasesToTrim] == altAllele.AlternateAllele[numPrefixBasesToTrim])
                    numPrefixBasesToTrim++;

                if (numPrefixBasesToTrim > 0)
                {
                    // start is advanced if there are cancelled out bases
                    altAllele.ReferenceBegin += numPrefixBasesToTrim;
                    altAllele.AlternateAllele = altAllele.AlternateAllele.Substring(numPrefixBasesToTrim);
                    altAllele.ReferenceAllele = altAllele.ReferenceAllele.Substring(numPrefixBasesToTrim);
                }

                // trimming at the end
                int numSuffixBasesToTrim = 0;
                while (numSuffixBasesToTrim < altAllele.ReferenceAllele.Length
                    && numSuffixBasesToTrim < altAllele.AlternateAllele.Length
                    && altAllele.ReferenceAllele[altAllele.ReferenceAllele.Length - numSuffixBasesToTrim - 1] == altAllele.AlternateAllele[altAllele.AlternateAllele.Length - numSuffixBasesToTrim - 1])
                    numSuffixBasesToTrim++;

                if (numSuffixBasesToTrim <= 0) continue;

                altAllele.ReferenceEnd -= numSuffixBasesToTrim;
                altAllele.AlternateAllele = altAllele.AlternateAllele.Substring(0, altAllele.AlternateAllele.Length - numSuffixBasesToTrim);
                altAllele.ReferenceAllele = altAllele.ReferenceAllele.Substring(0, altAllele.ReferenceAllele.Length - numSuffixBasesToTrim);
            }
        }
    }
}
