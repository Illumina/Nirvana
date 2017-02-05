using System;
using System.Collections.Generic;
using VariantAnnotation.DataStructures;

namespace VariantAnnotation.Algorithms
{
    public sealed class BiDirectionalTrimmer : IAlleleTrimmer
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
                    altAllele.Start++;
                    continue;
                }

                var trimmedAlleles = Trim(altAllele.Start, altAllele.ReferenceAllele, altAllele.AlternateAllele);

                altAllele.Start           = trimmedAlleles.Item1;
                altAllele.ReferenceAllele = trimmedAlleles.Item2;
                altAllele.AlternateAllele = trimmedAlleles.Item3;
                altAllele.End             = altAllele.Start + altAllele.ReferenceAllele.Length - 1;
            }
        }

        public static Tuple<int, string, string> Trim(int start, string refAllele, string altAllele)
        {
            // do not trim if ref and alt are same
            if (refAllele == altAllele) return new Tuple<int, string, string>(start, refAllele, altAllele);

            // trimming at the start
            var i = 0;
            while (i < refAllele.Length && i < altAllele.Length && refAllele[i] == altAllele[i]) i++;

            if (i > 0)
            {
                start += i;
                altAllele = altAllele.Substring(i);
                refAllele = refAllele.Substring(i);
            }

            // trimming at the end
            var j = 0;
            while (j < refAllele.Length && j < altAllele.Length &&
                   refAllele[refAllele.Length - j - 1] == altAllele[altAllele.Length - j - 1]) j++;

            if (j <= 0) return Tuple.Create(start, refAllele, altAllele);

            altAllele = altAllele.Substring(0, altAllele.Length - j);
            refAllele = refAllele.Substring(0, refAllele.Length - j);
            return Tuple.Create(start, refAllele, altAllele);
        }
    }
}
