namespace Variants
{
    public static class BiDirectionalTrimmer
    {
        public static (int Start, string RefAllele, string AltAllele) Trim(int start, string refAllele, string altAllele)
        {
            // do not trim if ref and alt are same
            if (refAllele == altAllele) return (start, refAllele, altAllele);

            if (refAllele == null) refAllele = "";
            if (altAllele == null) altAllele = "";

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

            if (j <= 0) return (start, refAllele, altAllele);

            altAllele = altAllele.Substring(0, altAllele.Length - j);
            refAllele = refAllele.Substring(0, refAllele.Length - j);
            return (start, refAllele, altAllele);
        }
    }
}