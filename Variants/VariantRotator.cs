using System;
using Genome;
using Intervals;

namespace Variants
{
    public static class VariantRotator
    {
        public const int MaxDownstreamLength = 500;

        public static ISimpleVariant Right(ISimpleVariant simpleVariant, IInterval rotateRegion, ISequence refSequence, bool onReverseStrand)
        {            
            if (refSequence == null) return simpleVariant;

            if (simpleVariant.Type != VariantType.deletion && simpleVariant.Type != VariantType.insertion)
                return simpleVariant;

            if (VariantStartOverlapsRegion(simpleVariant, rotateRegion, onReverseStrand))
                return simpleVariant;
            // if variant is before the transcript start, do not perform 3 prime shift
            
            string rotatingBases = GetRotatingBases(simpleVariant, onReverseStrand);

            string downStreamSeq = GetDownstreamSeq(simpleVariant, rotateRegion, refSequence, onReverseStrand, rotatingBases);

            string combinedSequence = rotatingBases + downStreamSeq;

            int shiftStart, shiftEnd;
            var hasShifted = false;

            // probably a VEP bug, just use it for consistency
            int numBases = rotatingBases.Length;

            for (shiftStart = 0, shiftEnd = numBases; shiftEnd < combinedSequence.Length; shiftStart++, shiftEnd++)
            {
                if (combinedSequence[shiftStart] != combinedSequence[shiftEnd]) break;
                hasShifted = true;
            }

            if (!hasShifted) return simpleVariant;

            // create a new alternative allele
            string rotatedSequence = combinedSequence.Substring(shiftStart, numBases);
            int rotatedStart       = simpleVariant.Start + shiftStart;
            int rotatedEnd         = simpleVariant.End + shiftStart;

            if (onReverseStrand)
            {
                rotatedSequence = SequenceUtilities.GetReverseComplement(rotatedSequence);
                rotatedStart    = simpleVariant.Start - shiftStart;
                rotatedEnd      = simpleVariant.End - shiftStart;
            }
            
            string rotatedRefAllele = simpleVariant.RefAllele;
            string rotatedAltAllele = simpleVariant.AltAllele;

            if (simpleVariant.Type == VariantType.insertion) rotatedAltAllele = rotatedSequence;
            else rotatedRefAllele = rotatedSequence;

            return new SimpleVariant(simpleVariant.Chromosome, rotatedStart, rotatedEnd, rotatedRefAllele,
                rotatedAltAllele, simpleVariant.Type);
        }

        private static string GetDownstreamSeq(IInterval simpleVariant, IInterval rotateRegion,
            ISequence refSequence, bool onReverseStrand, string rotatingBases)
        {
            int basesToEnd = onReverseStrand ? simpleVariant.Start - rotateRegion.Start : rotateRegion.End - simpleVariant.End;
            int downStreamLength =
                Math.Min(basesToEnd,
                    Math.Max(rotatingBases.Length,
                        MaxDownstreamLength)); // for large rotatingBases, we need to factor in its length but still make sure that we do not go past the end of transcript

            var downStreamSeq = onReverseStrand
                ? SequenceUtilities.GetReverseComplement(
                    refSequence.Substring(simpleVariant.Start - 1 - downStreamLength, downStreamLength))
                : refSequence.Substring(simpleVariant.End, downStreamLength);
            return downStreamSeq;
        }

        private static string GetRotatingBases(ISimpleVariant simpleVariant, bool onReverseStrand)
        {
            string rotatingBases = simpleVariant.Type == VariantType.insertion ? simpleVariant.AltAllele : simpleVariant.RefAllele;
            rotatingBases = onReverseStrand ? SequenceUtilities.GetReverseComplement(rotatingBases) : rotatingBases;
            return rotatingBases;
        }

        private static bool VariantStartOverlapsRegion(IInterval variant, IInterval region, bool onReverseStrand)
        {
            if (onReverseStrand)
            {
                return variant.End > region.End || region.Start >= variant.End;
            }
            
            return variant.Start < region.Start || region.End <= variant.Start;
        }
    }
}