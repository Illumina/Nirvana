using System;
using Genome;

namespace CacheUtils.TranscriptCache
{
    public sealed class NSequence : ISequence
    {
        public int Length => 1000;
        public string Substring(int offset, int length) => new string('N', length);
        public Band[] CytogeneticBands => null;
        public string Sequence         => throw new NotImplementedException();
    }
}