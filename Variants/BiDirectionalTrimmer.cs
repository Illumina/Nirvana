using System;

namespace Variants
{
    public static class BiDirectionalTrimmer
    {
        public static (int Start, string RefAllele, string AltAllele) Trim(int start, string refAllele,
            string altAllele)
        {
            // do not trim if ref and alt are same
            if (refAllele == altAllele) return (start, refAllele, altAllele);

            refAllele ??= "";
            altAllele ??= "";

            int refLen     = refAllele.Length;
            int altLen     = altAllele.Length;
            int origRefLen = refLen;

            ReadOnlySpan<char> refSpan = refAllele.AsSpan();
            ReadOnlySpan<char> altSpan = altAllele.AsSpan();

            // trimming at the start
            var offset = 0;
            while (offset < refLen && offset < altLen && refSpan[offset] == altSpan[offset]) offset++;

            if (offset > 0)
            {
                start   += offset;
                refSpan =  refSpan.Slice(offset);
                altSpan =  altSpan.Slice(offset);
                refLen  =  refSpan.Length;
                altLen  =  altSpan.Length;
            }

            // trimming at the end
            while (refLen > 0 && altLen > 0 && refSpan[refLen - 1] == altSpan[altLen - 1])
            {
                refLen--;
                altLen--;
            }

            // nothing to trim
            if (refLen == origRefLen) return (start, refAllele, altAllele);

            refAllele = new string(refSpan.Slice(0, refLen));
            altAllele = new string(altSpan.Slice(0, altLen));

            return (start, refAllele, altAllele);
        }
    }
}