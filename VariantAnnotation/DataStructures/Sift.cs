using System;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class Sift : AbstractFunctionalPrediction, IEquatable<Sift>
    {
        // constructor
        private Sift(byte[] predictionData, bool inCompressedState) : base(predictionData, inCompressedState) {}

        #region Equality Overrides

        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Sift return false:
            var other = obj as Sift;
            if (other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Sift>.Equals(Sift other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Sift other)
        {
            return this == other;
        }

        public static bool operator ==(Sift a, Sift b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return a.PredictionData.SequenceEqual(b.PredictionData);
        }

        public static bool operator !=(Sift a, Sift b)
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
                    ret = "tolerated";
                    break;
                case 1:
                    ret = "deleterious";
                    break;
                case 2:
                    ret = "tolerated - low confidence";
                    break;
                case 3:
                    ret = "deleterious - low confidence";
                    break;
                default:
                    throw new GeneralException($"Encountered an unknown Sift prediction ID: {id}");
            }

            return ret;
        }

        /// <summary>
        /// reads the sift data from the binary reader
        /// </summary>
        public static Sift Read(ExtendedBinaryReader reader)
        {
            var numCompressedBytes = reader.ReadInt();

            bool inCompressedState = numCompressedBytes < 0;
            if (inCompressedState) numCompressedBytes = -numCompressedBytes;

            var predictionData = reader.ReadBytes(numCompressedBytes);
            return new Sift(predictionData, inCompressedState);
        }
    }
}
