using System;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class PolyPhen : AbstractFunctionalPrediction, IEquatable<PolyPhen>
    {
        // constructor
        private PolyPhen(byte[] predictionData, bool inCompressedState) : base(predictionData, inCompressedState) {}

        #region Equality Overrides

        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to PolyPhen return false:
            var other = obj as PolyPhen;
            if (other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<PolyPhen>.Equals(PolyPhen other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(PolyPhen other)
        {
            return this == other;
        }

        public static bool operator ==(PolyPhen a, PolyPhen b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return a.PredictionData.SequenceEqual(b.PredictionData);
        }

        public static bool operator !=(PolyPhen a, PolyPhen b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// Given a numerical ID, this function returns the appropriate prediction string
        /// </summary>
        protected override string GetPredictionString(int id)
        {
            string ret;

            switch (id)
            {
                case 0:
                    ret = "probably damaging";
                    break;
                case 1:
                    ret = "possibly damaging";
                    break;
                case 2:
                    ret = "benign";
                    break;
                case 3:
                    ret = "unknown";
                    break;
                default:
                    throw new GeneralException($"Encountered an unknown PolyPhen prediction ID: {id}");
            }

            return ret;
        }

        /// <summary>
        /// reads the PolyPhen data from the binary reader
        /// </summary>
        public static PolyPhen Read(ExtendedBinaryReader reader)
        {
            var numCompressedBytes = reader.ReadInt();

            bool inCompressedState = numCompressedBytes < 0;
            if (inCompressedState) numCompressedBytes = -numCompressedBytes;

            var predictionData = reader.ReadBytes(numCompressedBytes);
            return new PolyPhen(predictionData, inCompressedState);
        }
    }
}
