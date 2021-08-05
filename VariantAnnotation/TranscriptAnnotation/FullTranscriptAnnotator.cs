using System;
using System.Collections.Generic;
using Genome;
using Intervals;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FullTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant leftShiftedVariant,
            ISequence refSequence, IPredictionCache siftCache, IPredictionCache polyphenCache, AminoAcid aminoAcids)
        {
            var rightShiftedVariant = VariantRotator.Right(leftShiftedVariant, transcript, refSequence,
                transcript.Gene.OnReverseStrand);

            var cdnaSequence = GetCdnaSequence(transcript, refSequence);
            ReadOnlySpan<char> extendedCds =
                GetExtendedCodingSequence(cdnaSequence, transcript.Translation?.CodingRegion);

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
            
            string hgvsProtein = HgvsProtein.GetHgvsProteinAnnotation(transcript.Translation?.ProteinId.WithVersion,
                hgvsCoding, extendedCds, transcript.Translation?.PeptideSeq, rightAnnotation.Position.CdsStart,
                rightAnnotation.Position.ExtendedCdsEnd, rightAnnotation.Position.ProteinStart, rightAnnotation.RefAminoAcids,
                rightAnnotation.AltAminoAcids, rightAnnotation.TranscriptAltAllele,
                leftShiftedVariant.Type == VariantType.reference, aminoAcids);
            
            // string hgvsProtein = HgvsProteinNomenclature.GetHgvsProteinAnnotation(transcript,
            //     rightAnnotation.RefAminoAcids, rightAnnotation.AltAminoAcids, rightAnnotation.TranscriptAltAllele,
            //     rightAnnotation.Position, rightAnnotation.VariantEffect, rightShiftedVariant, refSequence, hgvsCoding,
            //     aminoAcids);

            (PredictionScore sift, PredictionScore polyPhen) = GetPredictionScores(leftAnnotation.Position, leftAnnotation.RefAminoAcids,
                leftAnnotation.AltAminoAcids, siftCache, polyphenCache, transcript.SiftIndex, transcript.PolyPhenIndex);

            return new AnnotatedTranscript(transcript, leftAnnotation.RefAminoAcids, leftAnnotation.AltAminoAcids,
                leftAnnotation.RefCodons, leftAnnotation.AltCodons, leftAnnotation.Position, hgvsCoding, hgvsProtein,
                sift, polyPhen, consequences, null, false);
        }

        private static ReadOnlySpan<char> GetExtendedCodingSequence(ISequence cdnaSequence, ICodingRegion codingRegion)
        {
            ReadOnlySpan<char> cdnaSpan = cdnaSequence.Sequence.AsSpan();
            return codingRegion == null ? cdnaSpan : cdnaSpan.Slice(codingRegion.CdnaStart - 1);
        }

        private static (VariantEffect VariantEffect, IMappedPosition Position, string RefAminoAcids, string
            AltAminoAcids, string RefCodons, string AltCodons, string TranscriptAltAllele, string TranscriptRefAllele,
            bool WithinGap)
            AnnotateTranscript(ITranscript transcript, ISimpleVariant variant, AminoAcid aminoAcids,
                ISequence cdnaSequence, ReadOnlySpan<char> codingSequence)
        {
            bool onReverseStrand = transcript.Gene.OnReverseStrand;
            (int startIndex, ITranscriptRegion startRegion) =
                MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.Start);
            (int endIndex, ITranscriptRegion endRegion) =
                MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.End);

            bool withinGap = startRegion      != null                     &&
                             endRegion        != null                     &&
                             startRegion.Type == TranscriptRegionType.Gap &&
                             endRegion.Type   == TranscriptRegionType.Gap &&
                             startRegion.Id   == endRegion.Id;

            var position = GetMappedPosition(transcript.TranscriptRegions, startRegion, startIndex, endRegion, endIndex,
                variant, onReverseStrand, transcript.Translation?.CodingRegion, transcript.StartExonPhase,
                variant.Type == VariantType.insertion);

            string transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, onReverseStrand);

            (string referenceCodons, string alternateCodons) = Codons.GetCodons(transcriptAltAllele, position.CdsStart,
                position.ExtendedCdsEnd, position.ProteinStart, position.ExtendedProteinEnd, codingSequence);

            (string referenceAminoAcids, string alternateAminoAcids) = aminoAcids.Translate(referenceCodons,
                alternateCodons, transcript.AminoAcidEdits, position.ProteinStart);

            (referenceAminoAcids, alternateAminoAcids, position.ProteinStart, position.ProteinEnd) =
                TryTrimAminoAcidsAndUpdateProteinPositions(referenceAminoAcids, alternateAminoAcids,
                    position.ProteinStart,                                      position.ProteinEnd);

            (position.CoveredCdnaStart, position.CoveredCdnaEnd) =
                transcript.TranscriptRegions.GetCoveredCdnaPositions(position.CdnaStart, startIndex, position.CdnaEnd,
                    endIndex, onReverseStrand);

            (position.CoveredCdsStart, position.CoveredCdsEnd, position.CoveredProteinStart,
                position.CoveredProteinEnd) = MappedPositionUtilities.GetCoveredCdsAndProteinPositions(
                position.CoveredCdnaStart, position.CoveredCdnaEnd, transcript.StartExonPhase,
                transcript.Translation?.CodingRegion);

            string transcriptRefAllele = HgvsUtilities.AdjustTranscriptRefAllele(
                HgvsUtilities.GetTranscriptAllele(variant.RefAllele, onReverseStrand), position.CoveredCdnaStart,
                position.CoveredCdnaEnd, cdnaSequence);

            // only generate the covered version of ref & alt alleles when CDS start/end is -1
            string coveredRefAa;
            string coveredAltAa;

            if (position.CdsStart == -1 || position.CdsEnd == -1)
            {
                (string coveredRefCodon, string coveredAltCodon) = Codons.GetCodons(transcriptAltAllele,
                    position.CoveredCdsStart, position.CoveredCdsEnd, position.CoveredProteinStart,
                    position.CoveredProteinEnd, codingSequence);

                (coveredRefAa, coveredAltAa) = aminoAcids.Translate(coveredRefCodon, coveredAltCodon,
                    transcript.AminoAcidEdits, position.ProteinStart);

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
                transcriptAltAllele, transcriptRefAllele, withinGap);
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

        private static ISequence GetCdnaSequence(ITranscript transcript, ISequence refSequence)
        {
            return transcript.CdnaSequence ?? (transcript.CdnaSequence = new CdnaSequence(refSequence,
                transcript.Translation?.CodingRegion, transcript.TranscriptRegions,
                transcript.Gene.OnReverseStrand, transcript.RnaEdits));
        }

        private static IMappedPosition GetMappedPosition(ITranscriptRegion[] regions, ITranscriptRegion startRegion,
            int startIndex, ITranscriptRegion endRegion, int endIndex, IInterval variant, bool onReverseStrand,
            ICodingRegion codingRegion, byte startExonPhase, bool isInsertion)
        {
            (int cdnaStart, int cdnaEnd) = isInsertion
                ? MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion, variant.Start, variant.End,
                    onReverseStrand)
                : MappedPositionUtilities.GetCdnaPositions(startRegion, endRegion, variant.Start, variant.End,
                    onReverseStrand);
            if (onReverseStrand) Swap.Int(ref cdnaStart, ref cdnaEnd);

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, cdnaStart, cdnaEnd,
                startExonPhase, isInsertion);

            int extendedCdsEnd = cdsStart != -1 && cdsEnd == -1
                ? MappedPositionUtilities.GetExtendedCdsPosition(codingRegion.CdnaStart, cdnaEnd, startExonPhase)
                : cdsEnd;

            int proteinStart       = MappedPositionUtilities.GetProteinPosition(cdsStart);
            int proteinEnd         = MappedPositionUtilities.GetProteinPosition(cdsEnd);
            int extendedProteinEnd = MappedPositionUtilities.GetProteinPosition(extendedCdsEnd);

            (int exonStart, int exonEnd, int intronStart, int intronEnd) =
                regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(cdnaStart, cdnaEnd, cdsStart, cdsEnd, extendedCdsEnd, proteinStart, proteinEnd, extendedProteinEnd, exonStart,
                exonEnd, intronStart, intronEnd, startIndex, endIndex);
        }

        private static TranscriptPositionalEffect GetPositionalEffect(ITranscript transcript, ISimpleVariant variant,
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

        private static (PredictionScore Sift, PredictionScore PolyPhen) GetPredictionScores(IMappedPosition position,
            string refAminoAcid, string altAminoAcid, IPredictionCache siftCache, IPredictionCache polyphenCache,
            int siftIndex, int polyphenIndex)
        {
            if (!NeedPredictionScore(position.ProteinStart, position.ProteinEnd, refAminoAcid, altAminoAcid) ||
                position.ProteinStart == -1) return (null, null);

            char newAminoAcid = altAminoAcid[0];
            var siftScore     = GetPredictionScore(position.ProteinStart, newAminoAcid, siftCache, siftIndex);
            var polyphenScore = GetPredictionScore(position.ProteinStart, newAminoAcid, polyphenCache, polyphenIndex);
            return (siftScore, polyphenScore);
        }

        private static bool NeedPredictionScore(int proteinStart, int proteinEnd, string referenceAminoAcids,
            string alternateAminoAcids)
        {
            return proteinStart != -1 &&
                   proteinEnd != -1 &&
                   proteinStart == proteinEnd &&
                   referenceAminoAcids.Length == 1 &&
                   alternateAminoAcids.Length == 1 &&
                   referenceAminoAcids != alternateAminoAcids;
        }

        private static PredictionScore GetPredictionScore(int proteinPosition, char newAminoAcid,
            IPredictionCache predictionCache, int predictionIndex)
        {
            return predictionIndex == -1
                ? null
                : predictionCache?.GetProteinFunctionPrediction(predictionIndex, newAminoAcid, proteinPosition);
        }
    }
}