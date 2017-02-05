using System;
using VariantAnnotation.FileHandling;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class LegacyExon : IEquatable<LegacyExon>, ICacheSerializable
    {
        #region members

        public readonly int Start;
        public readonly int End;
        private readonly byte? _phase; // 0, 1, 2

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public LegacyExon(int start, int end, byte? phase)
        {
            Start = start;
            End = end;
            _phase = phase;
        }

        #region IEquatable Overrides

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode() ^ _phase.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(LegacyExon value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return End == value.End && Start == value.Start && _phase == value._phase;
        }

        #endregion

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"{Start}\t{End}\t{_phase}";
        }

        /// <summary>
        /// writes the exon data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.WriteOpt(_phase);
        }
    }
}
