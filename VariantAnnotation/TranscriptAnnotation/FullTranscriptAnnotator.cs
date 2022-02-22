using System;
using System.Collections.Generic;
using Cache.Data;
using Genome;
using Intervals;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FullTranscriptAnnotator
    {
        public static AnnotatedTranscript GetAnnotatedTranscript(Transcript transcript, IVariant leftShiftedVariant,
            ISequence refSequence, AminoAcid aminoAcids)
        {
            var rightShiftedVariant = VariantRotator.Right(leftShiftedVariant, transcript, refSequence,
                transcript.Gene.OnReverseStrand);

            string cdnaSequence = transcript.CdnaSeq;
            ReadOnlySpan<char> extendedCds =
                GetExtendedCodingSequence(cdnaSequence, transcript.CodingRegion);

            var leftAnnotation = AnnotateTranscript(transcript, leftShiftedVariant, aminoAcids, cdnaSequence, extendedCds);

            var rightAnnotation = ReferenceEquals(leftShiftedVariant, rightShiftedVariant)
                ? leftAnnotation
                : AnnotateTranscript(transcript, rightShiftedVariant, aminoAcids, cdnaSequence, extendedCds);

            List<ConsequenceTag> consequences = GetConsequences(transcript, leftShiftedVariant, leftAnnotation.VariantEffect);
            
            string refAllele = rightAnnotation.TranscriptRefAllele;
            string altAllele = rightAnnotation.TranscriptAltAllele;
            string hgvsCoding = HgvsCodingNomenclature.GetHgvscAnnotation(transcript, rightShiftedVariant, refSequence,
                    rightAnnotation.Position.RegionStartIndex, rightAnnotation.Position.RegionEndIndex, 
                    refAllele, altAllele);
            
            string hgvsProtein = HgvsProtein.GetHgvsProteinAnnotation(transcript.CodingRegion?.ProteinId,
                hgvsCoding, extendedCds, transcript.CodingRegion?.ProteinSeq, rightAnnotation.Position.CdsStart,
                rightAnnotation.Position.ExtendedCdsEnd, rightAnnotation.Position.ProteinStart, rightAnnotation.RefAminoAcids,
                rightAnnotation.AltAminoAcids, rightAnnotation.TranscriptAltAllele,
                leftShiftedVariant.Type == VariantType.reference, aminoAcids);

            return new AnnotatedTranscript(transcript, leftAnnotation.RefAminoAcids, leftAnnotation.AltAminoAcids,
                leftAnnotation.RefCodons, leftAnnotation.AltCodons, leftAnnotation.Position, hgvsCoding, hgvsProtein,
                consequences, null, false);
        }

        private static ReadOnlySpan<char> GetExtendedCodingSequence(string cdnaSequence, CodingRegion codingRegion)
        {
            ReadOnlySpan<char> cdnaSpan = cdnaSequence.AsSpan();

            return codingRegion == null
                ? cdnaSpan
                : cdnaSpan.Slice(codingRegion.CdnaStart - codingRegion.CdsOffset - 1);
        }

        private static (VariantEffect VariantEffect, IMappedPosition Position, string RefAminoAcids, string
            AltAminoAcids, string RefCodons, string AltCodons, string TranscriptAltAllele, string TranscriptRefAllele)
            AnnotateTranscript(Transcript transcript, ISimpleVariant variant, AminoAcid aminoAcids,
                string cdnaSequence, ReadOnlySpan<char> codingSequence)
        {
            bool onReverseStrand = transcript.Gene.OnReverseStrand;
            (int startIndex, TranscriptRegion startRegion) =
                MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.Start);
            (int endIndex, TranscriptRegion endRegion) =
                MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.End);

            var position = GetMappedPosition(transcript.TranscriptRegions, startRegion, startIndex, endRegion, endIndex,
                variant, onReverseStrand, transcript.CodingRegion, variant.Type == VariantType.insertion);

            string transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, onReverseStrand);

            (string referenceCodons, string alternateCodons) = Codons.GetCodons(transcriptAltAllele, position.CdsStart,
                position.ExtendedCdsEnd, position.ProteinStart, position.ExtendedProteinEnd, codingSequence);

            (string referenceAminoAcids, string alternateAminoAcids) = aminoAcids.Translate(referenceCodons,
                alternateCodons, transcript.CodingRegion?.AminoAcidEdits, position.ProteinStart);

            (referenceAminoAcids, alternateAminoAcids, position.ProteinStart, position.ProteinEnd) =
                TryTrimAminoAcidsAndUpdateProteinPositions(referenceAminoAcids, alternateAminoAcids,
                    position.ProteinStart, position.ProteinEnd);

            (position.CoveredCdnaStart, position.CoveredCdnaEnd) =
                transcript.TranscriptRegions.GetCoveredCdnaPositions(position.CdnaStart, startIndex, position.CdnaEnd,
                    endIndex, onReverseStrand);

            (position.CoveredCdsStart, position.CoveredCdsEnd, position.CoveredProteinStart,
                position.CoveredProteinEnd) = MappedPositionUtilities.GetCoveredCdsAndProteinPositions(
                position.CoveredCdnaStart, position.CoveredCdnaEnd, transcript.CodingRegion);

            string transcriptRefAllele = HgvsUtilities.AdjustTranscriptRefAllele(
                HgvsUtilities.GetTranscriptAllele(variant.RefAllele, onReverseStrand), position.CoveredCdnaStart,
                position.CoveredCdnaEnd, cdnaSequence);

            // only generate the covered version of ref & alt alleles when CDS start/end is -1
            string coveredRefAa;
            string coveredAltAa;

            if (position.CdsStart == -1 || position.CdsEnd == -1)
            {
                (string coveredRefCodon, string coveredAltCodon) = Codons.GetCodons(transcriptAltAllele,
                    position.CoveredCdsStart, position.ExtendedCdsEnd, position.CoveredProteinStart,
                    position.ExtendedProteinEnd, codingSequence);

                (coveredRefAa, coveredAltAa) = aminoAcids.Translate(coveredRefCodon, coveredAltCodon,
                    transcript.CodingRegion?.AminoAcidEdits, position.ProteinStart);

                (coveredRefAa, coveredAltAa, position.CoveredProteinStart, position.CoveredProteinEnd) =
                    TryTrimAminoAcidsAndUpdateProteinPositions(coveredRefAa, coveredAltAa, position.CoveredProteinStart,
                        position.CoveredProteinEnd);
            }
            else
            {
                coveredRefAa                 = referenceAminoAcids;
                coveredAltAa                 = alternateAminoAcids;
                position.CoveredProteinStart = position.ProteinStart;
                position.CoveredProteinEnd   = position.ProteinEnd;
            }

            var positionalEffect = GetPositionalEffect(transcript, variant, position, referenceAminoAcids,
                alternateAminoAcids, position.CoveredCdnaStart, position.CoveredCdnaEnd, position.CoveredCdsStart,
                position.CoveredCdsEnd);

            var variantEffect = new VariantEffect(positionalEffect, variant, transcript, referenceAminoAcids,
                alternateAminoAcids, referenceCodons, alternateCodons, position.ProteinStart, coveredRefAa,
                coveredAltAa);

            return (variantEffect, position, referenceAminoAcids, alternateAminoAcids, referenceCodons, alternateCodons,
                transcriptAltAllele, transcriptRefAllele);
        }

        internal static (string ReferenceAminoAcids, string AlternateAminoAcids, int ProteinStart, int ProteinEnd)
            TryTrimAminoAcidsAndUpdateProteinPositions(string referenceAminoAcids, string alternateAminoAcids, int proteinStart, int proteinEnd)
        {
            (int newStart, string newReference, string newAlternate) =
                BiDirectionalTrimmer.Trim(proteinStart, referenceAminoAcids, alternateAminoAcids);

            return string.IsNullOrEmpty(newReference)
                ? (referenceAminoAcids, alternateAminoAcids, proteinStart, proteinEnd)
                : (newReference, newAlternate, newStart, newStart + newReference.Length - 1);
        }

        private static IMappedPosition GetMappedPosition(TranscriptRegion[] regions, TranscriptRegion startRegion,
            int startIndex, TranscriptRegion endRegion, int endIndex, IInterval variant, bool onReverseStrand,
            CodingRegion codingRegion, bool isInsertion)
        {
            (int cdnaStart, int cdnaEnd) = isInsertion
                ? MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion, variant.Start, variant.End,
                    onReverseStrand)
                : MappedPositionUtilities.GetCdnaPositions(startRegion, endRegion, variant.Start, variant.End,
                    onReverseStrand);
            if (onReverseStrand) (cdnaStart, cdnaEnd) = (cdnaEnd, cdnaStart);

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, cdnaStart, cdnaEnd,
                isInsertion);

            int extendedCdsEnd = cdsStart != -1 && cdsEnd == -1
                ? MappedPositionUtilities.GetExtendedCdsPosition(codingRegion.CdnaStart, cdnaEnd, codingRegion.CdsOffset)
                : cdsEnd;

            int proteinStart       = MappedPositionUtilities.GetProteinPosition(cdsStart);
            int proteinEnd         = MappedPositionUtilities.GetProteinPosition(cdsEnd);
            int extendedProteinEnd = MappedPositionUtilities.GetProteinPosition(extendedCdsEnd);

            (int exonStart, int exonEnd, int intronStart, int intronEnd) =
                regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(cdnaStart, cdnaEnd, cdsStart, cdsEnd, extendedCdsEnd, proteinStart, proteinEnd, extendedProteinEnd, exonStart,
                exonEnd, intronStart, intronEnd, startIndex, endIndex);
        }

        private static TranscriptPositionalEffect GetPositionalEffect(Transcript transcript, ISimpleVariant variant,
            IMappedPosition position, string refAminoAcid, string altAminoAcid, int coveredCdnaStart,
            int coveredCdnaEnd, int coveredCdsStart, int coveredCdsEnd)
        {
            bool startCodonInsertionWithNoImpact = variant.Type == VariantType.insertion &&
                                                  position.ProteinStart <= 1 &&
                                                  altAminoAcid.EndsWith(refAminoAcid);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(transcript.TranscriptRegions, variant, variant.Type);
            positionalEffect.DetermineExonicEffect(transcript, variant, position, coveredCdnaStart, coveredCdnaEnd,
                coveredCdsStart, coveredCdsEnd, variant.AltAllele, startCodonInsertionWithNoImpact);
            return positionalEffect;
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static List<ConsequenceTag> GetConsequences(IInterval transcript, IVariant variant,
            IVariantEffect variantEffect)
        {
            var featureEffect = new FeatureVariantEffects(transcript, variant.Type, variant,
                variant.Behavior.StructuralVariantConsequence);

            var consequence = new Consequences(variantEffect, featureEffect);
            consequence.DetermineSmallVariantEffects();
            return consequence.GetConsequences();
        }
    }
}