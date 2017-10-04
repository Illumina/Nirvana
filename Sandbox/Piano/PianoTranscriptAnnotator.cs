using System;
using System.Collections.Generic;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Piano
{
    public static class PianoTranscriptAnnotator
    {
        private const int FlankingAminoAcidLength = 15;
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant variant, ISequence refSequence,
            AminoAcids aminoAcidsProvider)
        {

            var mappedPositions = MappedPositionsUtils.ComputeMappedPositions(variant.Start, variant.End, transcript);

            var transcriptRefAllele = HgvsUtilities.GetTranscriptAllele(variant.RefAllele, transcript.Gene.OnReverseStrand);
            var transcriptAltAllele = HgvsUtilities.GetTranscriptAllele(variant.AltAllele, transcript.Gene.OnReverseStrand);

            var codonsAndAminoAcids = GetCodonsAndAminoAcids(transcript, refSequence, transcriptRefAllele, transcriptAltAllele, variant, mappedPositions, aminoAcidsProvider);

            var referenceCodons = codonsAndAminoAcids.Item1;
            var alternateCodons = codonsAndAminoAcids.Item2;
            var referenceAminoAcids = codonsAndAminoAcids.Item3;
            var alternateAminoAcids = codonsAndAminoAcids.Item4;


            var insertionInStartCodonAndNoimpact = variant.Type == VariantType.insertion &&
                                                   mappedPositions.ProteinInterval.Start <= 1 &&
                                                   alternateAminoAcids.EndsWith(referenceAminoAcids);

            var variantEffect = GetVariantEffect(transcript, variant, mappedPositions, referenceAminoAcids,
                alternateAminoAcids, referenceCodons, alternateCodons, insertionInStartCodonAndNoimpact);


            var consequences = GetConsequences(transcript, variant, variantEffect);

            var proteinBegin = mappedPositions.ProteinInterval.Start == null
                ? -1
                : mappedPositions.ProteinInterval.Start.Value;

            var proteinEnd = mappedPositions.ProteinInterval.End == null
                ? -1
                : mappedPositions.ProteinInterval.End.Value;

            var upStreamAminoAcids = GetFlankingPeptides(transcript.Translation?.PeptideSeq, proteinBegin, proteinEnd, FlankingAminoAcidLength, true);
            var downStreamAminoAcids = consequences.Contains(ConsequenceTag.frameshift_variant)? null: GetFlankingPeptides(transcript.Translation?.PeptideSeq, proteinBegin, proteinEnd, FlankingAminoAcidLength, false);

            return new PianoAnnotatedTranscript(transcript,referenceAminoAcids, alternateAminoAcids, mappedPositions,upStreamAminoAcids,downStreamAminoAcids,consequences);
        }

        private static string GetFlankingPeptides(string peptideSeq, int proteinBegin,int proteinEnd, int nBase, bool upStrem)
        {
            if (peptideSeq == null) return null;
            if (proteinBegin == -1 && proteinEnd == -1) return null;
            if (proteinBegin == -1) proteinBegin = proteinEnd;
            if (proteinEnd == -1) proteinEnd = proteinBegin;

            if (upStrem)
            {
                var peptideStart = Math.Max(1, proteinBegin - nBase);
                return peptideSeq.Substring(peptideStart - 1, (proteinBegin - peptideStart));
            }

            var peptideEnd = Math.Min(peptideSeq.Length, proteinEnd + nBase);
            return peptideEnd > proteinEnd + 1 ? peptideSeq.Substring(proteinEnd, (peptideEnd - proteinEnd)) : "";
        }


        private static Tuple<string, string, string, string> GetCodonsAndAminoAcids(ITranscript transcript, ISequence refSequence,
            string transcriptRefAllele, string transcriptAltAllele, ISimpleVariant variant,
            IMappedPositions mappedPositions, AminoAcids aminoAcidsProvider)
        {
            var codingSequence = transcript.Translation == null
                ? null
                : new CodingSequence(refSequence, transcript.Translation.CodingRegion.Start,
                    transcript.Translation.CodingRegion.End, transcript.CdnaMaps, transcript.Gene.OnReverseStrand,
                    transcript.StartExonPhase);

            // compute codons and amino acids
            AssignCodonsAndAminoAcids(transcriptRefAllele, transcriptAltAllele, mappedPositions,
                codingSequence, aminoAcidsProvider, out string referenceCodons,
                out string alternateCodons, out string referenceAminoAcids, out string alternateAminoAcids);

            return Tuple.Create(referenceCodons ?? "", alternateCodons ?? "", referenceAminoAcids ?? "",
                alternateAminoAcids ?? "");
        }

        private static void AssignCodonsAndAminoAcids(string transcriptRefAllele, string transcriptAltAllele,
            IMappedPositions mappedPositions, ISequence codingSequence, AminoAcids aminoAcidProvier, out string refCodons,
            out string altCodons, out string refAminoAcids, out string altAminoAcids)
        {
            AssignExtended(transcriptRefAllele, transcriptAltAllele, mappedPositions.CdsInterval,
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

        private static VariantEffect GetVariantEffect(ITranscript transcript, ISimpleVariant variant, IMappedPositions mappedPositions, string refAminoAcids, string altAminoAcids, string refCodons, string altCodons, bool insertionInStartAndNoImpact)
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(transcript.Introns, variant, variant.Type);
            positionalEffect.DetermineExonicEffect(transcript, variant, mappedPositions, variant.AltAllele, insertionInStartAndNoImpact);

            var variantEffect = new VariantEffect(positionalEffect, variant, transcript, refAminoAcids,
                altAminoAcids,
                refCodons, altCodons, mappedPositions.ProteinInterval.Start);
            return variantEffect;
        }

        private static void AssignExtended(string transcriptReferenceAllele, string transcriptAlternateAllele,
            NullableInterval cdsInterval, NullableInterval proteinInterval, ISequence codingSequence, out string refCodons, out string altCodons)
        {
            refCodons = null;
            altCodons = null;

            if (cdsInterval.Start == null || cdsInterval.End == null || proteinInterval.Start == null ||
                proteinInterval.End == null) return;

            int aminoAcidStart = proteinInterval.Start.Value * 3 - 2;
            int aminoAcidEnd = proteinInterval.End.Value * 3;

            int prefixLen = cdsInterval.Start.Value - aminoAcidStart;
            int suffixLen = aminoAcidEnd - cdsInterval.End.Value;

            int start1 = aminoAcidStart - 1;
            int start2 = aminoAcidEnd - suffixLen;

            int maxSuffixLen = codingSequence.Length - start2;

            var atTailEnd = false;
            if (suffixLen > maxSuffixLen)
            {
                suffixLen = maxSuffixLen;
                atTailEnd = true;
            }

            if (suffixLen > maxSuffixLen) suffixLen = maxSuffixLen;

            string prefix = start1 + prefixLen < codingSequence.Length
                ? codingSequence.Substring(start1, prefixLen).ToLower()
                : "AAA";

            string suffix = suffixLen > 0
                ? codingSequence.Substring(start2, suffixLen).ToLower()
                : "";

            var needExtend = !atTailEnd && !Codons.IsTriplet(prefixLen + suffixLen + transcriptAlternateAllele.Length);
            var extendedLen = (maxSuffixLen - suffixLen) > 45 ? 45 : (maxSuffixLen - suffixLen) / 3 * 3;
            if (needExtend) suffix = codingSequence.Substring(start2, suffixLen + extendedLen);


            refCodons = Codons.GetCodon(transcriptReferenceAllele, prefix, suffix);
            altCodons = Codons.GetCodon(transcriptAlternateAllele, prefix, suffix);
        }


    }
}