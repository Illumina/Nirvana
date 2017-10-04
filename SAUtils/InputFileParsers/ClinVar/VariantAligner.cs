using System;
using System.IO;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class VariantAligner
    {
        private readonly ISequence _compressedSequence;
        private const int MaxRotationRange = 500;

        /// <summary>
        /// constructor
        /// </summary>
        public VariantAligner(ISequence compressedSequence)
        {
            _compressedSequence = compressedSequence;
        }

        /// <summary>
        /// Left aligns the variant using base rotation
        /// </summary>
        /// <returns>Tuple of new position, ref and alt allele</returns>
        public Tuple<int, string, string> LeftAlign(int refPosition, string refAllele, string altAllele)
        {
            var trimmedAllele = BiDirectionalTrimmer.Trim(refPosition, refAllele, altAllele);
            var trimmedPos = trimmedAllele.Item1;
            var trimmedRefAllele = trimmedAllele.Item2;
            var trimmedAltAllele = trimmedAllele.Item3;

            // alignment only makes sense for insertion and deletion
            if (!(trimmedAltAllele.Length == 0 || trimmedRefAllele.Length == 0)) return null;

            var upstreamSeq = GetUpstreamSeq(trimmedPos, MaxRotationRange);
            if (upstreamSeq == null)
                throw new InvalidDataException("Reference sequence not set, please check that it is loaded");

            // compressed seq is 0 based
            var combinedSeq = upstreamSeq;
            int repeatLength;
            int i;
            if (trimmedRefAllele.Length > trimmedAltAllele.Length)
            {
                // deletion
                combinedSeq += trimmedRefAllele;
                repeatLength = trimmedRefAllele.Length;
                for (i = combinedSeq.Length - 1; i >= repeatLength; i--, trimmedPos--)
                {
                    if (combinedSeq[i] != combinedSeq[i - repeatLength]) break;
                }
                var newRefAllele = combinedSeq.Substring(i + 1 - repeatLength, repeatLength);
                return Tuple.Create(trimmedPos, newRefAllele, ""); //alt is empty for deletion
            }
            else
            {
                //insertion
                combinedSeq += trimmedAltAllele;
                repeatLength = trimmedAltAllele.Length;

                for (i = combinedSeq.Length - 1; i >= repeatLength; i--, trimmedPos--)
                {
                    if (combinedSeq[i] != combinedSeq[i - repeatLength]) break;
                }
                var newAltAllele = combinedSeq.Substring(i + 1 - repeatLength, repeatLength);
                return Tuple.Create(trimmedPos, "", newAltAllele);
            }

        }

        private string GetUpstreamSeq(int position, int length)
        {
            var adjustedLength = length < position ? length : position - 1;
            return _compressedSequence.Substring(position - 1 - adjustedLength, adjustedLength);
        }
    }
}