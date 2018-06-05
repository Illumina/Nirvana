
using Genome;

namespace CacheUtils.TranscriptCache
{
    public sealed class NSequence : ISequence
    {
        public int Length { get; } = 1000;

        public string Substring(int offset, int length) => new string('N', length);
    }
}
