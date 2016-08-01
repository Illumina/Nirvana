using System;
using System.Globalization;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Compression;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public abstract class AbstractFunctionalPrediction
    {
        #region members

        //                                                 A   X  C  D  E  F  G  H  I   X  K  L   M   N   X   P   Q   R   S   T   X   V   W   X   Y   X
        private static readonly int[] AminoAcidIndices = { 0, -1, 1, 2, 3, 4, 5, 6, 7, -1, 8, 9, 10, 11, -1, 12, 13, 14, 15, 16, -1, 17, 18, -1, 19, -1 };

        public byte[] PredictionData;
        private bool _inCompressedState;

        private readonly QuickLZ _qlz;

        protected readonly int HashCode;

        private const ushort NullEntry      = 0xffff;
        private const ushort PredictionMask = 0xc000;
        private const ushort ScoreMask      = 0x03ff;
        private const int PredictionShift   = 14;
        private const int ScoreScaleFactor  = 1000;

        #endregion

        // constructor
        protected AbstractFunctionalPrediction(byte[] predictionData, bool inCompressedState)
        {
            PredictionData     = predictionData;
            _inCompressedState = inCompressedState;

            _qlz     = new QuickLZ();
            HashCode = FowlerNollVoPrimeHash.ComputeHash(PredictionData);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        /// <summary>
        /// Given an amino acid and a position, this method returns the appropriate byte array position
        /// </summary>
        private static int GetArrayPosition(char altAminoAcid, int aminoAcidPosition)
        {
            const int numBytesInShort = 2;
            const int numAminoAcids   = 20;

            int arrayIndex = char.ToUpper(altAminoAcid) - 'A';

            // sanity check: make sure the array index is within range
            if ((arrayIndex < 0) || (arrayIndex >= 26))
            {
                throw new IndexOutOfRangeException($"Expected an array index on the interval [0, 25], but observed the following: {arrayIndex} ({altAminoAcid})");
            }

            int aminoAcidIndex = AminoAcidIndices[arrayIndex];

            // sanity check: make sure the array index is within range
            if (aminoAcidIndex == -1)
            {
                throw new GeneralException($"An invalid amino acid was given: {altAminoAcid}");
            }

            int arrayPosition = numBytesInShort * (numAminoAcids * (aminoAcidPosition - 1) + aminoAcidIndex);

            return arrayPosition;
        }

        /// <summary>
        /// Given a numerical ID, this function returns the appropriate prediction string
        /// </summary>
        protected abstract string GetPredictionString(int id);

        /// <summary>
        /// Generates a prediction and a score for both the Sift and PolyPhen objects
        /// </summary>
        public void GetPrediction(char aminoAcid, int aminoAcidPosition, out string prediction, out string score)
        {
            prediction = null;
            score      = null;

            // sanity check: skip stop codons
            if (aminoAcid == AminoAcids.StopCodonChar || aminoAcid =='X') return;

            // uncompress our data
            if (_inCompressedState) UncompressPredictionData();

            // get the array position
            int arrayPosition = GetArrayPosition(aminoAcid, aminoAcidPosition);

            // sanity check: skip instances where the data isn't long enough
            if (arrayPosition >= PredictionData.Length) return;

            // get the entry and extract the prediction ID and the score
            ushort entry = BitConverter.ToUInt16(PredictionData, arrayPosition);

            // handle situations where the amino acid matches the reference
            if (entry == NullEntry) return;

            int scaledScore  = entry & ScoreMask;
            int predictionId = (entry & PredictionMask) >> PredictionShift;

            // sanity check: make sure the scaled score is not larger than 1000
            if(scaledScore > ScoreScaleFactor)
            {
                throw new IndexOutOfRangeException($"Expected the scaled score to be on the interval [0, 1000], but observed the following value: {scaledScore}");
            }

            // convert the score and prediction to strings
            prediction = GetPredictionString(predictionId);
            score      = (scaledScore / (double)ScoreScaleFactor).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// writes the functional prediction to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            var data               = PredictionData;
            int dataLength         = PredictionData.Length;
            bool inCompressedState = _inCompressedState;

            // try to compress the data
            if (!inCompressedState)
            {
                byte[] compressedData = null;
                var numCompressedBytes = _qlz.Compress(data, ref compressedData);

                if (numCompressedBytes < dataLength)
                {
                    data              = compressedData;
                    dataLength        = numCompressedBytes;
                    inCompressedState = true;
                }
            }

            writer.WriteInt(inCompressedState ? -dataLength : dataLength);
            writer.WriteBytes(data, 0, dataLength);
        }

        /// <summary>
        /// uncompresses the prediction data
        /// </summary>
        private void UncompressPredictionData()
        {
            byte[] qlzBytes = null;
            _qlz.Decompress(PredictionData, ref qlzBytes);

            PredictionData    = qlzBytes;
            _inCompressedState = false;
        }
    }
}
