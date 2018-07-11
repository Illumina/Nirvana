using System;

namespace Phantom.CodonInformation
{
    public sealed class CodingBlock : ICodingBlock, IEquatable<CodingBlock>
    {
        public int Start { get; }
        public int End { get; }
        public byte StartPhase { get; }

        public CodingBlock(int start, int end, byte startPhase)
        {
            Start      = start;
            End        = end;
            StartPhase = startPhase;
        }

        public bool Equals(CodingBlock other) =>
            Start == other.Start && End == other.End && StartPhase == other.StartPhase;

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Start;
                hashCode = (hashCode * 1201) ^ End;
                hashCode = (hashCode * 1201) ^ StartPhase;
                return hashCode;
            }
        }
    }
}