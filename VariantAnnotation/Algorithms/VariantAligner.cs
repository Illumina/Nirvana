using System;
using System.IO;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Algorithms
{
    /// <summary>
    /// Provides methods for left or right aligning a varint. 
    /// NOTE: The alignment requires a rotation of the variant. Please check method for details
    /// </summary>
    public sealed class VariantAligner
    {
        private readonly ICompressedSequence _compressedSequence;
        private const int MaxRotationRange = 500;

        /// <summary>
        /// constructor
        /// </summary>
        public VariantAligner(ICompressedSequence compressedSequence)
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

        /// <summary>
        /// Right aligns the variant using base rotation
        /// </summary>
        /// <returns>Tuple of new position, ref and alt allele</returns>
        public Tuple<int, string, string> RightAlign(int refPosition, string refAllele, string altAllele)
        {
            var trimmedAllele = BiDirectionalTrimmer.Trim(refPosition, refAllele, altAllele);
            var trimmedPos = trimmedAllele.Item1;
            var trimmedRefAllele = trimmedAllele.Item2;
            var trimmedAltAllele = trimmedAllele.Item3;

            //alignment only makes sense for insertion and deletion
            if (!(trimmedAltAllele.Length == 0 || trimmedRefAllele.Length == 0)) return null;

            var downStreamSeq = GetDownstreamSeq(trimmedPos, MaxRotationRange);
            if (downStreamSeq == null)
                throw new InvalidDataException("Reference sequence not set, please check that it is loaded");

            string combinedSeq;
            int repeatLength;
            int i;
            if (trimmedRefAllele.Length > trimmedAltAllele.Length)
            {
                // deletion
                repeatLength = trimmedRefAllele.Length;
                combinedSeq = downStreamSeq;
                for (i = 0; i < combinedSeq.Length - repeatLength; i++, trimmedPos++)
                {
                    if (combinedSeq[i] != combinedSeq[i + repeatLength]) break;
                }
                var newRefAllele = combinedSeq.Substring(i, repeatLength);
                return Tuple.Create(trimmedPos, newRefAllele, ""); //alt is empty for deletion
            }
            else
            {
                //insertion
                combinedSeq = trimmedAltAllele + downStreamSeq;
                repeatLength = trimmedAltAllele.Length;

                for (i = 0; i < combinedSeq.Length - repeatLength; i++, trimmedPos++)
                {
                    if (combinedSeq[i] != combinedSeq[i + repeatLength]) break;
                }
                var newAltAllele = combinedSeq.Substring(i, repeatLength);
                return Tuple.Create(trimmedPos, "", newAltAllele);
            }

        }

        private string GetDownstreamSeq(int position, int length)
        {
            var adjustedLenght = length + position > _compressedSequence.NumBases
                ? _compressedSequence.NumBases - position
                : length;
            return _compressedSequence.Substring(position - 1, adjustedLenght); // compressed seq is 0 based
        }

        private string GetUpstreamSeq(int position, int length)
        {
            var adjustedLength = length < position ? length : position - 1;
            return _compressedSequence.Substring(position - 1 - adjustedLength, adjustedLength);
        }

        public sealed class ReferenceSequence : ICompressedSequence
        {
            public ChromosomeRenamer Renamer { get; }
            public ICytogeneticBands CytogeneticBands { get; set; }
            public GenomeAssembly GenomeAssembly { get; set; }

            private readonly string _sequence;
            private readonly int _offset;
            public int NumBases { get; }

            public ReferenceSequence(string s, int offset = 0, ChromosomeRenamer renamer = null)
            {
                _sequence = s;
                _offset   = offset;
                Renamer   = renamer;
                NumBases  = s.Length;
            }

            public void Set(int numBases, byte[] buffer, IIntervalSearch<MaskedEntry> maskedIntervalSearch, int sequenceOffset = 0)
            {
                throw new NotImplementedException();
            }

            public string Substring(int offset, int length)
            {
                return _sequence.Substring(offset - _offset, length);
            }
        }
    }
}
