using System;
using System.Buffers;
using Cache.Data;

namespace VariantAnnotation.AnnotatedPositions.AminoAcids
{
    public sealed class AminoAcid
    {
        private readonly AminoAcidEntry[] _aminoAcidEntries;
        private readonly int              _maxIndex;

        internal AminoAcid(AminoAcidEntry[] aminoAcidEntries)
        {
            _aminoAcidEntries = aminoAcidEntries;
            _maxIndex         = aminoAcidEntries.Length - 1;
        }

        private static readonly (string, string) EmptyTuple = (string.Empty, string.Empty);

        public (string ReferenceAminoAcids, string AlternateAminoAcids) Translate(string referenceCodons,
            string alternateCodons, AminoAcidEdit[] aaEdits, int aaStart)
        {
            if (string.IsNullOrEmpty(referenceCodons) && string.IsNullOrEmpty(alternateCodons)) return EmptyTuple;
            if (referenceCodons != null && (referenceCodons.Contains("N") || alternateCodons.Contains("N")))
                return EmptyTuple;

            string referenceAminoAcids = TranslateBases(referenceCodons, aaEdits, aaStart, false);
            string alternateAminoAcids = TranslateBases(alternateCodons, null,    aaStart, false);
            return (referenceAminoAcids, alternateAminoAcids);
        }

        public string TranslateBases(string bases, AminoAcidEdit[] aaEdits, int aaStart, bool ignoreIncompleteCodons)
        {
            if (bases == null) return null;

            int numBases      = bases.Length;
            int numAminoAcids = bases.Length / 3;

            ReadOnlySpan<char> cdsSpan  = bases.AsSpan();
            ArrayPool<char>    charPool = ArrayPool<char>.Shared;

            // convert the bases to uppercase
            char[]     upperCds     = charPool.Rent(numBases);
            Span<char> upperCdsSpan = upperCds.AsSpan().Slice(0, numBases);
            cdsSpan.ToUpperInvariant(upperCdsSpan);

            // create output buffer
            bool addX       = !ignoreIncompleteCodons && numAminoAcids * 3 != numBases;
            int  bufferSize = addX ? numAminoAcids + 1 : numAminoAcids;

            char[]     buffer = charPool.Rent(bufferSize);
            Span<char> aaSpan = buffer.AsSpan().Slice(0, bufferSize);

            // convert codons to amino acids
            var offset = 0;
            for (var i = 0; i < numAminoAcids; i++, offset += 3)
            {
                ReadOnlySpan<char> span      = upperCdsSpan.Slice(offset, 3);
                int                triplet   = (span[0] << 16) | (span[1] << 8) | span[2];
                char               aminoAcid = BinarySearch(triplet);
                aaSpan[i] = aminoAcid;

                if (aminoAcid != '*') continue;

                aaSpan = aaSpan.Slice(0, i + 1);
                addX   = false;
                break;
            }

            if (addX) aaSpan[numAminoAcids] = 'X';

            if (aaEdits != null) ApplyAminoAcidEdits(aaSpan, aaEdits, aaStart);
            var aaString = aaSpan.ToString();

            charPool.Return(upperCds);
            charPool.Return(buffer);

            return aaString;
        }

        // this is only used for alt alleles and therefore no need to support AA edits
        public string TranslateBases(ReadOnlySpan<char> cdsSpan)
        {
            int numBases      = cdsSpan.Length;
            int numAminoAcids = cdsSpan.Length / 3;

            ArrayPool<char> charPool = ArrayPool<char>.Shared;

            // convert the bases to uppercase
            char[]     upperCds     = charPool.Rent(numBases);
            Span<char> upperCdsSpan = upperCds.AsSpan().Slice(0, numBases);
            cdsSpan.ToUpperInvariant(upperCdsSpan);

            // create output buffer
            int bufferSize = numAminoAcids;

            char[]     buffer = charPool.Rent(bufferSize);
            Span<char> aaSpan = buffer.AsSpan().Slice(0, bufferSize);

            // convert codons to amino acids
            var offset = 0;
            for (var i = 0; i < numAminoAcids; i++, offset += 3)
            {
                ReadOnlySpan<char> span    = upperCdsSpan.Slice(offset, 3);
                int                triplet = (span[0] << 16) | (span[1] << 8) | span[2];
                aaSpan[i] = BinarySearch(triplet);
            }

            var aaString = new string(aaSpan);

            charPool.Return(upperCds);
            charPool.Return(buffer);

            return aaString;
        }

        private static void ApplyAminoAcidEdits(Span<char> aaSpan, AminoAcidEdit[] aaEdits, int aaStart)
        {
            int aaEnd = aaStart + aaSpan.Length - 1;

            foreach (var aaEdit in aaEdits)
            {
                if (aaEdit.Position > aaEnd) break;
                if (aaEdit.Position < aaStart) continue;
                aaSpan[aaEdit.Position - aaStart] = aaEdit.AminoAcid;
            }
        }

        private char BinarySearch(int triplet)
        {
            var begin = 0;
            int end   = _maxIndex;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);
                var entry = _aminoAcidEntries[index];

                if (entry.Triplet == triplet) return entry.AminoAcid;
                if (entry.Triplet < triplet) begin = index + 1;
                else end                           = index - 1;
            }

            return 'X';
        }
    }
}