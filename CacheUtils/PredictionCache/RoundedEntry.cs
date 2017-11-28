using System;

namespace CacheUtils.PredictionCache
{
    public struct RoundedEntry : IEquatable<RoundedEntry>
    {
        public readonly ushort Score;
        public readonly byte EnumIndex;

        public RoundedEntry(ushort data)
        {
            Score     = Round((ushort)(data & 0x3ff));
            EnumIndex = (byte)((data & 0xc000) >> 14);
        }

        private static ushort Round(ushort us) => (ushort)((ushort)Math.Round(us / 5.0) * 5);

        public bool Equals(RoundedEntry other) => Score == other.Score && EnumIndex == other.EnumIndex;

        public override int GetHashCode()
        {
            unchecked { return (Score.GetHashCode() * 397) ^ EnumIndex.GetHashCode(); }
        }
    }
}
