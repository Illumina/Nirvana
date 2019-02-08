using System.IO;
using Genome;
using SAUtils.CreateMitoMapDb;
using Variants;

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
        public (int RefPosition, string RefAllele, string AltAllele) LeftAlign(int refPosition, string refAllele, string altAllele, bool isCircularGenome = false)
        {
            var trimmedAllele = BiDirectionalTrimmer.Trim(refPosition, refAllele, altAllele);
            var trimmedPos = trimmedAllele.Start;
            var trimmedRefAllele = trimmedAllele.RefAllele;
            var trimmedAltAllele = trimmedAllele.AltAllele;

            // alignment only makes sense for insertion and deletion
            if (!(trimmedAltAllele.Length == 0 || trimmedRefAllele.Length == 0)) return (refPosition, refAllele, altAllele);

            var upstreamSeq = GetUpstreamSeq(trimmedPos, MaxRotationRange, isCircularGenome);
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
                return (trimmedPos, newRefAllele, ""); //alt is empty for deletion
            }

            //insertion
            combinedSeq += trimmedAltAllele;
            repeatLength = trimmedAltAllele.Length;

            for (i = combinedSeq.Length - 1; i >= repeatLength; i--, trimmedPos--)
            {
                if (combinedSeq[i] != combinedSeq[i - repeatLength]) break;
            }
            var newAltAllele = combinedSeq.Substring(i + 1 - repeatLength, repeatLength);
            return (trimmedPos, "", newAltAllele);
        }

        private string GetUpstreamSeq(int position, int length, bool isCircularGenome)
        {
            if (isCircularGenome)
            {
                var circularGenome = new CircularGenomeModel(_compressedSequence);
                var interval = (position - length, position -1);
                return circularGenome.ExtractIntervalSequence(interval);
            }

            var adjustedLength = length < position ? length : position - 1;
            return _compressedSequence.Substring(position - 1 - adjustedLength, adjustedLength);
        }
    }
}