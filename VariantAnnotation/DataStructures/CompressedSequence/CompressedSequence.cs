using System;
using System.Collections.Generic;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.CompressedSequence
{
    public class CompressedSequence : ICompressedSequence
    {
        #region members

        public int NumBases { get; private set; }
        private byte[] _buffer;

        private IntervalTree<MaskedEntry> _maskedIntervalTree;

        private readonly char[] _convertNumberToBase;

        #endregion

        // constructor
        public CompressedSequence()
        {
            _maskedIntervalTree = new IntervalTree<MaskedEntry>();

            const string bases = "GCTA";
            _convertNumberToBase = bases.ToCharArray();
        }

        /// <summary>
        /// returns a tuple containing the base index and shift given a reference position
        /// </summary>
        private static Tuple<int, int> GetBaseIndexAndShift(int referencePosition)
        {
            int refPos    = referencePosition + 1;
            // ReSharper disable once SuggestUseVarKeywordEvident
            int baseIndex = (int)(refPos / 4.0);
            int shift     = (3 - refPos % 4) * 2;
            return new Tuple<int, int>(baseIndex, shift);
        }

        /// <summary>
        /// returns the number of bytes required in the buffer given the number of bases
        /// </summary>
        internal static int GetNumBufferBytes(int numBases)
        {
            return (int)((double)numBases / CompressedSequenceCommon.NumBasesPerByte + 1);
        }

        /// <summary>
        /// sets the underlying data for the compressed sequence
        /// </summary>
        public void Set(int numBases, byte[] buffer, IntervalTree<MaskedEntry> maskedIntervalTree)
        {
            NumBases            = numBases;
            _buffer             = buffer;
            _maskedIntervalTree = maskedIntervalTree;
        }

        /// <summary>
        /// decompresses the 2-bit notation into bases
        /// </summary>
        public string Substring(int offset, int length)
        {
            // handle negative offsets and lengths
            if ((offset < 0) || (length < 1) || (offset >= NumBases)) return null;

            // sanity check: avoid going past the end of the sequence
            if (offset + length > NumBases) length = NumBases - offset;

	        // allocate more memory if needed
            var decompressBuffer = new char[length];

            // set the initial state of the buffer
            var indexAndShiftTuple = GetBaseIndexAndShift(offset - 1);

            int bufferIndex = indexAndShiftTuple.Item1;
            int bufferShift = indexAndShiftTuple.Item2;
            byte currentBufferSeed = _buffer[bufferIndex];

            // get the overlapping masked interval
            var maskedIntervals = new List<MaskedEntry>();
            var overlapInterval = new IntervalTree<MaskedEntry>.Interval(string.Empty, offset, offset + length - 1);
            _maskedIntervalTree.GetAllOverlappingValues(overlapInterval, maskedIntervals);

            // get the first masked interval
            int numIntervals = maskedIntervals.Count;
            bool hasMaskedIntervals = numIntervals > 0;
            int currentOffset = 0;
            var currentInterval = hasMaskedIntervals ? maskedIntervals[0] : null;

            for (int baseIndex = 0; baseIndex < length; baseIndex++)
            {
                int currentPosition = offset + baseIndex;

                if (hasMaskedIntervals && (currentPosition >= currentInterval.Begin) && (currentPosition <= currentInterval.End))
                {
                    // evaluate the masked bases
                    for (; baseIndex <= currentInterval.End - offset && (baseIndex < length); baseIndex++) decompressBuffer[baseIndex] = 'N';
                    baseIndex--;

                    indexAndShiftTuple = GetBaseIndexAndShift(offset + baseIndex);

                    bufferIndex = indexAndShiftTuple.Item1;
                    bufferShift = indexAndShiftTuple.Item2;
                    currentBufferSeed = _buffer[bufferIndex];

                    currentOffset++;
                    hasMaskedIntervals = currentOffset < numIntervals;
                    currentInterval = hasMaskedIntervals ? maskedIntervals[currentOffset] : null;
                }
                else
                {
                    // evaluate normal bases
                    decompressBuffer[baseIndex] = _convertNumberToBase[(currentBufferSeed >> bufferShift) & 3];

                    bufferShift -= 2;

                    if (bufferShift < 0)
                    {
                        bufferShift = CompressedSequenceReader.MaxShift;
                        bufferIndex++;
                        currentBufferSeed = _buffer[bufferIndex];
                    }
                }
            }

            return new string(decompressBuffer, 0, length);
        }
    }
}
