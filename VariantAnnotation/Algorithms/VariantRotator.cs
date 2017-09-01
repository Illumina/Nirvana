using System;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Algorithms
{
    public static class VariantRotator
    {
        internal const int MaxDownstreamLength = 500;

        public static ISimpleVariant Right(ISimpleVariant simpleVariant, ITranscript transcript, ISequence refSequence,
            out bool shiftToEnd)
        {
            shiftToEnd = false;
            if (refSequence == null) return simpleVariant;

            if (simpleVariant.Type != VariantType.deletion && simpleVariant.Type != VariantType.insertion)
                return simpleVariant;

            var onReverseStrand = transcript.Gene.OnReverseStrand;

            // if variant is before the transcript start, do not perform 3 prime shift
            if (onReverseStrand  && simpleVariant.End   > transcript.End)   return simpleVariant;
            if (!onReverseStrand && simpleVariant.Start < transcript.Start) return simpleVariant;

            // consider insertion since insertion begin is larger than end
            // TODO: we shouldn't need special logic for insertions
            if (!onReverseStrand && simpleVariant.Start >= transcript.End)   return simpleVariant;
            // TODO: unable to find a situation where this is true
            if (onReverseStrand  && simpleVariant.End   <= transcript.Start) return simpleVariant;

            var rotatingBases = simpleVariant.Type == VariantType.insertion ? simpleVariant.AltAllele : simpleVariant.RefAllele;
            rotatingBases     = onReverseStrand ? SequenceUtilities.GetReverseComplement(rotatingBases) : rotatingBases;

            var basesToEnd       = onReverseStrand ? simpleVariant.Start - transcript.Start : transcript.End - simpleVariant.End;
            var downStreamLength = Math.Min(basesToEnd, MaxDownstreamLength);

            var downStreamSeq = onReverseStrand
                ? SequenceUtilities.GetReverseComplement(
                    refSequence.Substring(simpleVariant.Start - 1 - downStreamLength, downStreamLength))
                : refSequence.Substring(simpleVariant.End, downStreamLength);

            var combinedSequence = rotatingBases + downStreamSeq;

            int shiftStart, shiftEnd;
            var hasShifted = false;

            // TODO: probably a VEP bug, just use it for consistency
            var numBases = rotatingBases.Length;

            for (shiftStart = 0, shiftEnd = numBases; shiftEnd <= combinedSequence.Length - numBases; shiftStart++, shiftEnd++)
            {
                if (combinedSequence[shiftStart] != combinedSequence[shiftEnd]) break;
                hasShifted = true;
            }

            if (shiftStart >= basesToEnd) shiftToEnd = true;

            if (!hasShifted) return simpleVariant;

            // create a new alternative allele
            var rotatedSequence = combinedSequence.Substring(shiftStart, numBases);
            var seqToUpdate     = onReverseStrand ? SequenceUtilities.GetReverseComplement(rotatedSequence) : rotatedSequence;

            var rotatedRefAllele = simpleVariant.RefAllele;
            var rotatedAltAllele = simpleVariant.AltAllele;

            if (simpleVariant.Type == VariantType.insertion) rotatedAltAllele = seqToUpdate;
            else rotatedRefAllele = seqToUpdate;

            var rotatedStart = onReverseStrand
                ? simpleVariant.Start - shiftStart
                : simpleVariant.Start + shiftStart;

            var rotatedEnd = onReverseStrand
                ? simpleVariant.End - shiftStart
                : simpleVariant.End + shiftStart;

            return new SimpleVariant(simpleVariant.Chromosome, rotatedStart, rotatedEnd, rotatedRefAllele,
                rotatedAltAllele, simpleVariant.Type);
        }
    }
}