using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using Intervals;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Caches.Utilities;
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

            var consequences = GetConsequences(transcript, leftShiftedVariant, leftAnnotation.VariantEffect);

            // if(transcript.Id.WithVersion == "NM_000314.6")
            //     Console.WriteLine("bug");

            var refAllele = rightAnnotation.TranscriptRefAllele;
            var altAllele = rightAnnotation.TranscriptAltAllele;//GetAlleleFromCodon(rightAnnotation.AltCodons);
            string hgvsCoding = HgvsCodingNomenclature.GetHgvscAnnotation(transcript, rightShiftedVariant, refSequence,
                    rightAnnotation.Position.RegionStartIndex, rightAnnotation.Position.RegionEndIndex, 
                    refAllele, altAllele);
            
            string hgvsProtein = HgvsProteinNomenclature.GetHgvsProteinAnnotation(transcript,
                rightAnnotation.RefAminoAcids, rightAnnotation.AltAminoAcids, rightAnnotation.TranscriptAltAllele,
                rightAnnotation.Position, rightAnnotation.VariantEffect, rightShiftedVariant, refSequence, hgvsCoding,
                leftShiftedVariant.Chromosome.UcscName == "chrM");

            (PredictionScore sift, PredictionScore polyPhen) = GetPredictionScores(leftAnnotation.Position, leftAnnotation.RefAminoAcids,
                leftAnnotation.AltAminoAcids, siftCache, polyphenCache, transcript.SiftIndex, transcript.PolyPhenIndex);

            return new AnnotatedTranscript(transcript, leftAnnotation.RefAminoAcids, leftAnnotation.AltAminoAcids,
                leftAnnotation.RefCodons, leftAnnotation.AltCodons, leftAnnotation.Position, hgvsCoding, hgvsProtein,
                sift, polyPhen, consequences, null, false);
        }

        private static string GetAlleleFromCodon(string codons)
        {
            if (string.IsNullOrEmpty(codons)) return null;
            return new string(codons.Where(char.IsUpper).ToArray());
        }

        private static (VariantEffect VariantEffect, IMappedPosition Position, string RefAminoAcids, string
            AltAminoAcids, string RefCodons, string AltCodons, string TranscriptAltAllele, string TranscriptRefAllele) AnnotateTranscript(ITranscript transcript, ISimpleVariant variant, AminoAcids aminoAcids, ISequence refSequence)
        {
            bool onReverseStrand = transcript.Gene.OnReverseStrand;
            (int startIndex, ITranscriptRegion startRegion) = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.Start);
            (int endIndex, ITranscriptRegion endRegion)     = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, variant.End);

            
            var position = GetMappedPosition(transcript.TranscriptRegions, startRegion, startIndex, endRegion,
                endIndex, variant, onReverseStrand, transcript.Translation?.CodingRegion, transcript.StartExonPhase,
                variant.Type == VariantType.insertion);

            // if(transcript.Id.WithVersion=="NM_033489.2")
            //     Console.WriteLine("bug");
            var codingSequence = GetCodingSequence(transcript, refSequence);
            var cdnaSequence = GetCdnaSequence(transcript, refSequence);
            //var codingFromCdna = transcript.Translation != null ?
            //                     GetCodingFromCdna(transcript.Translation.CodingRegion, cdnaSequence): null;
            // if (codingSequence != null && codingSequence.Substring(0,codingSequence.Length) !=codingFromCdna)
            // {
            //     Console.WriteLine($"Coding sequence mismatch !! Transcript Id: {transcript.Id.WithVersion}");
            //     Console.WriteLine(codingSequence.Substring(0,codingSequence.Length));
            //     Console.WriteLine(codingFromCdna);
            //     throw new InvalidDataException("mismatch between coding sequence and extracted from cdna sequence");
            // }
            
            var transcriptRefAllele = GetTranscriptRefAllele(variant, position, cdnaSequence, onReverseStrand);
            string transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, onReverseStrand);
            
            var codons = Codons.GetCodons(transcriptAltAllele, position.CdsStart, position.CdsEnd, position.ProteinStart, position.ProteinEnd, codingSequence);
            
            var aa = aminoAcids.Translate(codons.Reference, codons.Alternate);
            (aa, position.ProteinStart, position.ProteinEnd) = TryTrimAminoAcidsAndUpdateProteinPositions(aa, position.ProteinStart, position.ProteinEnd);

            (position.CoveredCdnaStart, position.CoveredCdnaEnd) = transcript.TranscriptRegions.GetCoveredCdnaPositions(position.CdnaStart, startIndex, position.CdnaEnd, endIndex, onReverseStrand);
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

            return (variantEffect, position, aa.Reference, aa.Alternate, codons.Reference, codons.Alternate, transcriptAltAllele, transcriptRefAllele);
        }

        private static string GetTranscriptRefAllele(ISimpleVariant variant, IMappedPosition position, ISequence cdnaSequence,
            bool onReverseStrand)
        {
            try
            {
                if (position == null || cdnaSequence==null) return null;
                var start = position.CdnaStart;
                var end = position.CdnaEnd;
                if (end < start) Swap.Int(ref start, ref end);
                var transcriptRefAllele = start != -1 && end != -1
                    ? cdnaSequence.Substring(start - 1, end - start + 1)
                    : null;

                transcriptRefAllele = transcriptRefAllele ?? HgvsUtilities.GetTranscriptAllele(variant.RefAllele, onReverseStrand);
                return transcriptRefAllele;
            }
            catch (Exception e)
            {
                Console.WriteLine($"cdna start:{position.CdnaStart}, cdna end: {position.CdnaEnd}");
                Console.WriteLine(e);
                throw;
            }
            
        }

        private static string GetCodingFromCdna(ICodingRegion codingRegion, ISequence cdnaSequence)
        {
            if (codingRegion == null) return null;
            return cdnaSequence.Substring(codingRegion.CdnaStart - 1, codingRegion.CdnaEnd- codingRegion.CdnaStart + 1);
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
        
        private static ISequence GetCdnaSequence(ITranscript transcript, ISequence refSequence)
        {
            if (transcript.Translation == null) return null;

            return transcript.CdnaSequence ?? (transcript.CdnaSequence = new CdnaSequence(refSequence,
                transcript.Translation.CodingRegion, transcript.TranscriptRegions,
                transcript.Gene.OnReverseStrand, transcript.RnaEdits));
        }

        private static IMappedPosition GetMappedPosition(ITranscriptRegion[] regions,  ITranscriptRegion startRegion, 
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