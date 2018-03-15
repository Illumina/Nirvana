using System;
using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FullTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant leftShiftedVariant,
            ISequence refSequence, IPredictionCache siftCache, IPredictionCache polyphenCache, AminoAcids aminoAcids)
        {
            var rightShiftedVariant = VariantRotator.Right(leftShiftedVariant, transcript, refSequence,
                transcript.Gene.OnReverseStrand);

            
            var leftAnnotation = AnnotateTranscript(transcript, leftShiftedVariant, aminoAcids, refSequence);

            var rightAnnotation = ReferenceEquals(leftShiftedVariant, rightShiftedVariant)
                ? leftAnnotation
                : AnnotateTranscript(transcript, rightShiftedVariant, aminoAcids, refSequence);

            var consequences = GetConsequences(transcript, leftShiftedVariant, leftAnnotation.VariantEffect);

            var hgvsCoding = HgvsCodingNomenclature.GetHgvscAnnotation(transcript, rightShiftedVariant, refSequence,
                    rightAnnotation.Position.RegionStartIndex, rightAnnotation.Position.RegionEndIndex);

            var hgvsProtein = HgvsProteinNomenclature.GetHgvsProteinAnnotation(transcript,
                rightAnnotation.RefAminoAcids, rightAnnotation.AltAminoAcids, rightAnnotation.TranscriptAltAllele,
                rightAnnotation.Position, rightAnnotation.VariantEffect, rightShiftedVariant, refSequence, hgvsCoding,
                leftShiftedVariant.Chromosome.UcscName == "chrM");

            var predictionScores = GetPredictionScores(leftAnnotation.Position, leftAnnotation.RefAminoAcids,
                leftAnnotation.AltAminoAcids, siftCache, polyphenCache, transcript.SiftIndex, transcript.PolyPhenIndex);

            return new AnnotatedTranscript(transcript, leftAnnotation.RefAminoAcids, leftAnnotation.AltAminoAcids,
                leftAnnotation.RefCodons, leftAnnotation.AltCodons, leftAnnotation.Position, hgvsCoding, hgvsProtein,
                predictionScores.Sift, predictionScores.PolyPhen, consequences, null);
        }

        internal static (VariantEffect VariantEffect, IMappedPosition Position, string RefAminoAcids, string
            AltAminoAcids, string RefCodons, string AltCodons, string TranscriptAltAllele) AnnotateTranscript(ITranscript transcript, ISimpleVariant variant, AminoAcids aminoAcids,
                ISequence refSequence)
        {
            var onReverseStrand = transcript.Gene.OnReverseStrand;
            var start = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.Start);
            var end   = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.End);

            var position = GetMappedPosition(transcript.TranscriptRegions, start.Region, start.Index, end.Region,
                end.Index, variant, onReverseStrand, transcript.Translation?.CodingRegion, transcript.StartExonPhase,
                variant.Type == VariantType.insertion);

            var transcriptRefAllele = HgvsUtilities.GetTranscriptAllele(variant.RefAllele, onReverseStrand);
            var transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, onReverseStrand);

            var codingSequence = transcript.Translation == null
                ? null
                : new CodingSequence(refSequence, transcript.Translation.CodingRegion, transcript.TranscriptRegions,
                    transcript.Gene.OnReverseStrand, transcript.StartExonPhase);

            var codons = Codons.GetCodons(transcriptRefAllele, transcriptAltAllele, position.CdsStart, position.CdsEnd,
                position.ProteinStart, position.ProteinEnd, codingSequence);

            var coveredCdna = transcript.TranscriptRegions.GetCoveredCdnaPositions(position.CdnaStart, start.Index,
                position.CdnaEnd, end.Index, onReverseStrand);

            var coveredCds = MappedPositionUtilities.GetCoveredCdsPositions(coveredCdna.Start, coveredCdna.End,
                transcript.StartExonPhase, transcript.Translation?.CodingRegion);

            var aa = aminoAcids.Translate(codons.Reference, codons.Alternate);

            var positionalEffect = GetPositionalEffect(transcript, variant, position, aa.Reference, aa.Alternate,
                coveredCdna.Start, coveredCdna.End, coveredCds.Start, coveredCds.End);

            var variantEffect = new VariantEffect(positionalEffect, variant, transcript, aa.Reference, aa.Alternate,
                codons.Reference, codons.Alternate, position.ProteinStart);

            return (variantEffect, position, aa.Reference, aa.Alternate, codons.Reference, codons.Alternate,
                transcriptAltAllele);
        }

        private static IMappedPosition GetMappedPosition(ITranscriptRegion[] regions, ITranscriptRegion startRegion,
            int startIndex, ITranscriptRegion endRegion, int endIndex, IInterval variant, bool onReverseStrand,
            ICodingRegion codingRegion, byte startExonPhase, bool isInsertion)
        {
            var (cdnaStart, cdnaEnd) = MappedPositionUtilities.GetCdnaPositions(startRegion, endRegion, variant, onReverseStrand, isInsertion);
            if (onReverseStrand) Swap.Int(ref cdnaStart, ref cdnaEnd);

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, cdnaStart, cdnaEnd,
                startExonPhase, isInsertion);

            var proteinStart = MappedPositionUtilities.GetProteinPosition(cdsStart);
            var proteinEnd   = MappedPositionUtilities.GetProteinPosition(cdsEnd);

            var (exonStart, exonEnd, intronStart, intronEnd) = regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(cdnaStart, cdnaEnd, cdsStart, cdsEnd, proteinStart, proteinEnd, exonStart,
                exonEnd, intronStart, intronEnd, startIndex, endIndex);
        }

        private static TranscriptPositionalEffect GetPositionalEffect(ITranscript transcript, ISimpleVariant variant,
            IMappedPosition position, string refAminoAcid, string altAminoAcid, int coveredCdnaStart,
            int coveredCdnaEnd, int coveredCdsStart, int coveredCdsEnd)
        {
            var startCodonInsertionWithNoImpact = variant.Type == VariantType.insertion &&
                                                  position.ProteinStart <= 1 &&
                                                  altAminoAcid.EndsWith(refAminoAcid);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(transcript.TranscriptRegions, variant, variant.Type);
            positionalEffect.DetermineExonicEffect(transcript, variant, position, coveredCdnaStart, coveredCdnaEnd,
                coveredCdsStart, coveredCdsEnd, variant.AltAllele, startCodonInsertionWithNoImpact);
            return positionalEffect;
        }

        private static List<ConsequenceTag> GetConsequences(ITranscript transcript, IVariant variant,
            VariantEffect variantEffect)
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

            var newAminoAcid  = altAminoAcid[0];
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