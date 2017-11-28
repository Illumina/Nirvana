using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FullTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant variant,
            ISequence refSequence, IPredictionCache siftCache, IPredictionCache polyphenCache, AminoAcids aminoAcids)
        {
            #region duplicate

            // todo: refactor to reduce code duplication
            var mappedPositions = MappedPositionsUtils.ComputeMappedPositions(variant.Start, variant.End, transcript);

            var transcriptRefAllele = HgvsUtilities.GetTranscriptAllele(variant.RefAllele, transcript.Gene.OnReverseStrand);
            var transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, transcript.Gene.OnReverseStrand);

            var caa = GetCodonsAndAminoAcids(transcript, refSequence, transcriptRefAllele,
                transcriptAltAllele, mappedPositions, aminoAcids);

            var insertionInStartCodonAndNoimpact = variant.Type == VariantType.insertion &&
                                                   mappedPositions.ProteinInterval.Start <= 1 &&
                                                   caa.AlternateAminoAcids.EndsWith(caa.ReferenceAminoAcids);

            var variantEffect = GetVariantEffect(transcript, variant, mappedPositions, caa.ReferenceAminoAcids,
                caa.AlternateAminoAcids, caa.ReferenceCodons, caa.AlternateCodons, insertionInStartCodonAndNoimpact);

            #endregion

            var consequences = GetConsequences(transcript, variant, variantEffect);

            var rightShiftedVariant = VariantRotator.Right(variant, transcript, refSequence,
                transcript.Gene.OnReverseStrand, out var shiftToEnd);

            var hgvsCoding = shiftToEnd
                ? null
                : HgvsCodingNomenclature.GetHgvscAnnotation(transcript, rightShiftedVariant ?? variant, refSequence);

            var hgvsProtein = ReferenceEquals(rightShiftedVariant, variant)
                ? GetHgvspLeftAlignPath(transcript, variant, variantEffect, transcriptAltAllele, refSequence,
                    caa.ReferenceAminoAcids, caa.AlternateAminoAcids, mappedPositions, hgvsCoding)
                : GetHgvspForRightAlignPath(transcript, rightShiftedVariant, refSequence, hgvsCoding, aminoAcids);

            PredictionScore siftScore = null, polyphenScore = null;

            if (NeedPredictionScore(mappedPositions.ProteinInterval, caa.ReferenceAminoAcids, caa.AlternateAminoAcids))
            {
                siftScore     = GetPredictionScore(mappedPositions.ProteinInterval.Start.Value, caa.AlternateAminoAcids, siftCache, transcript.SiftIndex);
                polyphenScore = GetPredictionScore(mappedPositions.ProteinInterval.Start.Value, caa.AlternateAminoAcids, polyphenCache, transcript.PolyPhenIndex);
            }

            return new AnnotatedTranscript(transcript, caa.ReferenceAminoAcids, caa.AlternateAminoAcids,
                caa.ReferenceCodons, caa.AlternateCodons, mappedPositions, hgvsCoding, hgvsProtein, siftScore,
                polyphenScore, consequences, null);
        }

        private static PredictionScore GetPredictionScore(int proteinPosition, string alternateAminoAcids,
            IPredictionCache predictionCache, int predictionIndex)
        {
            return predictionIndex == -1
                ? null
                : predictionCache?.GetProteinFunctionPrediction(predictionIndex, alternateAminoAcids[0],
                    proteinPosition);
        }

        private static bool NeedPredictionScore(NullableInterval proteinInterval, string referenceAminoAcids,
            string alternateAminoAcids)
        {
            return proteinInterval.Start != null &&
                   proteinInterval.End   != null &&
                   proteinInterval.Start.Value == proteinInterval.End.Value &&
                   referenceAminoAcids.Length  == 1 &&
                   alternateAminoAcids.Length  == 1 &&
                   referenceAminoAcids   != alternateAminoAcids;
        }

        private static (string ReferenceCodons, string AlternateCodons, string ReferenceAminoAcids, string
            AlternateAminoAcids) GetCodonsAndAminoAcids(ITranscript transcript, ISequence refSequence,
                string transcriptRefAllele, string transcriptAltAllele, IMappedPositions mappedPositions,
                AminoAcids aminoAcids)
        {
            var codingSequence = transcript.Translation == null
                ? null
                : new CodingSequence(refSequence, transcript.Translation.CodingRegion.Start,
                    transcript.Translation.CodingRegion.End, transcript.CdnaMaps, transcript.Gene.OnReverseStrand,
                    transcript.StartExonPhase);

            // compute codons and amino acids
            AssignCodonsAndAminoAcids(transcriptRefAllele, transcriptAltAllele, mappedPositions,
                codingSequence, aminoAcids, out string referenceCodons,
                out string alternateCodons, out string referenceAminoAcids, out string alternateAminoAcids);

            return (referenceCodons ?? "", alternateCodons ?? "", referenceAminoAcids ?? "", alternateAminoAcids ?? "");
        }

        private static void AssignCodonsAndAminoAcids(string transcriptRefAllele, string transcriptAltAllele,
            IMappedPositions mappedPositions, ISequence codingSequence, AminoAcids aminoAcidProvier, out string refCodons,
            out string altCodons, out string refAminoAcids, out string altAminoAcids)
        {
            Codons.Assign(transcriptRefAllele, transcriptAltAllele, mappedPositions.CdsInterval,
                mappedPositions.ProteinInterval, codingSequence, out refCodons, out altCodons);

            aminoAcidProvier.Assign(refCodons, altCodons, out refAminoAcids, out altAminoAcids);
        }

        private static List<ConsequenceTag> GetConsequences(ITranscript transcript, IVariant variant, VariantEffect variantEffect)
        {
            var featureEffect = new FeatureVariantEffects(transcript, variant.Type, variant.Start, variant.End,
                variant.Behavior.StructuralVariantConsequence);

            var consequence = new Consequences(variantEffect, featureEffect);
            consequence.DetermineSmallVariantEffects();
            return consequence.GetConsequences();
        }

        private static VariantEffect GetVariantEffect(ITranscript transcript, ISimpleVariant variant,
            IMappedPositions mappedPositions, string refAminoAcids, string altAminoAcids, string refCodons,
            string altCodons, bool insertionInStartAndNoImpact)
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(transcript.Introns, variant, variant.Type);
            positionalEffect.DetermineExonicEffect(transcript, variant, mappedPositions, variant.AltAllele,
                insertionInStartAndNoImpact);

            return new VariantEffect(positionalEffect, variant, transcript, refAminoAcids, altAminoAcids,
                refCodons, altCodons, mappedPositions.ProteinInterval.Start);
        }

        private static string GetHgvspLeftAlignPath(ITranscript transcript, ISimpleVariant variant,
            VariantEffect variantEffect, string transcriptAltAllele, ISequence refSequence, string referenceAminoAcids,
            string alternateAminoAcids, IMappedPositions mappedPositions, string hgvsCoding)
        {
            return HgvsProteinNomenclature.GetHgvsProteinAnnotation(transcript, referenceAminoAcids,
                alternateAminoAcids, transcriptAltAllele, mappedPositions, variantEffect, variant, refSequence,
                hgvsCoding, variant.Chromosome.UcscName == "chrM");
        }

        private static string GetHgvspForRightAlignPath(ITranscript transcript, ISimpleVariant variant,
            ISequence refSequence, string hgvsCoding, AminoAcids aminoAcids)
        {
            #region duplicate

            // todo: refactor to reduce duplicate code (see above)
            var mappedPositions = MappedPositionsUtils.ComputeMappedPositions(variant.Start, variant.End, transcript);

            var transcriptRefAllele = HgvsUtilities.GetTranscriptAllele(variant.RefAllele, transcript.Gene.OnReverseStrand);
            var transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, transcript.Gene.OnReverseStrand);

            var caa = GetCodonsAndAminoAcids(transcript, refSequence, transcriptRefAllele, transcriptAltAllele, mappedPositions, aminoAcids);

            var insertionInStartCodonAndNoimpact = variant.Type == VariantType.insertion &&
                                                   mappedPositions.ProteinInterval.Start <= 1 &&
                                                   caa.AlternateAminoAcids.EndsWith(caa.ReferenceAminoAcids);

            var variantEffect = GetVariantEffect(transcript, variant, mappedPositions, caa.ReferenceAminoAcids,
                caa.AlternateAminoAcids, caa.ReferenceCodons, caa.AlternateCodons, insertionInStartCodonAndNoimpact);

            #endregion

            return HgvsProteinNomenclature.GetHgvsProteinAnnotation(transcript, caa.ReferenceAminoAcids,
                caa.AlternateAminoAcids, transcriptAltAllele, mappedPositions, variantEffect, variant, refSequence,
                hgvsCoding, variant.Chromosome.UcscName == "chrM");
        }
    }
}