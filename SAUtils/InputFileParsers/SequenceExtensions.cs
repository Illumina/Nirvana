using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers
{
    public static class SequenceExtensions
    {
        public static bool Validate( this ISequence referenceSequence, int start, int end, string testSequence)
        {
            var expSequence = referenceSequence.Substring(start - 1, end - start + 1);
            return testSequence == expSequence;

        }
    }
}