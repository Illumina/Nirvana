using System.Collections.Generic;
using Genome;
using Intervals;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
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
            ISequence refSequence, IPredictionCache siftCache, IPredictionCache polyphenCache, AminoAcids aminoAcids)
        {
            var rightShiftedVariant = VariantRotator.Right(leftShiftedVariant, transcript, refSequence,
                transcript.Gene.OnReverseStrand);

            var leftAnnotation = AnnotateTranscript(transcript, leftShiftedVariant, aminoAcids, refSequence);

            var rightAnnotation = ReferenceEquals(leftShiftedVariant, rightShiftedVariant)
                ? leftAnnotation
                : AnnotateTranscript(transcript, rightShiftedVariant, aminoAcids, refSequence);

            List<ConsequenceTag> consequences = GetConsequences(transcript, transcript.Gene.OnReverseStrand,
                leftShiftedVariant, leftAnnotation.VariantEffect);
            
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
                predictionScores.Sift, predictionScores.PolyPhen, consequences, false);
        }

        private static (VariantEffect VariantEffect, IMappedPosition Position, string RefAminoAcids, string
            AltAminoAcids, string RefCodons, string AltCodons, string TranscriptAltAllele) AnnotateTranscript(ITranscript transcript, ISimpleVariant variant, AminoAcids aminoAcids, ISequence refSequence)
        {
            bool onReverseStrand = transcript.Gene.OnReverseStrand;
            var start = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.Start);
            var end   = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.End);

            var position = GetMappedPosition(transcript.TranscriptRegions, start.Region, start.Index, end.Region,
                end.Index, variant, onReverseStrand, transcript.Translation?.CodingRegion, transcript.StartExonPhase,
                variant.Type == VariantType.insertion);

            string transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, onReverseStrand);
            var codingSequence = GetCodingSequence(transcript, refSequence);
            var codons = Codons.GetCodons(transcriptAltAllele, position.CdsStart, position.CdsEnd, position.ProteinStart, position.ProteinEnd, codingSequence);
            
            var aa = aminoAcids.Translate(codons.Reference, codons.Alternate);
            (aa, position.ProteinStart, position.ProteinEnd) = TryTrimAminoAcidsAndUpdateProteinPositions(aa, position.ProteinStart, position.ProteinEnd);

            (position.CoveredCdnaStart, position.CoveredCdnaEnd) = transcript.TranscriptRegions.GetCoveredCdnaPositions(position.CdnaStart, start.Index, position.CdnaEnd, end.Index, onReverseStrand);
            (position.CoveredCdsStart, position.CoveredCdsEnd, position.CoveredProteinStart, position.CoveredProteinEnd) = MappedPositionUtilities.GetCoveredCdsAndProteinPositions(position.CoveredCdnaStart, position.CoveredCdnaEnd, transcript.StartExonPhase, transcript.Translation?.CodingRegion);

            SequenceChange coveredAa;
            // only generate the covered version of ref & alt alleles when CDS start/end is -1
            if (position.CdsStart == -1 || position.CdsEnd == -1)
            {
                coveredAa = GetCoveredAa(aminoAcids, transcriptAltAllele, position.CoveredCdsStart, position.CoveredCdsEnd, position.CoveredProteinStart, position.CoveredProteinEnd, codingSequence);
                (coveredAa, position.CoveredProteinStart, position.CoveredProteinEnd) = TryTrimAminoAcidsAndUpdateProteinPositions(coveredAa, position.CoveredProteinStart, position.CoveredProteinEnd);
            }
            else
            {
                coveredAa = aa;
                position.CoveredProteinStart = position.ProteinStart;
                position.CoveredProteinEnd = position.ProteinEnd;
            }
            

            var positionalEffect = GetPositionalEffect(transcript, variant, position, aa.Reference, aa.Alternate,
                position.CoveredCdnaStart, position.CoveredCdnaEnd, position.CoveredCdsStart, position.CoveredCdsEnd);

            var variantEffect = new VariantEffect(positionalEffect, variant, transcript, aa.Reference, aa.Alternate,
                codons.Reference, codons.Alternate, position.ProteinStart, coveredAa.Reference, coveredAa.Alternate);

            return (variantEffect, position, aa.Reference, aa.Alternate, codons.Reference, codons.Alternate, transcriptAltAllele);
        }

        internal static (SequenceChange AaChange, int ProteinStart, int ProteinEnd) TryTrimAminoAcidsAndUpdateProteinPositions(SequenceChange aaChange, int proteinStart, int proteinEnd)
        {
            (int newStart, string newReference, string newAlternate) = BiDirectionalTrimmer.Trim(proteinStart, aaChange.Reference, aaChange.Alternate);

            return string.IsNullOrEmpty(newReference) ? (aaChange, proteinStart, proteinEnd) : 
                (new SequenceChange(newReference, newAlternate), newStart, newStart + newReference.Length - 1);
        }

        private static SequenceChange GetCoveredAa(AminoAcids aminoAcids, string transcriptAltAllele, int coveredCdsStart, int coveredCdsEnd, int coveredProteinStart, int coveredProteinEnd, ISequence codingSequence)
        {
            var codonsChange = Codons.GetCodons(transcriptAltAllele, coveredCdsStart, coveredCdsEnd, coveredProteinStart, coveredProteinEnd, codingSequence);
            return aminoAcids.Translate(codonsChange.Reference, codonsChange.Alternate);
        }

        private static ISequence GetCodingSequence(ITranscript transcript, ISequence refSequence)
        {
            if (transcript.Translation == null) return null;

            return transcript.CodingSequence ?? (transcript.CodingSequence = new CodingSequence(refSequence,
                       transcript.Translation.CodingRegion, transcript.TranscriptRegions,
                       transcript.Gene.OnReverseStrand, transcript.StartExonPhase, transcript.RnaEdits));
        }

        private static IMappedPosition GetMappedPosition(ITranscriptRegion[] regions, ITranscriptRegion startRegion, 
            int startIndex, ITranscriptRegion endRegion, int endIndex, IInterval variant, bool onReverseStrand,
            ICodingRegion codingRegion, byte startExonPhase, bool isInsertion)
        {
            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetCdnaPositions(startRegion, endRegion, variant, onReverseStrand, isInsertion);
            if (onReverseStrand) Swap.Int(ref cdnaStart, ref cdnaEnd);

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, cdnaStart, cdnaEnd,
                startExonPhase, isInsertion);

            int proteinStart = MappedPositionUtilities.GetProteinPosition(cdsStart);
            int proteinEnd   = MappedPositionUtilities.GetProteinPosition(cdsEnd);

            (int exonStart, int exonEnd, int intronStart, int intronEnd) = regions.GetExonsAndIntrons(startIndex, endIndex);

            return new MappedPosition(cdnaStart, cdnaEnd, cdsStart, cdsEnd, proteinStart, proteinEnd, exonStart,
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

        private static List<ConsequenceTag> GetConsequences(IInterval transcript, bool onReverseStrand, IVariant variant,
            IVariantEffect variantEffect)
        {
            OverlapType overlapType = Intervals.Utilities.GetOverlapType(transcript.Start, transcript.End, variant.Start, variant.End);
            EndpointOverlapType endpointOverlapType = Intervals.Utilities.GetEndpointOverlapType(transcript.Start, transcript.End, variant.Start, variant.End);
            var featureEffect = new FeatureVariantEffects(overlapType, endpointOverlapType, onReverseStrand, variant.Type, variant.IsStructuralVariant);

            var consequence = new Consequences(variant.Type, variantEffect, featureEffect);
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