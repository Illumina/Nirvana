using System;
using System.Buffers;
using Genome;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsProtein
    {
        private const char AnyAminoAcid = 'X';
        
        // the extended CDS sequence starts where the normal CDS sequence starts, but continues until the end of the
        // cDNA sequence
        public static string GetHgvsProteinAnnotation(string proteinId, string hgvsCoding,
            ReadOnlySpan<char> extendedCdsSequence, string aaSequence, int cdsBegin, int cdsEnd, int aaBegin,
            string refAminoAcids, string altAminoAcids, string altAllele, bool isReference, AminoAcid aminoAcid)
        {
            if (SkipHgvsProtein(isReference, cdsBegin, cdsEnd, hgvsCoding, altAllele)) return null;

            int  aaEnd,        refAlleleLen, altAlleleLen;
            char refAminoAcid, altAminoAcid;

            (aaBegin, aaEnd, refAminoAcids, altAminoAcids, refAlleleLen, altAlleleLen, refAminoAcid, altAminoAcid) =
                NormalizeAminoAcids(aaBegin, refAminoAcids, altAminoAcids, aaSequence);

            bool hasFrameshift   = HasFrameshift(cdsBegin, cdsEnd, altAllele.Length);
            var  proteinCategory = GetProteinCategory(aaBegin, refAminoAcids, altAminoAcids, aaSequence, hasFrameshift);
            
            // convert these to substitutions
            if ((proteinCategory == ProteinCategory.Insertion || proteinCategory == ProteinCategory.Duplication ||
                proteinCategory == ProteinCategory.DeletionInsertion) && altAminoAcid == AminoAcidCommon.StopCodon)
            {
                if (proteinCategory != ProteinCategory.DeletionInsertion) refAminoAcid = aaSequence[aaEnd];
                proteinCategory = ProteinCategory.Substitution;
            }

            bool insertionBeforeTranscript = proteinCategory == ProteinCategory.Insertion && aaBegin == 1;

            if (proteinCategory == ProteinCategory.StartLost)
            {
                return UseStartLostNotation(proteinId, refAminoAcid, aaBegin);
            }

            if (refAminoAcids == altAminoAcids || insertionBeforeTranscript)
            {
                return UseSilentNotation(hgvsCoding, refAminoAcid, aaBegin);
            }

            if (proteinCategory == ProteinCategory.Substitution)
            {
                return UseSubstitutionNotation(proteinId, refAminoAcid, aaBegin, altAminoAcid);
            }

            if (proteinCategory == ProteinCategory.Deletion)
            {
                return UseDeletionNotation(proteinId, refAminoAcid, aaBegin, refAminoAcids[refAlleleLen - 1], aaEnd);
            }

            if (proteinCategory == ProteinCategory.Insertion)
            {
                int beforePosition = aaEnd;
                int afterPosition  = aaBegin;
                return UseInsertionNotation(proteinId, aaSequence[beforePosition - 1], beforePosition,
                    aaSequence[afterPosition - 1],     afterPosition,                  altAminoAcids);
            }

            if (proteinCategory == ProteinCategory.Duplication)
            {
                int firstPosition = aaBegin - altAlleleLen;
                return UseDuplicationNotation(proteinId, altAminoAcid, firstPosition, altAminoAcids[altAlleleLen - 1],
                    aaEnd);
            }

            if (proteinCategory == ProteinCategory.DeletionInsertion)
            {
                return UseDeletionInsertionNotation(proteinId, refAminoAcids[0], aaBegin,
                    refAminoAcids[refAlleleLen - 1],           aaEnd,            altAminoAcids);
            }
            
            // when dealing with frameshifts and extensions, we need to create an alternate AA sequence and find the
            // first difference between the ref and alt AA sequences
            string altAaSequence = GetAltPeptideSequence(extendedCdsSequence, cdsBegin, cdsEnd, altAllele, aminoAcid);

            (aaBegin, refAminoAcid, altAminoAcid) = FindFirstChangeAfterFrameshift(aaBegin, aaSequence, altAaSequence);

            if (altAminoAcid == AminoAcidCommon.StopCodon)
            {
                return refAminoAcid == AminoAcidCommon.StopCodon
                    ? UseSilentNotation(hgvsCoding, refAminoAcid, aaBegin)
                    : UseSubstitutionNotation(proteinId, refAminoAcid, aaBegin, altAminoAcid);
            }
            
            if (refAminoAcid == AminoAcidCommon.StopCodon && altAminoAcid != AminoAcidCommon.StopCodon)
                proteinCategory = ProteinCategory.Extension;

            int? newTerPosition;

            if (proteinCategory == ProteinCategory.Extension)
            {
                newTerPosition = CountAminoAcidsUntilNextStopCodon(altAaSequence, aaBegin);
                return UseExtensionNotation(proteinId, aaBegin, altAminoAcid, newTerPosition);
            }
            
            newTerPosition = CountAminoAcidsUntilNextStopCodon(altAaSequence, aaBegin - 1);
            return UseFrameshiftNotation(proteinId, refAminoAcid, aaBegin, altAminoAcid, newTerPosition);
        }

        internal static (int aaBegin, char refAminoAcid, char altAminoAcid) FindFirstChangeAfterFrameshift(int aaBegin,
            string aaSequence, string altAaSequence)
        {
            char refAminoAcid = aaBegin < aaSequence.Length ? aaSequence[aaBegin - 1] : 'X';
            char altAminoAcid = aaBegin < altAaSequence.Length ? altAaSequence[aaBegin - 1] : 'X';

            int maxPosition = Math.Min(aaSequence.Length, altAaSequence.Length);

            while (aaBegin <= maxPosition)
            {
                refAminoAcid = aaSequence[aaBegin - 1];
                altAminoAcid = altAaSequence[aaBegin - 1];
                if (refAminoAcid == AminoAcidCommon.StopCodon && altAminoAcid == AminoAcidCommon.StopCodon ||
                    refAminoAcid != altAminoAcid) break;
                aaBegin++;
            }

            return (aaBegin, refAminoAcid, altAminoAcid);
        }

        // https://varnomen.hgvs.org/recommendations/protein/variant/extension/
        // both N-terminal & C-terminal are defined, but only C-terminal extensions are implemented
        private static string UseExtensionNotation(string proteinId, int position, char altAminoAcid,
            int? newTerPosition)
        {
            string altAbbreviation        = AminoAcidAbbreviation.GetThreeLetterAbbreviation(altAminoAcid);
            string terminalPositionSuffix = GetTerminalPositionSuffix(newTerPosition);
            return $"{proteinId}:p.(Ter{position}{altAbbreviation}extTer{terminalPositionSuffix})";
        }

        // https://varnomen.hgvs.org/recommendations/protein/variant/frameshift/
        private static string UseFrameshiftNotation(string proteinId, char refAminoAcid, int position,
            char altAminoAcid, int? newTerPosition)
        {
            string refAbbreviation        = AminoAcidAbbreviation.GetThreeLetterAbbreviation(refAminoAcid);
            string altAbbreviation        = AminoAcidAbbreviation.GetThreeLetterAbbreviation(altAminoAcid);
            string terminalPositionSuffix = GetTerminalPositionSuffix(newTerPosition);
            return $"{proteinId}:p.({refAbbreviation}{position}{altAbbreviation}fsTer{terminalPositionSuffix})";
        }

        private static string GetTerminalPositionSuffix(int? newTerPosition) => newTerPosition switch
        {
            0    => "",
            null => "?",
            _    => newTerPosition.Value.ToString()
        };

        // https://varnomen.hgvs.org/recommendations/protein/variant/delins/
        private static string UseDeletionInsertionNotation(string proteinId, char firstAminoAcid, int firstPosition,
            char lastAminoAcid, int lastPosition, string insertedAminoAcids)
        {
            string firstAbbreviation     = AminoAcidAbbreviation.GetThreeLetterAbbreviation(firstAminoAcid);
            string lastAbbreviation      = AminoAcidAbbreviation.GetThreeLetterAbbreviation(lastAminoAcid);
            string insertedAbbreviations = AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations(insertedAminoAcids);

            return firstPosition == lastPosition
                ? $"{proteinId}:p.({firstAbbreviation}{firstPosition}delins{insertedAbbreviations})"
                : $"{proteinId}:p.({firstAbbreviation}{firstPosition}_{lastAbbreviation}{lastPosition}delins{insertedAbbreviations})";
        }

        // https://varnomen.hgvs.org/recommendations/protein/variant/insertion/
        private static string UseInsertionNotation(string proteinId, char beforeAminoAcid, int beforePosition,
            char afterAminoAcid, int afterPosition, string insertedAminoAcids)
        {
            string beforeAbbreviation    = AminoAcidAbbreviation.GetThreeLetterAbbreviation(beforeAminoAcid);
            string afterAbbreviation     = AminoAcidAbbreviation.GetThreeLetterAbbreviation(afterAminoAcid);
            string insertedAbbreviations = AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations(insertedAminoAcids);

            return
                $"{proteinId}:p.({beforeAbbreviation}{beforePosition}_{afterAbbreviation}{afterPosition}ins{insertedAbbreviations})";
        }

        // https://varnomen.hgvs.org/recommendations/protein/variant/duplication/
        private static string UseDuplicationNotation(string proteinId, char firstAminoAcid, int firstPosition,
            char lastAminoAcid, int lastPosition)
        {
            string firstAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(firstAminoAcid);
            string lastAbbreviation  = AminoAcidAbbreviation.GetThreeLetterAbbreviation(lastAminoAcid);
            return firstPosition == lastPosition
                ? $"{proteinId}:p.({firstAbbreviation}{firstPosition}dup)"
                : $"{proteinId}:p.({firstAbbreviation}{firstPosition}_{lastAbbreviation}{lastPosition}dup)";
        }

        // https://varnomen.hgvs.org/recommendations/protein/variant/deletion/
        private static string UseDeletionNotation(string proteinId, char firstAminoAcid, int firstPosition,
            char lastAminoAcid, int lastPosition)
        {
            string firstAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(firstAminoAcid);
            string lastAbbreviation  = AminoAcidAbbreviation.GetThreeLetterAbbreviation(lastAminoAcid);

            return firstPosition == lastPosition
                ? $"{proteinId}:p.({firstAbbreviation}{firstPosition}del)"
                : $"{proteinId}:p.({firstAbbreviation}{firstPosition}_{lastAbbreviation}{lastPosition}del)";
        }

        // https://varnomen.hgvs.org/recommendations/protein/variant/substitution/
        private static string UseSubstitutionNotation(string proteinId, char refAminoAcid, int position,
            char altAminoAcid)
        {
            string refAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(refAminoAcid);
            string altAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(altAminoAcid);
            return $"{proteinId}:p.({refAbbreviation}{position}{altAbbreviation})";
        }

        // specialized version of substitution
        private static string UseSilentNotation(string hgvsCoding, char refAminoAcid, int position)
        {
            string refAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(refAminoAcid);
            return $"{hgvsCoding}(p.({refAbbreviation}{position}=))";
        }
        
        private static string UseStartLostNotation(string proteinId, char refAminoAcid, int position)
        {
            string refAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(refAminoAcid);
            return $"{proteinId}:p.{refAbbreviation}{position}?";
        }

        internal static ProteinCategory GetProteinCategory(int aaBegin, string refAminoAcids, string altAminoAcids,
            string aaSequence, bool hasFrameshift)
        {
            int refLength = refAminoAcids.Length;
            int altLength = altAminoAcids.Length;
            
            bool isInsertion = refLength == 0 && altLength != 0;
            bool isDeletion  = refLength != 0 && altLength == 0;

            bool truncatedByStop = IsTruncatedByStop(refAminoAcids, altAminoAcids);
            bool startLost       = IsStartLost(aaBegin, refAminoAcids, altAminoAcids);

            if (startLost) return ProteinCategory.StartLost;
            
            if (refAminoAcids.Contains(AminoAcidCommon.StopCodon) && !altAminoAcids.Contains(AminoAcidCommon.StopCodon))
                return ProteinCategory.Extension;

            if (hasFrameshift && !truncatedByStop) return ProteinCategory.Frameshift;
            if (refLength == 1 && altLength == 1) return ProteinCategory.Substitution;

            if (isInsertion)
            {
                return IsDuplicate(aaBegin, altAminoAcids, aaSequence)
                    ? ProteinCategory.Duplication
                    : ProteinCategory.Insertion;
            }

            return isDeletion ? ProteinCategory.Deletion : ProteinCategory.DeletionInsertion;
        }

        internal static bool IsStartLost(int aaBegin, string refAminoAcids, string altAminoAcids)
        {
            if (aaBegin != 1) return false;

            // handle most SNVs/MNVs
            if (refAminoAcids.Length > 0 && altAminoAcids.Length > 0) return refAminoAcids[0] != altAminoAcids[0];

            // TODO: we might need to reconstruct the alt AA sequence to see what happens here
            
            return false;
        }

        internal static bool IsTruncatedByStop(string refAminoAcids, string altAminoAcids)
        {
            if (altAminoAcids == "") return false;

            int stopPosition = altAminoAcids.IndexOf(AminoAcidCommon.StopCodon);
            if (stopPosition == -1) return false;

            if (altAminoAcids[0] == AminoAcidCommon.StopCodon) return true;

            ReadOnlySpan<char> refSpan = refAminoAcids.AsSpan();
            ReadOnlySpan<char> altSpan = altAminoAcids.AsSpan().Slice(0, stopPosition);
            return refSpan.StartsWith(altSpan);
        }

        internal static bool IsDuplicate(int start, string altAminoAcids, string aaSequence)
        {
            ReadOnlySpan<char> aaSpan = aaSequence.AsSpan();

            int altLen       = altAminoAcids.Length;
            int testPosition = start - altLen - 1;
            if (testPosition < 0) return false;

            ReadOnlySpan<char> precedingSpan = aaSpan.Slice(testPosition, altLen);
            return precedingSpan.Equals(altAminoAcids, StringComparison.Ordinal);
        }

        private static bool SkipHgvsProtein(bool isReference, int cdsBegin, int cdsEnd, string hgvsCoding,
            string altAllele)
        {
            return isReference || string.IsNullOrEmpty(hgvsCoding) || cdsBegin == -1 || cdsEnd == -1 ||
                SequenceUtilities.HasNonCanonicalBase(altAllele);
        }

        private static (int aaBegin, int aaEnd, string refAminoAcids, string altAminoAcids, int refLength, int altLength
            , char refAminoAcid, char altAminoAcid) NormalizeAminoAcids(int aaBegin, string refAminoAcids,
                string altAminoAcids, string aaSequence)
        {
            refAminoAcids = RemoveAminoAcidsAfterStopCodon(refAminoAcids);
            altAminoAcids = RemoveAminoAcidsAfterStopCodon(altAminoAcids);
            
            (aaBegin, refAminoAcids, altAminoAcids) = BiDirectionalTrimmer.Trim(aaBegin, refAminoAcids, altAminoAcids);

            int  refLength   = refAminoAcids.Length;
            int  altLength   = altAminoAcids.Length;
            bool isInsertion = refLength == 0 && altLength != 0;
            bool isDeletion  = refLength != 0 && altLength == 0;

            if (isInsertion || isDeletion)
            {
                (aaBegin, refAminoAcids, altAminoAcids) =
                    Rotate3Prime(refAminoAcids, altAminoAcids, aaBegin, aaSequence, isInsertion);
            }

            int aaEnd = aaBegin + refAminoAcids.Length - 1;
            if (aaEnd >= aaSequence.Length) aaEnd = aaSequence.Length - 1;

            char refAminoAcid = refLength > 0 ? refAminoAcids[0] : AnyAminoAcid;
            char altAminoAcid = altLength > 0 ? altAminoAcids[0] : AnyAminoAcid;

            return (aaBegin, aaEnd, refAminoAcids, altAminoAcids, refLength, altLength, refAminoAcid, altAminoAcid);
        }

        private static string RemoveAminoAcidsAfterStopCodon(string aminoAcids)
        {
            int stopPosition = aminoAcids.IndexOf(AminoAcidCommon.StopCodon);
            return stopPosition == -1 ? aminoAcids : aminoAcids.Substring(0, stopPosition + 1);
        }

        internal static string GetAltPeptideSequence(ReadOnlySpan<char> cdsSpan, int cdsBegin, int cdsEnd,
            string altAllele, AminoAcid aminoAcid)
        {
            ArrayPool<char>    charPool   = ArrayPool<char>.Shared;
            ReadOnlySpan<char> beforeSpan = cdsSpan.Slice(0, cdsBegin - 1);
            ReadOnlySpan<char> afterSpan  = cdsSpan.Slice(cdsEnd);

            ReadOnlySpan<char> altAlleleSpan = altAllele.AsSpan();
            int                altAlleleLen  = altAllele.Length;

            int        bufferLen  = beforeSpan.Length + altAlleleLen + afterSpan.Length;
            char[]     buffer     = charPool.Rent(bufferLen);
            Span<char> bufferSpan = buffer.AsSpan();

            // build our CDS sequence
            beforeSpan.CopyTo(bufferSpan);
            bufferSpan = bufferSpan.Slice(beforeSpan.Length);
            altAlleleSpan.CopyTo(bufferSpan);
            bufferSpan = bufferSpan.Slice(altAlleleLen);
            afterSpan.CopyTo(bufferSpan);

            bufferSpan = buffer.AsSpan().Slice(0,bufferLen);
            string aaSequence = aminoAcid.TranslateBases(bufferSpan);
            charPool.Return(buffer);

            return aaSequence;
        }

        // returns null if there are no stop codons
        public static int? CountAminoAcidsUntilNextStopCodon(string aaSequence, int aaBegin)
        {
            ReadOnlySpan<char> aaSpan       = aaSequence.AsSpan().Slice(aaBegin - 1);
            int                termCodonPos = aaSpan.IndexOf(AminoAcidCommon.StopCodon);
            return termCodonPos == -1 ? null : termCodonPos;
        }

        // according to https://varnomen.hgvs.org/recommendations/checklist/#:~:text=The%203'%20rule, this should be
        // applied to deletions, duplications, and insertions
        internal static (int Start, string RefAminoAcids, string AltAminoAcids) Rotate3Prime(string refAminoAcids,
            string altAminoAcids, int start, string peptides, bool isInsertion)
        {
            string             aminoAcids     = isInsertion ? altAminoAcids : refAminoAcids;
            ReadOnlySpan<char> aminoAcidsSpan = aminoAcids.AsSpan();
            int                alleleLen      = aminoAcids.Length;
            int                end            = start + refAminoAcids.Length - 1;

            ArrayPool<char>    charPool    = ArrayPool<char>.Shared;
            ReadOnlySpan<char> peptideSpan = end >= peptides.Length ? null : peptides.AsSpan().Slice(end);

            int        bufferLen  = alleleLen + peptideSpan.Length;
            char[]     buffer     = charPool.Rent(bufferLen);
            Span<char> bufferSpan = buffer.AsSpan();

            aminoAcidsSpan.CopyTo(bufferSpan);
            peptideSpan.CopyTo(bufferSpan.Slice(alleleLen));

            var shiftStart = 0;
            int shiftEnd   = alleleLen;

            for (; shiftEnd < bufferLen; shiftStart++, shiftEnd++)
            {
                if (bufferSpan[shiftStart] != bufferSpan[shiftEnd]) break;
            }

            if (shiftStart == 0)
            {
                charPool.Return(buffer);
                return (start, refAminoAcids, altAminoAcids);
            }

            aminoAcids =  new string(bufferSpan.Slice(shiftStart, alleleLen));
            start      += shiftStart;
            charPool.Return(buffer);

            if (isInsertion) altAminoAcids = aminoAcids;
            else refAminoAcids             = aminoAcids;

            return (start, refAminoAcids, altAminoAcids);
        }
        
        private static bool HasFrameshift(int cdsBegin, int cdsEnd, int altAlleleLength)
        {
            int refAlleleLen = cdsEnd - cdsBegin + 1;
            return !Codons.IsTriplet(altAlleleLength - refAlleleLen);
        }

        // https://varnomen.hgvs.org/recommendations/protein/
        internal enum ProteinCategory
        {
            Substitution,
            Deletion,
            Duplication,
            Insertion,
            DeletionInsertion,
            Frameshift,
            Extension,
            StartLost
        }
    }
}